namespace ARCA.SDK.Models
{
    /// <summary>
    /// Representa una alícuota de IVA aplicada en un comprobante
    /// </summary>
    public class AlicuotaIVA
    {
        /// <summary>
        /// Código de alícuota (3=0%, 4=10.5%, 5=21%, 6=27%, etc.)
        /// </summary>
        public int Codigo { get; set; }

        /// <summary>
        /// Base imponible sobre la que se aplica el IVA
        /// </summary>
        public decimal BaseImponible { get; set; }

        /// <summary>
        /// Importe del IVA calculado
        /// </summary>
        public decimal Importe { get; set; }
    }
}