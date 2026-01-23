using System;

namespace ARCA.SDK.Clients
{
    /// <summary>
    /// Estructura de autenticación para WSFE
    /// </summary>
    internal class WsfeAuth
    {
        public string Token { get; set; } = string.Empty;
        public string Sign { get; set; } = string.Empty;
        public long Cuit { get; set; }
    }

    /// <summary>
    /// Request para autorizar comprobante
    /// </summary>
    internal class WsfeCAERequest
    {
        public WsfeAuth Auth { get; set; } = new WsfeAuth();
        public WsfeComprobante[] Comprobantes { get; set; } = Array.Empty<WsfeComprobante>();
    }

    /// <summary>
    /// Datos de un comprobante para WSFE
    /// </summary>
    internal class WsfeComprobante
    {
        public int Concepto { get; set; }
        public int DocTipo { get; set; }
        public long DocNro { get; set; }
        public long CbteDesde { get; set; }
        public long CbteHasta { get; set; }
        public string CbteFch { get; set; } = string.Empty;
        public decimal ImpTotal { get; set; }
        public decimal ImpTotConc { get; set; }
        public decimal ImpNeto { get; set; }
        public decimal ImpOpEx { get; set; }
        public decimal ImpIVA { get; set; }
        public decimal ImpTrib { get; set; }
        public string MonId { get; set; } = "PES";
        public decimal MonCotiz { get; set; }
        public int CondicionIVAReceptor { get; set; }
        public string? FchServDesde { get; set; }
        public string? FchServHasta { get; set; }
        public string? FchVtoPago { get; set; }
        public WsfeAlicuotaIVA[]? Iva { get; set; }
        public WsfeTributo[]? Tributos { get; set; }
        public WsfeComprobanteAsociado[]? CbtesAsoc { get; set; }
    }

    /// <summary>
    /// Alícuota de IVA para WSFE
    /// </summary>
    internal class WsfeAlicuotaIVA
    {
        public int Id { get; set; }
        public decimal BaseImp { get; set; }
        public decimal Importe { get; set; }
    }

    /// <summary>
    /// Tributo para WSFE
    /// </summary>
    internal class WsfeTributo
    {
        public int Id { get; set; }
        public string Desc { get; set; } = string.Empty;
        public decimal BaseImp { get; set; }
        public decimal Alic { get; set; }
        public decimal Importe { get; set; }
    }

    /// <summary>
    /// Comprobante asociado para WSFE
    /// </summary>
    internal class WsfeComprobanteAsociado
    {
        public int Tipo { get; set; }
        public int PtoVta { get; set; }
        public long Nro { get; set; }
        public long? Cuit { get; set; }
    }

    /// <summary>
    /// Response de autorización CAE
    /// </summary>
    internal class WsfeCAEResponse
    {
        public WsfeResultado[]? FeDetResp { get; set; }
        public WsfeError[]? Errors { get; set; }
    }

    /// <summary>
    /// Resultado de autorización de un comprobante
    /// </summary>
    internal class WsfeResultado
    {
        public string? Resultado { get; set; }
        public string? CAE { get; set; }
        public string? CAEFchVto { get; set; }
        public long? CbteDesde { get; set; }
        public WsfeObservacion[]? Observaciones { get; set; }
    }

    /// <summary>
    /// Observación de ARCA
    /// </summary>
    internal class WsfeObservacion
    {
        public int Code { get; set; }
        public string? Msg { get; set; }
    }

    /// <summary>
    /// Error de WSFE
    /// </summary>
    internal class WsfeError
    {
        public int Code { get; set; }
        public string? Msg { get; set; }
    }

    /// <summary>
    /// Response de último comprobante
    /// </summary>
    internal class WsfeUltimoComprobanteResponse
    {
        public long? CbteNro { get; set; }
        public WsfeError[]? Errors { get; set; }
    }
}