using System;

namespace ARCA.SDK.Models
{
    /// <summary>
    /// Representa un comprobante asociado (para notas de crédito/débito)
    /// </summary>
    public class ComprobanteAsociado
    {
        /// <summary>
        /// Tipo de comprobante asociado
        /// </summary>
        public int Tipo { get; set; }

        /// <summary>
        /// Punto de venta del comprobante asociado
        /// </summary>
        public int PuntoVenta { get; set; }

        /// <summary>
        /// Número del comprobante asociado
        /// </summary>
        public long Numero { get; set; }

        /// <summary>
        /// CUIT del emisor del comprobante asociado (opcional)
        /// </summary>
        public long? Cuit { get; set; }

        /// <summary>
        /// Fecha del comprobante asociado (opcional)
        /// </summary>
        public DateTime? Fecha { get; set; }
    }
}