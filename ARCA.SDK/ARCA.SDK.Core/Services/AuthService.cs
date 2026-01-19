using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using ARCA.SDK.Clients;
using ARCA.SDK.Configuration;
using ARCA.SDK.Exceptions;
using ARCA.SDK.Utils;

namespace ARCA.SDK.Services
{
    /// <summary>
    /// Servicio de autenticación con ARCA (WSAA)
    /// </summary>
    internal class AuthService
    {
        private readonly ArcaConfig _config;
        private readonly WsaaClient _wsaaClient;
        private readonly AuthCache _cache;
        private X509Certificate2? _certificate;

        public AuthService(ArcaConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _wsaaClient = new WsaaClient(_config.Environment);
            _cache = new AuthCache();
        }

        /// <summary>
        /// Obtiene token y sign de autenticación (con caché)
        /// </summary>
        /// <param name="servicio">Nombre del servicio (ej: "wsfe", "wsfex")</param>
        public async Task<(string Token, string Sign)> ObtenerCredencialesAsync(
            string servicio,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(servicio))
                throw new ArgumentException("El nombre del servicio es requerido", nameof(servicio));

            // Generar clave de caché única por CUIT + Servicio + Ambiente
            var cacheKey = $"{_config.Cuit}_{servicio}_{_config.Environment}";

            // Intentar obtener del caché
            var cached = _cache.Get(cacheKey);
            if (cached.HasValue)
            {
                return cached.Value;
            }

            // No está en caché o expiró, autenticar de nuevo
            return await AutenticarAsync(servicio, cacheKey, cancellationToken);
        }

        private async Task<(string Token, string Sign)> AutenticarAsync(
            string servicio,
            string cacheKey,
            CancellationToken cancellationToken)
        {
            try
            {
                // 1. Cargar certificado si aún no está cargado
                if (_certificate == null)
                {
                    _certificate = CargarCertificado();
                    CertificateHelper.ValidateCertificate(_certificate);
                }

                // 2. Generar TRA (LoginTicketRequest) firmado
                var tra = LoginTicketRequest.Generate(servicio, _config.Cuit, _certificate);

                // 3. Enviar al WSAA y obtener credenciales
                var (token, sign, expiration) = await _wsaaClient.LoginAsync(tra, cancellationToken);

                // 4. Guardar en caché
                _cache.Set(cacheKey, token, sign, expiration);

                return (token, sign);
            }
            catch (Exception ex) when (!(ex is ArcaException))
            {
                throw new ArcaAuthException($"Error al autenticar servicio '{servicio}'", ex);
            }
        }

        private X509Certificate2 CargarCertificado()
        {
            if (string.IsNullOrEmpty(_config.CertificatePath))
                throw new ArcaAuthException("No se especificó la ruta del certificado");

            return CertificateHelper.LoadCertificate(
                _config.CertificatePath,
                _config.PrivateKeyPath,
                _config.CertificatePassword
            );
        }

        /// <summary>
        /// Limpia el caché de autenticación
        /// </summary>
        public void LimpiarCache()
        {
            _cache.Clear();
            _certificate?.Dispose();
            _certificate = null;
        }
    }
}