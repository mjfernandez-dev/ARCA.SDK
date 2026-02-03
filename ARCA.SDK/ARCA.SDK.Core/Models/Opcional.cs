namespace ARCA.SDK.Models
{
    /// <summary>
    /// Dato opcional para regímenes especiales
    /// </summary>
    public class Opcional
    {
        /// <summary>
        /// Código del opcional (ej: "2101" para CBU, "27" para FCE)
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Valor del opcional
        /// </summary>
        public string Valor { get; set; } = string.Empty;
    }
}