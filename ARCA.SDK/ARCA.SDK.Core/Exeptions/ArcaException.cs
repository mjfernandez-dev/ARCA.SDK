using System;

namespace ARCA.SDK.Exceptions
{
    /// <summary>
    /// Excepción base para errores del SDK de ARCA
    /// </summary>
    public class ArcaException : Exception
    {
        /// <summary>
        /// Código de error de ARCA (si aplica)
        /// </summary>
        public string? CodigoError { get; set; }

        public ArcaException() { }

        public ArcaException(string message) : base(message) { }

        public ArcaException(string message, Exception innerException)
            : base(message, innerException) { }

        public ArcaException(string message, string codigoError) : base(message)
        {
            CodigoError = codigoError;
        }
    }
}