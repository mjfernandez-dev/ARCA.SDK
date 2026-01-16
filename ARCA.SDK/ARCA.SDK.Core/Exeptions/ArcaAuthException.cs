using System;

namespace ARCA.SDK.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando falla la autenticación con ARCA
    /// </summary>
    public class ArcaAuthException : ArcaException
    {
        public ArcaAuthException() { }

        public ArcaAuthException(string message) : base(message) { }

        public ArcaAuthException(string message, Exception innerException)
            : base(message, innerException) { }

        public ArcaAuthException(string message, string codigoError)
            : base(message, codigoError) { }
    }
}