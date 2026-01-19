using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ARCA.SDK.Clients;
using ARCA.SDK.Configuration;
using ARCA.SDK.Exceptions;
using ARCA.SDK.Models;

namespace ARCA.SDK.Services
{
    /// <summary>
    /// Servicio de facturación electrónica (WSFE)
    /// </summary>
    internal class FacturacionService
    {
        private readonly ArcaConfig _config;
        private readonly AuthService _authService;
        private readonly WsfeClient _wsfeClient;

        public FacturacionService(ArcaConfig config, AuthService authService)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _wsfeClient = new WsfeClient(_config.Environment);
        }

        /// <summary>
        /// Autoriza un comprobante ante ARCA
        /// </summary>
        public async Task<AutorizacionResult> AutorizarComprobanteAsync(
            Comprobante comprobante,
            CancellationToken cancellationToken = default)
        {
            if (comprobante == null)
                throw new ArgumentNullException(nameof(comprobante));

            ValidarComprobante(comprobante);

            try
            {
                // 1. Obtener credenciales de autenticación
                var (token, sign) = await _authService.ObtenerCredencialesAsync("wsfe", cancellationToken);

                var auth = new WsfeAuth
                {
                    Token = token,
                    Sign = sign,
                    Cuit = _config.Cuit
                };

                // 2. Convertir modelo del SDK a modelo WSFE
                var wsfeComprobante = ConvertirAWsfeComprobante(comprobante);

                // 3. Solicitar CAE
                var response = await _wsfeClient.FECAESolicitarAsync(
                    auth,
                    comprobante.PuntoVenta,
                    comprobante.TipoComprobante,
                    new[] { wsfeComprobante },
                    cancellationToken
                );

                // 4. Procesar respuesta
                return ProcesarRespuestaCAE(response, comprobante.Numero);
            }
            catch (Exception ex) when (!(ex is ArcaException))
            {
                throw new ArcaException("Error al autorizar comprobante", ex);
            }
        }

        /// <summary>
        /// Obtiene el último comprobante autorizado
        /// </summary>
        public async Task<long> ObtenerUltimoComprobanteAsync(
            int puntoVenta,
            int tipoComprobante,
            CancellationToken cancellationToken = default)
        {
            if (puntoVenta <= 0)
                throw new ArcaValidationException("El punto de venta debe ser mayor a 0");

            if (tipoComprobante <= 0)
                throw new ArcaValidationException("El tipo de comprobante debe ser mayor a 0");

            try
            {
                // Obtener credenciales
                var (token, sign) = await _authService.ObtenerCredencialesAsync("wsfe", cancellationToken);

                var auth = new WsfeAuth
                {
                    Token = token,
                    Sign = sign,
                    Cuit = _config.Cuit
                };

                // Consultar último comprobante
                return await _wsfeClient.FECompUltimoAutorizadoAsync(
                    auth,
                    puntoVenta,
                    tipoComprobante,
                    cancellationToken
                );
            }
            catch (Exception ex) when (!(ex is ArcaException))
            {
                throw new ArcaException("Error al consultar último comprobante", ex);
            }
        }

        private WsfeComprobante ConvertirAWsfeComprobante(Comprobante comprobante)
        {
            var wsfe = new WsfeComprobante
            {
                Concepto = comprobante.Concepto,
                DocTipo = comprobante.TipoDocumento,
                DocNro = comprobante.NumeroDocumento,
                CbteDesde = comprobante.Numero,
                CbteHasta = comprobante.Numero,
                CbteFch = comprobante.FechaEmision.ToString("yyyyMMdd"),
                ImpTotal = comprobante.ImporteTotal,
                ImpTotConc = comprobante.ImporteNoGravado,
                ImpNeto = comprobante.ImporteNeto,
                ImpOpEx = comprobante.ImporteExento,
                ImpIVA = comprobante.ImporteIVA,
                ImpTrib = comprobante.ImporteTributos,
                MonId = comprobante.MonedaId,
                MonCotiz = comprobante.MonedaCotizacion
            };

            // Fechas de servicio (si aplica)
            if (comprobante.FechaServicioDesde.HasValue)
                wsfe.FchServDesde = comprobante.FechaServicioDesde.Value.ToString("yyyyMMdd");

            if (comprobante.FechaServicioHasta.HasValue)
                wsfe.FchServHasta = comprobante.FechaServicioHasta.Value.ToString("yyyyMMdd");

            if (comprobante.FechaVencimientoPago.HasValue)
                wsfe.FchVtoPago = comprobante.FechaVencimientoPago.Value.ToString("yyyyMMdd");

            // Alícuotas IVA
            if (comprobante.AlicuotasIVA.Count > 0)
            {
                wsfe.Iva = comprobante.AlicuotasIVA.Select(a => new WsfeAlicuotaIVA
                {
                    Id = a.Codigo,
                    BaseImp = a.BaseImponible,
                    Importe = a.Importe
                }).ToArray();
            }

            // Tributos
            if (comprobante.Tributos.Count > 0)
            {
                wsfe.Tributos = comprobante.Tributos.Select(t => new WsfeTributo
                {
                    Id = t.Codigo,
                    Desc = t.Descripcion,
                    BaseImp = t.BaseImponible,
                    Alic = t.Alicuota,
                    Importe = t.Importe
                }).ToArray();
            }

            // Comprobantes asociados
            if (comprobante.ComprobantesAsociados.Count > 0)
            {
                wsfe.CbtesAsoc = comprobante.ComprobantesAsociados.Select(c => new WsfeComprobanteAsociado
                {
                    Tipo = c.Tipo,
                    PtoVta = c.PuntoVenta,
                    Nro = c.Numero,
                    Cuit = c.Cuit
                }).ToArray();
            }

            return wsfe;
        }

