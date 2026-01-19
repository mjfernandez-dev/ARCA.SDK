using System;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using ARCA.SDK.Configuration;
using ARCA.SDK.Exceptions;

namespace ARCA.SDK.Clients
{
    /// <summary>
    /// Cliente para el Web Service de Autenticación y Autorización (WSAA)
    /// </summary>
    internal class WsaaClient
    {
        private const string WSAA_URL_HOMOLOGACION = "https://wsaahomo.afip.gov.ar/ws/services/LoginCms";
        private const string WSAA_URL_PRODUCCION = "https://wsaa.afip.gov.ar/ws/services/LoginCms";

        private readonly string _endpointUrl;
        private readonly HttpClient _httpClient;

        public WsaaClient(ArcaEnvironment environment)
        {
            _endpointUrl = environment == ArcaEnvironment.Produccion
                ? WSAA_URL_PRODUCCION
                : WSAA_URL_HOMOLOGACION;

            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        /// <summary>
        /// Obtiene credenciales (Token y Sign) enviando el TRA firmado
        /// </summary>
        public async Task<(string Token, string Sign, DateTime Expiration)> LoginAsync(
            string signedTra,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Construir SOAP envelope
                var soapRequest = BuildSoapRequest(signedTra);

                // Crear HTTP request
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, _endpointUrl)
                {
                    Content = new StringContent(soapRequest, Encoding.UTF8, "text/xml")
                };

                httpRequest.Headers.Add("SOAPAction", "");

                // Enviar request
                var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);

                // Leer response
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new ArcaAuthException(
                        $"Error en la comunicación con WSAA. Status: {httpResponse.StatusCode}. " +
                        $"Response: {responseContent}"
                    );
                }

                // Parsear response
                return ParseSoapResponse(responseContent);
            }
            catch (Exception ex) when (!(ex is ArcaException))
            {
                throw new ArcaAuthException("Error al autenticar con WSAA", ex);
            }
        }

        private string BuildSoapRequest(string signedTra)
        {
            var soap = new StringBuilder();
            soap.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            soap.AppendLine("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:wsaa=\"http://wsaa.view.sua.dvadac.desein.afip.gov\">");
            soap.AppendLine("  <soapenv:Header/>");
            soap.AppendLine("  <soapenv:Body>");
            soap.AppendLine("    <wsaa:loginCms>");
            soap.AppendLine("      <wsaa:in0>");
            soap.Append(System.Security.SecurityElement.Escape(signedTra));
            soap.AppendLine();
            soap.AppendLine("      </wsaa:in0>");
            soap.AppendLine("    </wsaa:loginCms>");
            soap.AppendLine("  </soapenv:Body>");
            soap.AppendLine("</soapenv:Envelope>");

            return soap.ToString();
        }

        private (string Token, string Sign, DateTime Expiration) ParseSoapResponse(string soapResponse)
        {
            try
            {
                var doc = XDocument.Parse(soapResponse);
                var ns = XNamespace.Get("http://wsaa.view.sua.dvadac.desein.afip.gov");

                var loginReturn = doc.Descendants(ns + "loginCmsReturn").FirstOrDefault();

                if (loginReturn == null)
                {
                    // Buscar errores en la respuesta
                    var fault = doc.Descendants("faultstring").FirstOrDefault();
                    if (fault != null)
                    {
                        throw new ArcaAuthException($"WSAA retornó error: {fault.Value}");
                    }

                    throw new ArcaAuthException("Respuesta WSAA inválida: no se encontró loginCmsReturn");
                }

                var token = loginReturn.Element(ns + "token")?.Value;
                var sign = loginReturn.Element(ns + "sign")?.Value;
                var expirationStr = loginReturn.Element(ns + "expirationTime")?.Value;

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(sign))
                {
                    throw new ArcaAuthException("WSAA no retornó Token o Sign");
                }

                DateTime expiration = DateTime.MinValue;
                if (!string.IsNullOrEmpty(expirationStr))
                {
                    // El formato puede ser: 2026-01-19T12:30:00.000-03:00
                    DateTime.TryParse(expirationStr, out expiration);
                }

                return (token, sign, expiration);
            }
            catch (Exception ex) when (!(ex is ArcaException))
            {
                throw new ArcaAuthException("Error al parsear la respuesta del WSAA", ex);
            }
        }
    }
}