namespace ARCA.SDK.Configuration
{
    /// <summary>
    /// Configuración principal del cliente ARCA
    /// </summary>
    public class ArcaConfig
    {
        /// <summary>
        /// Ambiente de trabajo: Homologación o Producción
        /// </summary>
        public ArcaEnvironment Environment { get; set; } = ArcaEnvironment.Homologacion;

        /// <summary>
        /// CUIT del contribuyente
        /// </summary>
        public long Cuit { get; set; }

        /// <summary>
        /// Ruta al archivo del certificado (.crt o .pem)
        /// </summary>
        public string? CertificatePath { get; set; }

        /// <summary>
        /// Ruta al archivo de la clave privada (.key)
        /// </summary>
        public string? PrivateKeyPath { get; set; }

        /// <summary>
        /// Contraseña de la clave privada (opcional)
        /// </summary>
        public string? PrivateKeyPassword { get; set; }
    }

    /// <summary>
    /// Ambientes disponibles de ARCA
    /// </summary>
    public enum ArcaEnvironment
    {
        /// <summary>
        /// Ambiente de homologación/testing
        /// </summary>
        Homologacion,

        /// <summary>
        /// Ambiente de producción
        /// </summary>
        Produccion
    }
}