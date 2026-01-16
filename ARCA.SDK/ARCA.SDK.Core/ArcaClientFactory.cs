using ARCA.SDK.Configuration;
using System;

namespace ARCA.SDK
{
    /// <summary>
    /// Factory para crear instancias de ArcaClient de forma simplificada
    /// </summary>
    public static class ArcaClientFactory
    {
        /// <summary>
        /// Crea un cliente ARCA con configuración mediante Action
        /// </summary>
        /// <param name="configureAction">Acción para configurar el cliente</param>
        /// <returns>Instancia configurada de ArcaClient</returns>
        public static ArcaClient Create(Action<ArcaConfig> configureAction)
        {
            if (configureAction == null)
                throw new ArgumentNullException(nameof(configureAction));

            var config = new ArcaConfig();
            configureAction(config);

            return new ArcaClient(config);
        }

        /// <summary>
        /// Crea un cliente ARCA con una configuración existente
        /// </summary>
        /// <param name="config">Configuración del cliente</param>
        /// <returns>Instancia configurada de ArcaClient</returns>
        public static ArcaClient Create(ArcaConfig config)
        {
            return new ArcaClient(config);
        }
    }
}