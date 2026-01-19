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
        /// Ruta al archivo del certificado (.pfx, .p12, .crt)
        /// </summary>
        public string? CertificatePath { get; set; }

        /// <summary>
        /// Ruta al archivo de la clave privada (.key) - Solo si usa .crt separado
        /// Nota: Archivos .crt/.key separados solo funcionan en .NET 5+
        /// Para .NET Framework 4.8, use .pfx
        /// </summary>
        public string? PrivateKeyPath { get; set; }

        /// <summary>
        /// Contraseña del certificado (si está protegido)
        /// </summary>
        public string? CertificatePassword { get; set; }
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