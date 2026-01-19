using System;
using System.Threading.Tasks;
using ARCA.SDK.Configuration;
using ARCA.SDK.Exceptions;
using ARCA.SDK.Services;
using Xunit;

namespace ARCA.SDK.Tests
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task ObtenerCredenciales_SinCertificado_LanzaExcepcion()
        {
            // Arrange
            var config = new ArcaConfig
            {
                Cuit = 20123456789,
                Environment = ArcaEnvironment.Homologacion,
                CertificatePath = "cert_inexistente.pfx"
            };

            var authService = new AuthService(config);

            // Act & Assert
            await Assert.ThrowsAsync<ArcaException>(async () =>
            {
                await authService.ObtenerCredencialesAsync("wsfe");
            });
        }

        [Fact]
        public void LimpiarCache_NoLanzaExcepcion()
        {
            // Arrange
            var config = new ArcaConfig
            {
                Cuit = 20123456789,
                Environment = ArcaEnvironment.Homologacion,
                CertificatePath = "cert.pfx"
            };

            var authService = new AuthService(config);

            // Act & Assert (no debe lanzar excepción)
            authService.LimpiarCache();
        }
    }
}