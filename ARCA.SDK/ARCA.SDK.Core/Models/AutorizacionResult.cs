using System;
using System.Collections.Generic;

namespace ARCA.SDK.Models
{
    /// <summary>
    /// Resultado de la autorización de un comprobante
    /// </summary>
    public class AutorizacionResult
    {
        /// <summary>
        /// Indica si la autorización fue exitosa
        /// </summary>
        public bool Exitoso { get; set; }

        /// <summary>
        /// Código de Autorización Electrónica (CAE)
        /// </summary>
        public string? CAE { get; set; }

        /// <summary>
        /// Fecha de vencimiento del CAE
        /// </summary>
        public DateTime? FechaVencimientoCAE { get; set; }

        /// <summary>
        /// Número de comprobante autorizado
        /// </summary>
        public long NumeroComprobante { get; set; }

        /// <summary>
        /// Observaciones de ARCA
        /// </summary>
        public List<string> Observaciones { get; set; } = new List<string>();

        /// <summary>
        /// Mensaje de error (si hubo)
        /// </summary>
        public string? MensajeError { get; set; }

        /// <summary>
        /// Código de error (si hubo)
        /// </summary>
        public string? CodigoError { get; set; }
    }
}