        private AutorizacionResult ProcesarRespuestaCAE(WsfeCAEResponse response, long numeroComprobante)
        {
            var result = new AutorizacionResult();

            // Verificar errores
            if (response.Errors != null && response.Errors.Length > 0)
            {
                var error = response.Errors[0];
                result.Exitoso = false;
                result.CodigoError = error.Code.ToString();
                result.MensajeError = error.Msg;
                return result;
            }

            // Verificar detalles de respuesta
            if (response.FeDetResp == null || response.FeDetResp.Length == 0)
            {
                result.Exitoso = false;
                result.MensajeError = "WSFE no retornó detalles de respuesta";
                return result;
            }

            var detalle = response.FeDetResp[0];

            // Verificar resultado
            if (detalle.Resultado == "A")
            {
                result.Exitoso = true;
                result.CAE = detalle.CAE;
                result.NumeroComprobante = detalle.CbteDesde ?? numeroComprobante;

                // Parsear fecha vencimiento CAE (formato: YYYYMMDD)
                if (!string.IsNullOrEmpty(detalle.CAEFchVto) && detalle.CAEFchVto.Length == 8)
                {
                    if (DateTime.TryParseExact(
                        detalle.CAEFchVto,
                        "yyyyMMdd",
                        null,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime fecha))
                    {
                        result.FechaVencimientoCAE = fecha;
                    }
                }

                // Observaciones
                if (detalle.Observaciones != null && detalle.Observaciones.Length > 0)
                {
                    result.Observaciones = detalle.Observaciones
                        .Select(o => $"[{o.Code}] {o.Msg}")
                        .ToList();
                }
            }
            else
            {
                result.Exitoso = false;
                result.MensajeError = detalle.Resultado == "R"
                    ? "Comprobante rechazado"
                    : "Comprobante observado";

                if (detalle.Observaciones != null && detalle.Observaciones.Length > 0)
                {
                    result.Observaciones = detalle.Observaciones
                        .Select(o => $"[{o.Code}] {o.Msg}")
                        .ToList();

                    result.MensajeError += ": " + string.Join("; ", result.Observaciones);
                }
            }

            return result;
        }

        private void ValidarComprobante(Comprobante comprobante)
        {
            if (comprobante.PuntoVenta <= 0)
                throw new ArcaValidationException("El punto de venta debe ser mayor a 0");

            if (comprobante.TipoComprobante <= 0)
                throw new ArcaValidationException("El tipo de comprobante debe ser mayor a 0");

            if (comprobante.Numero <= 0)
                throw new ArcaValidationException("El número de comprobante debe ser mayor a 0");

            if (comprobante.ImporteTotal <= 0)
                throw new ArcaValidationException("El importe total debe ser mayor a 0");

            // Validar fechas de servicio si el concepto lo requiere
            if (comprobante.Concepto == 2 || comprobante.Concepto == 3)
            {
                if (!comprobante.FechaServicioDesde.HasValue)
                    throw new ArcaValidationException("Fecha de inicio de servicio es requerida para concepto de servicios");

                if (!comprobante.FechaServicioHasta.HasValue)
                    throw new ArcaValidationException("Fecha de fin de servicio es requerida para concepto de servicios");

                if (!comprobante.FechaVencimientoPago.HasValue)
                    throw new ArcaValidationException("Fecha de vencimiento de pago es requerida para concepto de servicios");
            }
        }
    }
}