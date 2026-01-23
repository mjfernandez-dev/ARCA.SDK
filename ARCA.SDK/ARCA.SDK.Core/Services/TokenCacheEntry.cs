using System;

namespace ARCA.SDK.Services
{
    /// <summary>
    /// Entrada de caché de token para persistencia
    /// </summary>
    internal class TokenCacheEntry
    {
        public string Token { get; set; } = string.Empty;
        public string Sign { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public long Cuit { get; set; }
        public string Service { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;

        /// <summary>
        /// Verifica si el token aún es válido (con margen de 5 minutos)
        /// </summary>
        public bool IsValid()
        {
            return Expiration > DateTime.Now.AddMinutes(5);
        }
    }
}