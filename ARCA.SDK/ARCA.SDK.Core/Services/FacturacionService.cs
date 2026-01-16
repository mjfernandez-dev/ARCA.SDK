using ARCA.SDK.Configuration;
using ARCA.SDK.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ARCA.SDK.Services
{
    /// <summary>
    /// Servicio de facturación electrónica (WSFE)
    /// </summary>
    internal class FacturacionService
    {
        private readonly ArcaConfig _config;
        private readonly AuthService _authService;

        public FacturacionService(ArcaConfig config, AuthService authService)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        /// <summary>
        /// Autoriza un comprobante ante ARCA
        /// </summary>
        public async Task<AutorizacionResult> AutorizarComprobanteAsync(
            Comprobante comprobante,
            CancellationToken cancellationToken = default)
        {
            // TODO: Implementar autorización WSFE
            await Task.CompletedTask;

            throw new NotImplementedException(
                "Autorización WSFE próximamente. " +
                "Necesitamos llamar al método FECAESolicitar con los datos del comprobante"
            );
        }

        /// <summary>
        /// Obtiene el último comprobante autorizado
        /// </summary>
        public async Task<long> ObtenerUltimoComprobanteAsync(
            int puntoVenta,
            int tipoComprobante,
            CancellationToken cancellationToken = default)
        {
            // TODO: Implementar consulta
            await Task.CompletedTask;

            throw new NotImplementedException(
                "Consulta de último comprobante próximamente. " +
                "Necesitamos llamar al método FECompUltimoAutorizado"
            );
        }
    }
}