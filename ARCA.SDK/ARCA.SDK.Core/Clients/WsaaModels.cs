using System;
using System.Xml.Serialization;

namespace ARCA.SDK.Clients
{
    /// <summary>
    /// Request para el servicio WSAA
    /// </summary>
    [XmlRoot("loginCms")]
    public class WsaaLoginCmsRequest
    {
        [XmlElement("in0")]
        public string? LoginTicketRequest { get; set; }
    }

    /// <summary>
    /// Response del servicio WSAA
    /// </summary>
    [XmlRoot("loginCmsReturn")]
    public class WsaaLoginCmsResponse
    {
        [XmlElement("token")]
        public string? Token { get; set; }

        [XmlElement("sign")]
        public string? Sign { get; set; }

        [XmlElement("expirationTime")]
        public DateTime ExpirationTime { get; set; }
    }
}