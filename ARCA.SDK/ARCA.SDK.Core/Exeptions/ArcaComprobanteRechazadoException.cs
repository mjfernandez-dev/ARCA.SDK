using System;
using System.Collections.Generic;

namespace ARCA.SDK.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando ARCA rechaza un comprobante
    /// </summary>
    public class ArcaComprobanteRechazadoException : ArcaException
    {
        /// <summary>
        /// Observaciones devueltas por ARCA
        /// </summary>
        public List<string> Observaciones { get; set; } = new List<string>();

        public ArcaComprobanteRechazadoException() { }

        public ArcaComprobanteRechazadoException(string message) : base(message) { }

        public ArcaComprobanteRechazadoException(string message, List<string> observaciones)
            : base(message)
        {
            Observaciones = observaciones;
        }

        public ArcaComprobanteRechazadoException(string message, string codigoError, List<string> observaciones)
            : base(message, codigoError)
        {
            Observaciones = observaciones;
        }
    }
}