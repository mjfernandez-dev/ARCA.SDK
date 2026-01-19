using System;
using System.Collections.Concurrent;

namespace ARCA.SDK.Services
{
    /// <summary>
    /// Caché en memoria para tokens de autenticación
    /// </summary>
    internal class AuthCache
    {
        private readonly ConcurrentDictionary<string, CachedToken> _cache;

        public AuthCache()
        {
            _cache = new ConcurrentDictionary<string, CachedToken>();
        }

        /// <summary>
        /// Obtiene token del caché si existe y es válido
        /// </summary>
        public (string Token, string Sign)? Get(string key)
        {
            if (_cache.TryGetValue(key, out var cached))
            {
                // Verificar si el token aún es válido (con margen de 5 minutos)
                if (cached.Expiration > DateTime.Now.AddMinutes(5))
                {
                    return (cached.Token, cached.Sign);
                }

                // Token expirado, remover del caché
                _cache.TryRemove(key, out _);
            }

            return null;
        }

        /// <summary>
        /// Guarda token en el caché
        /// </summary>
        public void Set(string key, string token, string sign, DateTime expiration)
        {
            var cached = new CachedToken
            {
                Token = token,
                Sign = sign,
                Expiration = expiration
            };

            _cache.AddOrUpdate(key, cached, (k, old) => cached);
        }

        /// <summary>
        /// Limpia el caché completo
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
        }

        private class CachedToken
        {
            public string Token { get; set; } = string.Empty;
            public string Sign { get; set; } = string.Empty;
            public DateTime Expiration { get; set; }
        }
    }
}