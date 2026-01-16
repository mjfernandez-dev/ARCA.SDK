using ARCA.SDK;
using ARCA.SDK.Configuration;
using ARCA.SDK.Exceptions;
using Xunit;

namespace ARCA.SDK.Tests
{
    public class ArcaClientTests
    {
        [Fact]
        public void Create_ConConfiguracionValida_CreaClienteExitosamente()
        {
            // Arrange & Act
            var client = ArcaClientFactory.Create(config =>
            {
                config.Environment = ArcaEnvironment.Homologacion;
                config.Cuit = 20123456789;
                config.CertificatePath = "cert.crt";
                config.PrivateKeyPath = "key.key";
            });

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public void Create_SinCuit_LanzaExcepcion()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArcaValidationException>(() =>
            {
                ArcaClientFactory.Create(config =>
                {
                    config.CertificatePath = "cert.crt";
                    config.PrivateKeyPath = "key.key";
                    // No configuramos CUIT
                });
            });
        }

        [Fact]
        public void Create_SinCertificado_LanzaExcepcion()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArcaValidationException>(() =>
            {
                ArcaClientFactory.Create(config =>
                {
                    config.Cuit = 20123456789;
                    config.PrivateKeyPath = "key.key";
                    // No configuramos certificado
                });
            });
        }
    }
}