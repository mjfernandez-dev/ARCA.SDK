using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using ARCA.SDK.Configuration;
using ARCA.SDK.Exceptions;

namespace ARCA.SDK.Clients
{
    /// <summary>
    /// Cliente para el Web Service de Facturación Electrónica (WSFE v1)
    /// </summary>
    internal class WsfeClient
    {
        private const string WSFE_URL_HOMOLOGACION = "https://wswhomo.afip.gov.ar/wsfev1/service.asmx";
        private const string WSFE_URL_PRODUCCION = "https://servicios1.afip.gov.ar/wsfev1/service.asmx";

        private readonly string _endpointUrl;
        private readonly HttpClient _httpClient;

        public WsfeClient(ArcaEnvironment environment)
        {
            _endpointUrl = environment == ArcaEnvironment.Produccion
                ? WSFE_URL_PRODUCCION
                : WSFE_URL_HOMOLOGACION;

            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        /// <summary>
        /// Obtiene el último número de comprobante autorizado
        /// </summary>
        public async Task<long> FECompUltimoAutorizadoAsync(
            WsfeAuth auth,
            int puntoVenta,
            int tipoComprobante,
            CancellationToken cancellationToken = default)
        {
            var soapRequest = BuildUltimoComprobanteRequest(auth, puntoVenta, tipoComprobante);

            var response = await SendSoapRequestAsync(
                soapRequest,
                "http://ar.gov.afip.dif.FEV1/FECompUltimoAutorizado",
                cancellationToken
            );

            return ParseUltimoComprobanteResponse(response);
        }

        /// <summary>
        /// Solicita CAE para uno o más comprobantes
        /// </summary>
        public async Task<WsfeCAEResponse> FECAESolicitarAsync(
            WsfeAuth auth,
            int puntoVenta,
            int tipoComprobante,
            WsfeComprobante[] comprobantes,
            CancellationToken cancellationToken = default)
        {
            var soapRequest = BuildCAERequest(auth, puntoVenta, tipoComprobante, comprobantes);

            var response = await SendSoapRequestAsync(
                soapRequest,
                "http://ar.gov.afip.dif.FEV1/FECAESolicitar",
                cancellationToken
            );

            return ParseCAEResponse(response);
        }

        private async Task<string> SendSoapRequestAsync(
            string soapRequest,
            string soapAction,
            CancellationToken cancellationToken)
                {
                    try
                    {
                        var httpRequest = new HttpRequestMessage(HttpMethod.Post, _endpointUrl)
                        {
                            Content = new StringContent(soapRequest, Encoding.UTF8, "text/xml")
                        };

                        httpRequest.Headers.Add("SOAPAction", soapAction);

                        var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
                        var responseContent = await httpResponse.Content.ReadAsStringAsync();

                        // 👇 AGREGAR ESTAS LÍNEAS DE LOGGING
                        Console.WriteLine("=== REQUEST WSFE ===");
                        Console.WriteLine(soapRequest);
                        Console.WriteLine("===================");
                        Console.WriteLine("=== RESPONSE WSFE ===");
                        Console.WriteLine(responseContent);
                        Console.WriteLine("=====================\n");

                        if (!httpResponse.IsSuccessStatusCode)
                        {
                            throw new ArcaException(
                                $"Error en comunicación con WSFE. Status: {httpResponse.StatusCode}"
                            );
                        }

                        return responseContent;
                    }
                    catch (Exception ex) when (!(ex is ArcaException))
                    {
                        throw new ArcaException("Error al comunicarse con WSFE", ex);
                    }
                }

        private string BuildUltimoComprobanteRequest(WsfeAuth auth, int puntoVenta, int tipoComprobante)
        {
            var soap = new StringBuilder();
            soap.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            soap.AppendLine("<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:ar=\"http://ar.gov.afip.dif.FEV1/\">");
            soap.AppendLine("  <soap:Body>");
            soap.AppendLine("    <ar:FECompUltimoAutorizado>");
            soap.AppendLine("      <ar:Auth>");
            soap.AppendLine($"        <ar:Token>{auth.Token}</ar:Token>");
            soap.AppendLine($"        <ar:Sign>{auth.Sign}</ar:Sign>");
            soap.AppendLine($"        <ar:Cuit>{auth.Cuit}</ar:Cuit>");
            soap.AppendLine("      </ar:Auth>");
            soap.AppendLine($"      <ar:PtoVta>{puntoVenta}</ar:PtoVta>");
            soap.AppendLine($"      <ar:CbteTipo>{tipoComprobante}</ar:CbteTipo>");
            soap.AppendLine("    </ar:FECompUltimoAutorizado>");
            soap.AppendLine("  </soap:Body>");
            soap.AppendLine("</soap:Envelope>");

            return soap.ToString();
        }

        private string BuildCAERequest(
            WsfeAuth auth,
            int puntoVenta,
            int tipoComprobante,
            WsfeComprobante[] comprobantes)
        {
            var soap = new StringBuilder();
            soap.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            soap.AppendLine("<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:ar=\"http://ar.gov.afip.dif.FEV1/\">");
            soap.AppendLine("  <soap:Body>");
            soap.AppendLine("    <ar:FECAESolicitar>");
            soap.AppendLine("      <ar:Auth>");
            soap.AppendLine($"        <ar:Token>{auth.Token}</ar:Token>");
            soap.AppendLine($"        <ar:Sign>{auth.Sign}</ar:Sign>");
            soap.AppendLine($"        <ar:Cuit>{auth.Cuit}</ar:Cuit>");
            soap.AppendLine("      </ar:Auth>");
            soap.AppendLine("      <ar:FeCAEReq>");
            soap.AppendLine("        <ar:FeCabReq>");
            soap.AppendLine($"          <ar:CantReg>{comprobantes.Length}</ar:CantReg>");
            soap.AppendLine($"          <ar:PtoVta>{puntoVenta}</ar:PtoVta>");
            soap.AppendLine($"          <ar:CbteTipo>{tipoComprobante}</ar:CbteTipo>");
            soap.AppendLine("        </ar:FeCabReq>");

            // Detalle de comprobantes
            soap.AppendLine("        <ar:FeDetReq>");
            foreach (var comp in comprobantes)
            {
                AppendComprobante(soap, comp);
            }
            soap.AppendLine("        </ar:FeDetReq>");

            soap.AppendLine("      </ar:FeCAEReq>");
            soap.AppendLine("    </ar:FECAESolicitar>");
            soap.AppendLine("  </soap:Body>");
            soap.AppendLine("</soap:Envelope>");

            return soap.ToString();
        }

        private void AppendComprobante(StringBuilder soap, WsfeComprobante comp)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture; // 👈 AGREGAR ESTA LÍNEA AL INICIO

            soap.AppendLine("          <ar:FECAEDetRequest>");
            soap.AppendLine($"            <ar:Concepto>{comp.Concepto}</ar:Concepto>");
            soap.AppendLine($"            <ar:DocTipo>{comp.DocTipo}</ar:DocTipo>");
            soap.AppendLine($"            <ar:DocNro>{comp.DocNro}</ar:DocNro>");
            soap.AppendLine($"            <ar:CbteDesde>{comp.CbteDesde}</ar:CbteDesde>");
            soap.AppendLine($"            <ar:CbteHasta>{comp.CbteHasta}</ar:CbteHasta>");
            soap.AppendLine($"            <ar:CbteFch>{comp.CbteFch}</ar:CbteFch>");
            soap.AppendLine($"            <ar:ImpTotal>{comp.ImpTotal.ToString("F2", inv)}</ar:ImpTotal>");
            soap.AppendLine($"            <ar:ImpTotConc>{comp.ImpTotConc.ToString("F2", inv)}</ar:ImpTotConc>");
            soap.AppendLine($"            <ar:ImpNeto>{comp.ImpNeto.ToString("F2", inv)}</ar:ImpNeto>");
            soap.AppendLine($"            <ar:ImpOpEx>{comp.ImpOpEx.ToString("F2", inv)}</ar:ImpOpEx>");
            soap.AppendLine($"            <ar:ImpIVA>{comp.ImpIVA.ToString("F2", inv)}</ar:ImpIVA>");
            soap.AppendLine($"            <ar:ImpTrib>{comp.ImpTrib.ToString("F2", inv)}</ar:ImpTrib>");
            soap.AppendLine($"            <ar:MonId>{comp.MonId}</ar:MonId>");
            soap.AppendLine($"            <ar:MonCotiz>{comp.MonCotiz.ToString("F6", inv)}</ar:MonCotiz>");
            soap.AppendLine($"            <ar:CondicionIVAReceptorId>{comp.CondicionIVAReceptor}</ar:CondicionIVAReceptorId>"); ;

            // Campos opcionales
            if (!string.IsNullOrEmpty(comp.FchServDesde))
                soap.AppendLine($"            <ar:FchServDesde>{comp.FchServDesde}</ar:FchServDesde>");
            if (!string.IsNullOrEmpty(comp.FchServHasta))
                soap.AppendLine($"            <ar:FchServHasta>{comp.FchServHasta}</ar:FchServHasta>");
            if (!string.IsNullOrEmpty(comp.FchVtoPago))
                soap.AppendLine($"            <ar:FchVtoPago>{comp.FchVtoPago}</ar:FchVtoPago>");

            // IVA
            if (comp.Iva != null && comp.Iva.Length > 0)
            {
                soap.AppendLine("            <ar:Iva>");
                foreach (var iva in comp.Iva)
                {
                    soap.AppendLine("              <ar:AlicIva>");
                    soap.AppendLine($"                <ar:Id>{iva.Id}</ar:Id>");
                    soap.AppendLine($"                <ar:BaseImp>{iva.BaseImp.ToString("F2", inv)}</ar:BaseImp>");
                    soap.AppendLine($"                <ar:Importe>{iva.Importe.ToString("F2", inv)}</ar:Importe>");
                    soap.AppendLine("              </ar:AlicIva>");
                }
                soap.AppendLine("            </ar:Iva>");
            }

            soap.AppendLine("          </ar:FECAEDetRequest>");
        }

