namespace ARCA.SDK.Models
{
    /// <summary>
    /// Representa un tributo aplicado en un comprobante
    /// </summary>
    public class Tributo
    {
        /// <summary>
        /// Código del tributo (1=Impuestos nacionales, 2=Impuestos provinciales, etc.)
        /// </summary>
        public int Codigo { get; set; }

        /// <summary>
        /// Descripción del tributo
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Base imponible del tributo
        /// </summary>
        public decimal BaseImponible { get; set; }

        /// <summary>
        /// Alícuota aplicada
        /// </summary>
        public decimal Alicuota { get; set; }

        /// <summary>
        /// Importe del tributo
        /// </summary>
        public decimal Importe { get; set; }
    }
}