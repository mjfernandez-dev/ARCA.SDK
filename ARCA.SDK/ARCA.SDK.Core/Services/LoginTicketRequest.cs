using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace ARCA.SDK.Services
{
    /// <summary>
    /// Genera el LoginTicketRequest (TRA) para autenticación con WSAA
    /// </summary>
    internal static class LoginTicketRequest
    {
        /// <summary>
        /// Genera un TRA firmado para el servicio especificado
        /// </summary>
        public static string Generate(
            string service,
            long cuit,
            X509Certificate2 certificate)
        {
            // Generar XML del TRA
            var tra = GenerateTRA(service, cuit);

            // Firmar el TRA con el certificado
            var signedTra = SignXml(tra, certificate);

            return signedTra;
        }

        private static string GenerateTRA(string service, long cuit)
        {
            var uniqueId = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
            var generationTime = DateTime.UtcNow.AddMinutes(-10);
            var expirationTime = DateTime.UtcNow.AddMinutes(10);

            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<loginTicketRequest version=\"1.0\">");
            xml.AppendLine("  <header>");
            xml.AppendLine($"    <uniqueId>{uniqueId}</uniqueId>");
            xml.AppendLine($"    <generationTime>{generationTime:s}</generationTime>");
            xml.AppendLine($"    <expirationTime>{expirationTime:s}</expirationTime>");
            xml.AppendLine("  </header>");
            xml.AppendLine("  <service>{service}</service>");
            xml.AppendLine("</loginTicketRequest>");

            return xml.ToString();
        }

        private static string SignXml(string xml, X509Certificate2 certificate)
        {
            // Cargar el XML
            var doc = new XmlDocument { PreserveWhitespace = true };
            doc.LoadXml(xml);

            // Crear objeto SignedXml
            var signedXml = new SignedXml(doc)
            {
                SigningKey = certificate.GetRSAPrivateKey()
            };

            // Crear referencia al documento completo
            var reference = new Reference(string.Empty);
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            signedXml.AddReference(reference);

            // Agregar información del certificado
            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(certificate));
            signedXml.KeyInfo = keyInfo;

            // Firmar
            signedXml.ComputeSignature();

            // Obtener XML firmado
            var signatureXml = signedXml.GetXml();

            // Insertar firma en el documento
            doc.DocumentElement?.AppendChild(doc.ImportNode(signatureXml, true));

            return doc.OuterXml;
        }
    }
}