        private long ParseUltimoComprobanteResponse(string soapResponse)
        {
            try
            {
                var doc = XDocument.Parse(soapResponse);
                var ns = XNamespace.Get("http://ar.gov.afip.dif.FEV1/");

                var cbteNro = doc.Descendants(ns + "CbteNro").FirstOrDefault();

                if (cbteNro != null && long.TryParse(cbteNro.Value, out long numero))
                {
                    return numero;
                }

                // Buscar errores
                var error = doc.Descendants(ns + "Err").FirstOrDefault();
                if (error != null)
                {
                    var code = error.Element(ns + "Code")?.Value;
                    var msg = error.Element(ns + "Msg")?.Value;
                    throw new ArcaException($"WSFE Error {code}: {msg}");
                }

                return 0;
            }
            catch (Exception ex) when (!(ex is ArcaException))
            {
                throw new ArcaException("Error al parsear respuesta de WSFE", ex);
            }
        }

        private WsfeCAEResponse ParseCAEResponse(string soapResponse)
        {
            try
            {
                var doc = XDocument.Parse(soapResponse);
                var ns = XNamespace.Get("http://ar.gov.afip.dif.FEV1/");

                var response = new WsfeCAEResponse();

                // Parsear detalles de respuesta
                var detResp = doc.Descendants(ns + "FECAEDetResponse").ToArray();
                if (detResp.Length > 0)
                {
                    response.FeDetResp = detResp.Select(det => new WsfeResultado
                    {
                        Resultado = det.Element(ns + "Resultado")?.Value,
                        CAE = det.Element(ns + "CAE")?.Value,
                        CAEFchVto = det.Element(ns + "CAEFchVto")?.Value,
                        CbteDesde = long.TryParse(det.Element(ns + "CbteDesde")?.Value, out long cbte) ? cbte : null
                    }).ToArray();
                }

                // Parsear errores
                var errors = doc.Descendants(ns + "Err").ToArray();
                if (errors.Length > 0)
                {
                    response.Errors = errors.Select(err => new WsfeError
                    {
                        Code = int.TryParse(err.Element(ns + "Code")?.Value, out int code) ? code : 0,
                        Msg = err.Element(ns + "Msg")?.Value
                    }).ToArray();
                }

                return response;
            }
            catch (Exception ex) when (!(ex is ArcaException))
            {
                throw new ArcaException("Error al parsear respuesta CAE de WSFE", ex);
            }
        }
    }
}