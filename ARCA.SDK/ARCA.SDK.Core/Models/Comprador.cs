namespace ARCA.SDK.Models
{
    /// <summary>
    /// Comprador en operaciones con múltiples compradores
    /// </summary>
    public class Comprador
    {
        /// <summary>
        /// Tipo de documento
        /// </summary>
        public int TipoDocumento { get; set; }

        /// <summary>
        /// Número de documento
        /// </summary>
        public long NumeroDocumento { get; set; }

        /// <summary>
        /// Porcentaje de participación (0-100)
        /// </summary>
        public decimal Porcentaje { get; set; }
    }
}