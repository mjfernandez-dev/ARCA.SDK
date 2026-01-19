using ARCA.SDK.Configuration;
using ARCA.SDK.Models;
using ARCA.SDK.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ARCA.SDK
{
    /// <summary>
    /// Cliente principal para interactuar con los servicios de ARCA
    /// </summary>
    public class ArcaClient
    {
        private readonly ArcaConfig _config;

        /// <summary>
        /// Crea una nueva instancia del cliente ARCA
        /// </summary>
        /// <param name="config">Configuración del cliente</param>
        public ArcaClient(ArcaConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            ValidarConfiguracion();
        }

        /// <summary>
        /// Autoriza un comprobante ante ARCA
        /// </summary>
        /// <param name="comprobante">Datos del comprobante a autorizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la autorización con CAE</returns>
        public async Task<AutorizacionResult> AutorizarComprobanteAsync(
            Comprobante comprobante,
            CancellationToken cancellationToken = default)
        {
            if (comprobante == null)
                throw new ArgumentNullException(nameof(comprobante));

            // TODO: Implementar lógica de autorización
            throw new NotImplementedException("Próximamente implementaremos la autorización");
        }

        /// <summary>
        /// Obtiene el último número de comprobante autorizado
        /// </summary>
        /// <param name="puntoVenta">Punto de venta</param>
        /// <param name="tipoComprobante">Tipo de comprobante</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Último número de comprobante</returns>
        public async Task<long> ObtenerUltimoComprobanteAsync(
            int puntoVenta,
            int tipoComprobante,
            CancellationToken cancellationToken = default)
        {
            // TODO: Implementar consulta de último comprobante
            throw new NotImplementedException("Próximamente implementaremos la consulta");
        }

        /// <summary>
        /// Consulta la cotización de una moneda
        /// </summary>
        /// <param name="monedaId">Código de moneda (DOL, 060, etc.)</param>
        /// <param name="fecha">Fecha de cotización (null = hoy)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Cotización de la moneda</returns>
        public async Task<decimal> ConsultarCotizacionAsync(
            string monedaId,
            DateTime? fecha = null,
            CancellationToken cancellationToken = default)
        {
            // TODO: Implementar consulta de cotización
            throw new NotImplementedException("Próximamente implementaremos la consulta de cotización");
        }

        private void ValidarConfiguracion()
        {
            if (_config.Cuit <= 0)
                throw new ArcaValidationException("El CUIT es requerido");

            if (string.IsNullOrEmpty(_config.CertificatePath))
                throw new ArcaValidationException("La ruta del certificado es requerida");

            // PrivateKeyPath solo es obligatorio si el certificado NO es .pfx/.p12
            var isPfx = _config.CertificatePath.EndsWith(".pfx", StringComparison.OrdinalIgnoreCase) ||
                        _config.CertificatePath.EndsWith(".p12", StringComparison.OrdinalIgnoreCase);

            if (!isPfx && string.IsNullOrEmpty(_config.PrivateKeyPath))
            {
                throw new ArcaValidationException(
                    "La ruta de la clave privada es requerida cuando no se usa .pfx. " +
                    "Use un archivo .pfx/.p12 o proporcione la ruta al archivo .key"
                );
            }
        }
    }
}