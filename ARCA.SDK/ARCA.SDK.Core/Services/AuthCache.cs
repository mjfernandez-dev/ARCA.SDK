using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ARCA.SDK.Services
{
    /// <summary>
    /// Caché persistente de tokens de autenticación
    /// </summary>
    internal class AuthCache
    {
        private readonly string _cacheFilePath;
        private Dictionary<string, TokenCacheEntry> _cache;
        private readonly object _lock = new object();

        public AuthCache()
        {
            _cacheFilePath = GetCacheFilePath();
            _cache = new Dictionary<string, TokenCacheEntry>();
            LoadFromDisk();
        }

        /// <summary>
        /// Obtiene token del caché si existe y es válido
        /// </summary>
        public (string Token, string Sign)? Get(string key)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var entry))
                {
                    // Verificar si el token aún es válido (con margen de 5 minutos)
                    if (entry.IsValid())
                    {
                        return (entry.Token, entry.Sign);
                    }

                    // Token expirado, remover del caché
                    _cache.Remove(key);
                    SaveToDisk();
                }

                return null;
            }
        }

        /// <summary>
        /// Guarda token en el caché (memoria y disco)
        /// </summary>
        public void Set(string key, string token, string sign, DateTime expiration)
        {
            lock (_lock)
            {
                var entry = new TokenCacheEntry
                {
                    Token = token,
                    Sign = sign,
                    Expiration = expiration
                };

                _cache[key] = entry;
                SaveToDisk();
            }
        }

        /// <summary>
        /// Limpia el caché completo (memoria y disco)
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _cache.Clear();
                SaveToDisk();
            }
        }

        /// <summary>
        /// Obtiene la ruta del archivo de caché según el sistema operativo
        /// </summary>
        private string GetCacheFilePath()
        {
            string baseDir;

            // Detectar sistema operativo
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                // Windows: %APPDATA%\ARCA.SDK
                baseDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ARCA.SDK"
                );
            }
            else
            {
                // Linux/Mac: ~/.arca-sdk
                var home = Environment.GetEnvironmentVariable("HOME")
                    ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                baseDir = Path.Combine(home, ".arca-sdk");
            }

            // Crear directorio si no existe
            if (!Directory.Exists(baseDir))
            {
                Directory.CreateDirectory(baseDir);
            }

            return Path.Combine(baseDir, "tokens.json");
        }

        /// <summary>
        /// Carga el caché desde disco
        /// </summary>
        private void LoadFromDisk()
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(_cacheFilePath))
                    {
                        var json = File.ReadAllText(_cacheFilePath);
                        var loaded = JsonSerializer.Deserialize<Dictionary<string, TokenCacheEntry>>(json);

                        if (loaded != null)
                        {
                            // Filtrar tokens expirados al cargar
                            _cache = new Dictionary<string, TokenCacheEntry>();
                            foreach (var kvp in loaded)
                            {
                                if (kvp.Value.IsValid())
                                {
                                    _cache[kvp.Key] = kvp.Value;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Si hay error al cargar, empezar con caché vacío
                    _cache = new Dictionary<string, TokenCacheEntry>();
                }
            }
        }

        /// <summary>
        /// Guarda el caché a disco
        /// </summary>
        private void SaveToDisk()
        {
            lock (_lock)
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };

                    var json = JsonSerializer.Serialize(_cache, options);
                    File.WriteAllText(_cacheFilePath, json);
                }
                catch
                {
                    // Si falla el guardado, continuar (el caché en memoria sigue funcionando)
                    // En producción podrías loguear este error
                }
            }
        }
    }
}