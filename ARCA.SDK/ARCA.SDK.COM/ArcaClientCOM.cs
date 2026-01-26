using System;
using System.Runtime.InteropServices;
using ARCA.SDK;
using ARCA.SDK.Configuration;
using ARCA.SDK.Models;

namespace ARCA.SDK.COM
{
    /// <summary>
    /// Wrapper COM para ArcaClient - Compatible con Visual FoxPro
    /// </summary>
    [ComVisible(true)]
    [Guid("A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D")]
    [ProgId("ARCA.SDK.ArcaClient")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class ArcaClientCOM
    {
        private ArcaClient _client;
        private string _ultimoError;

        /// <summary>
        /// Configura el cliente ARCA con los datos necesarios
        /// </summary>
        /// <param name="cuit">CUIT del contribuyente (sin guiones)</param>
        /// <param name="certificadoPath">Ruta completa al archivo .pfx o .crt</param>
        /// <param name="certificadoPassword">Contraseña del certificado (vacío para .crt)</param>
        /// <param name="ambiente">1=Homologación, 2=Producción</param>
        /// <returns>true si se configuró correctamente, false en caso de error</returns>
        public bool Configurar(string cuit, string certificadoPath, string certificadoPassword, int ambiente)
        {
            try
            {
                _ultimoError = string.Empty;

                // Convertir CUIT string a long
                if (!long.TryParse(cuit, out long cuitLong))
                {
                    _ultimoError = "CUIT inválido";
                    return false;
                }

                var ambienteEnum = ambiente == 2 ? ArcaEnvironment.Produccion : ArcaEnvironment.Homologacion;

                var config = new ArcaConfig
                {
                    Cuit = cuitLong,
                    CertificatePath = certificadoPath,
                    CertificatePassword = certificadoPassword,
                    Environment = ambienteEnum
                };

                _client = new ArcaClient(config);

                return true;
            }
            catch (Exception ex)
            {
                _ultimoError = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Consulta el último número de comprobante autorizado
        /// </summary>
        /// <param name="puntoVenta">Número de punto de venta</param>
        /// <param name="tipoComprobante">Tipo de comprobante (1=Factura A, 6=Factura B, 11=Factura C)</param>
        /// <returns>Último número autorizado, -1 en caso de error</returns>
        public int ConsultarUltimoComprobante(int puntoVenta, int tipoComprobante)
        {
            try
            {
                _ultimoError = string.Empty;

                if (_client == null)
                {
                    _ultimoError = "Cliente no configurado. Llamar primero a Configurar()";
                    return -1;
                }

                // Convertir async a sync para COM
                var tarea = _client.ObtenerUltimoComprobanteAsync(puntoVenta, tipoComprobante);
                tarea.Wait();

                // Convertir long a int (COM es más compatible con int)
                return (int)tarea.Result;
            }
            catch (Exception ex)
            {
                _ultimoError = ex.Message;
                return -1;
            }
        }

        /// <summary>
        /// Obtiene el último mensaje de error
        /// </summary>
        public string ObtenerUltimoError()
        {
            return _ultimoError ?? string.Empty;
        }
    }
}