using System;
using System.Collections.Generic;

namespace ARCA.SDK.Models
{
    /// <summary>
    /// Representa un comprobante electrónico para ARCA
    /// </summary>
    public class Comprobante
    {
        /// <summary>
        /// Punto de venta
        /// </summary>
        public int PuntoVenta { get; set; }

        /// <summary>
        /// Tipo de comprobante (1=Factura A, 6=Factura B, 11=Factura C, etc.)
        /// </summary>
        public int TipoComprobante { get; set; }

        /// <summary>
        /// Número del comprobante
        /// </summary>
        public long Numero { get; set; }

        /// <summary>
        /// Concepto: 1=Productos, 2=Servicios, 3=Productos y Servicios
        /// </summary>
        public int Concepto { get; set; }

        /// <summary>
        /// Tipo de documento del receptor (80=CUIT, 96=DNI, etc.)
        /// </summary>
        public int TipoDocumento { get; set; }

        /// <summary>
        /// Número de documento del receptor
        /// </summary>
        public long NumeroDocumento { get; set; }

        /// <summary>
        /// Fecha de emisión del comprobante
        /// </summary>
        public DateTime FechaEmision { get; set; }

        /// <summary>
        /// Importe total del comprobante
        /// </summary>
        public decimal ImporteTotal { get; set; }

        /// <summary>
        /// Importe neto gravado
        /// </summary>
        public decimal ImporteNeto { get; set; }

        /// <summary>
        /// Importe no gravado
        /// </summary>
        public decimal ImporteNoGravado { get; set; }

        /// <summary>
        /// Importe exento de IVA
        /// </summary>
        public decimal ImporteExento { get; set; }

        /// <summary>
        /// Importe total de IVA
        /// </summary>
        public decimal ImporteIVA { get; set; }

        /// <summary>
        /// Importe total de tributos
        /// </summary>
        public decimal ImporteTributos { get; set; }

        /// <summary>
        /// Código de moneda (PES, USD, etc.)
        /// </summary>
        public string MonedaId { get; set; } = "PES";

        /// <summary>
        /// Cotización de la moneda
        /// </summary>
        public decimal MonedaCotizacion { get; set; } = 1;

        /// <summary>
        /// Condición frente al IVA del receptor (obligatorio desde RG 5616/2024)
        /// 1=IVA Responsable Inscripto, 4=IVA Sujeto Exento, 5=Consumidor Final, 6=Responsable Monotributo
        /// </summary>
        public int CondicionIVAReceptor { get; set; }

        /// <summary>
        /// Fecha de inicio del servicio (requerido si Concepto = 2 o 3)
        /// </summary>
        public DateTime? FechaServicioDesde { get; set; }

        /// <summary>
        /// Fecha de fin del servicio (requerido si Concepto = 2 o 3)
        /// </summary>
        public DateTime? FechaServicioHasta { get; set; }

        /// <summary>
        /// Fecha de vencimiento del pago (requerido si Concepto = 2 o 3)
        /// </summary>
        public DateTime? FechaVencimientoPago { get; set; }

        /// <summary>
        /// Alícuotas de IVA aplicadas
        /// </summary>
        public List<AlicuotaIVA> AlicuotasIVA { get; set; } = new List<AlicuotaIVA>();

        /// <summary>
        /// Tributos aplicados
        /// </summary>
        public List<Tributo> Tributos { get; set; } = new List<Tributo>();

        /// <summary>
        /// Comprobantes asociados (para notas de crédito/débito)
        /// </summary>
        public List<ComprobanteAsociado> ComprobantesAsociados { get; set; } = new List<ComprobanteAsociado>();
    }
}