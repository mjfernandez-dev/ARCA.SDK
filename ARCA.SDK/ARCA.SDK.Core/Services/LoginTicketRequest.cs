using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ARCA.SDK.Services
{
    /// <summary>
    /// Genera el LoginTicketRequest (TRA) firmado con PKCS#7 CMS para WSAA
    /// </summary>
    internal static class LoginTicketRequest
    {
        /// <summary>
        /// Genera un TRA firmado con PKCS#7 CMS para el servicio especificado
        /// </summary>
        public static string Generate(
            string service,
            long cuit,
            X509Certificate2 certificate)
        {
            // Generar XML del TRA
            var tra = GenerateTRA(service, cuit);

            // Firmar el TRA con PKCS#7 CMS
            var signedCms = SignWithCMS(tra, certificate);

            return signedCms;
        }

        private static string GenerateTRA(string service, long cuit)
        {
            // uniqueId debe ser un número entero (Unix timestamp en segundos es suficiente)
            var uniqueId = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var now = DateTime.UtcNow;
            var generationTime = now.AddMinutes(-10);
            var expirationTime = now.AddHours(12);

            // ARCA requiere formato ISO 8601 con zona horaria
            var generationTimeStr = generationTime.ToString("yyyy-MM-ddTHH:mm:ss.fff-00:00");
            var expirationTimeStr = expirationTime.ToString("yyyy-MM-ddTHH:mm:ss.fff-00:00");

            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<loginTicketRequest version=\"1.0\">");
            xml.AppendLine("  <header>");
            xml.AppendLine($"    <uniqueId>{uniqueId}</uniqueId>");
            xml.AppendLine($"    <generationTime>{generationTimeStr}</generationTime>");
            xml.AppendLine($"    <expirationTime>{expirationTimeStr}</expirationTime>");
            xml.AppendLine("  </header>");
            xml.AppendLine($"  <service>{service}</service>");
            xml.AppendLine("</loginTicketRequest>");

            return xml.ToString();
        }

        private static string SignWithCMS(string xml, X509Certificate2 certificate)
        {
            try
            {
                // Convertir XML a bytes
                var content = Encoding.UTF8.GetBytes(xml);

                // Crear ContentInfo
                var contentInfo = new ContentInfo(content);

                // Crear SignedCms
                var signedCms = new SignedCms(contentInfo, false); // false = datos incluidos

                // Crear CmsSigner con el certificado
                var signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, certificate)
                {
                    IncludeOption = X509IncludeOption.EndCertOnly
                };

                // Firmar
                signedCms.ComputeSignature(signer, false);

                // Obtener el CMS en formato Base64
                var signedBytes = signedCms.Encode();
                return Convert.ToBase64String(signedBytes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Error al firmar el TRA con PKCS#7 CMS. " +
                    "Verifique que el certificado tenga clave privada válida.",
                    ex
                );
            }
        }
    }
}