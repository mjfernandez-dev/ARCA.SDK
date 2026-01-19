using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using ARCA.SDK.Exceptions;

namespace ARCA.SDK.Utils
{
    /// <summary>
    /// Utilidades para trabajar con certificados X.509
    /// </summary>
    internal static class CertificateHelper
    {
        /// <summary>
        /// Carga un certificado desde archivos .crt/.key o .pfx
        /// </summary>
        public static X509Certificate2 LoadCertificate(
            string certPath,
            string? keyPath = null,
            string? password = null)
        {
            if (!File.Exists(certPath))
                throw new ArcaException($"No se encontró el certificado en: {certPath}");

            try
            {
                // Si es .pfx, cargar directamente
                if (certPath.EndsWith(".pfx", StringComparison.OrdinalIgnoreCase) ||
                    certPath.EndsWith(".p12", StringComparison.OrdinalIgnoreCase))
                {
                    return LoadFromPfx(certPath, password);
                }

                // Si es .pem/.crt con .key separado
                if (!string.IsNullOrEmpty(keyPath))
                {
                    return LoadFromPemFiles(certPath, keyPath, password);
                }

                // Intentar cargar como certificado simple
                return new X509Certificate2(certPath);
            }
            catch (Exception ex) when (!(ex is ArcaException))
            {
                throw new ArcaException(
                    "Error al cargar el certificado. Verifique que los archivos sean válidos y la contraseña correcta.",
                    ex
                );
            }
        }

        private static X509Certificate2 LoadFromPfx(string pfxPath, string? password)
        {
#if NET5_0_OR_GREATER || NET6_0_OR_GREATER || NET8_0_OR_GREATER
            return new X509Certificate2(pfxPath, password, X509KeyStorageFlags.Exportable);
#else
            return new X509Certificate2(
                File.ReadAllBytes(pfxPath),
                password,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable
            );
#endif
        }

        private static X509Certificate2 LoadFromPemFiles(
            string certPath,
            string keyPath,
            string? password)
        {
#if NET5_0_OR_GREATER || NET6_0_OR_GREATER || NET8_0_OR_GREATER
            // .NET 5+ tiene soporte nativo para PEM
            var certPem = File.ReadAllText(certPath);
            var keyPem = File.ReadAllText(keyPath);
            
            var cert = X509Certificate2.CreateFromPem(certPem);
            
            RSA rsa = RSA.Create();
            if (!string.IsNullOrEmpty(password))
            {
                rsa.ImportFromEncryptedPem(keyPem, password);
            }
            else
            {
                rsa.ImportFromPem(keyPem);
            }
            
            var certWithKey = cert.CopyWithPrivateKey(rsa);
            return new X509Certificate2(certWithKey.Export(X509ContentType.Pkcs12));
#else
            // Para .NET Framework 4.8 y .NET Standard 2.0
            // Necesitamos usar BouncyCastle o convertir a PFX manualmente
            throw new ArcaException(
                "La carga de certificados .crt/.key separados requiere .NET 5 o superior. " +
                "Por favor use un archivo .pfx/.p12 que combine certificado y clave privada, " +
                "o ejecute en .NET 6/8. " +
                "Para convertir a PFX use: openssl pkcs12 -export -out cert.pfx -inkey key.key -in cert.crt"
            );
#endif
        }

        /// <summary>
        /// Verifica que el certificado sea válido y no esté vencido
        /// </summary>
        public static void ValidateCertificate(X509Certificate2 certificate)
        {
            if (certificate == null)
                throw new ArgumentNullException(nameof(certificate));

            var now = DateTime.Now;

            if (certificate.NotBefore > now)
            {
                throw new ArcaAuthException(
                    $"El certificado aún no es válido. Válido desde: {certificate.NotBefore:dd/MM/yyyy}"
                );
            }

            if (certificate.NotAfter < now)
            {
                throw new ArcaAuthException(
                    $"El certificado está vencido. Venció el: {certificate.NotAfter:dd/MM/yyyy}"
                );
            }

            if (!certificate.HasPrivateKey)
            {
                throw new ArcaAuthException("El certificado no tiene clave privada asociada");
            }
        }

        /// <summary>
        /// Convierte certificado PEM a PFX (solo .NET 5+)
        /// </summary>
        public static byte[] ConvertPemToPfx(
            string certPath,
            string keyPath,
            string? password = null)
        {
#if NET5_0_OR_GREATER || NET6_0_OR_GREATER || NET8_0_OR_GREATER
            var cert = LoadFromPemFiles(certPath, keyPath, password);
            return cert.Export(X509ContentType.Pkcs12, password);
#else
            throw new ArcaException(
                "La conversión PEM a PFX requiere .NET 5 o superior. " +
                "Use OpenSSL: openssl pkcs12 -export -out cert.pfx -inkey key.key -in cert.crt"
            );
#endif
        }
    }
}