using ARCA.SDK.Configuration;
using ARCA.SDK.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ARCA.SDK.Services
{
    /// <summary>
    /// Servicio de autenticación con ARCA (WSAA)
    /// </summary>
    internal class AuthService
    {
        private readonly ArcaConfig _config;

        public AuthService(ArcaConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene token y sign de autenticación
        /// </summary>
        public async Task<(string Token, string Sign)> ObtenerCredencialesAsync(
            string servicio,
            CancellationToken cancellationToken = default)
        {
            // TODO: Implementar autenticación WSAA
            // Por ahora retornamos valores de prueba
            await Task.CompletedTask;

            throw new NotImplementedException(
                "Autenticación WSAA próximamente. " +
                "Necesitamos generar el LoginTicketRequest, firmarlo con el certificado, " +
                "y enviarlo al WSAA para obtener Token y Sign"
            );
        }
    }
}