using System;

namespace ARCA.SDK.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando los datos del comprobante no pasan las validaciones
    /// </summary>
    public class ArcaValidationException : ArcaException
    {
        public ArcaValidationException() { }

        public ArcaValidationException(string message) : base(message) { }

        public ArcaValidationException(string message, Exception innerException)
            : base(message, innerException) { }

        public ArcaValidationException(string message, string codigoError)
            : base(message, codigoError) { }
    }
}