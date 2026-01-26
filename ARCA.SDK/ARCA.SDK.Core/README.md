# ARCA.SDK

SDK .NET para integración con ARCA (ex AFIP) - Facturación electrónica para Argentina

## Características

✅ Autenticación WSAA con caché automático de tokens  
✅ Facturación electrónica WSFE (Facturas A, B, C)  
✅ Multi-targeting: .NET Framework 4.8, .NET Standard 2.0, .NET 8  
✅ Soporte para certificados .pfx y .pem  
✅ Manejo robusto de errores  

## Instalación
```bash
dotnet add package ARCA.SDK
```

## Uso básico
```csharp
using ARCA.SDK;
using ARCA.SDK.Configuration;

// Configurar cliente
var config = new ArcaConfig
{
    Cuit = 20123456789,
    CertificatePath = "certificado.pfx",
    CertificatePassword = "password",
    Environment = ArcaEnvironment.Homologacion
};

var client = new ArcaClient(config);

// Consultar último comprobante
var ultimo = await client.ObtenerUltimoComprobanteAsync(
    puntoVenta: 1,
    tipoComprobante: 11  // Factura C
);

Console.WriteLine($"Último número: {ultimo}");
```

## Documentación

📖 [Documentación completa](https://github.com/tu-usuario/ARCA.SDK)

## Licencia

MIT License - Ver [LICENSE](LICENSE) para más detalles