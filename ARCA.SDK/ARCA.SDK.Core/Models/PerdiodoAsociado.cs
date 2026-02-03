using System;

namespace ARCA.SDK.Models
{
    /// <summary>
    /// Período asociado para servicios
    /// </summary>
    public class PeriodoAsociado
    {
        /// <summary>
        /// Fecha desde del período (YYYYMMDD)
        /// </summary>
        public DateTime FechaDesde { get; set; }

        /// <summary>
        /// Fecha hasta del período (YYYYMMDD)
        /// </summary>
        public DateTime FechaHasta { get; set; }
    }
}