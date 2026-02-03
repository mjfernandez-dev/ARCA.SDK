# MJF.ARCA.SDK

[![NuGet](https://img.shields.io/nuget/v/MJF.ARCA.SDK.svg)](https://www.nuget.org/packages/MJF.ARCA.SDK/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MJF.ARCA.SDK.svg)](https://www.nuget.org/packages/MJF.ARCA.SDK/)

SDK .NET para integración con ARCA (ex AFIP) - Facturación electrónica para Argentina

## Características

✅ Autenticación WSAA con caché automático de tokens  
✅ Facturación electrónica WSFE (Facturas A, B, C)  
✅ Multi-targeting: .NET Framework 4.8, .NET Standard 2.0, .NET 8  
✅ Soporte para certificados .pfx y .pem  
✅ Manejo robusto de errores  

## Instalación
```bash
dotnet add package MJF.ARCA.SDK --prerelease
```

## Inicio rápido

### 1. Configurar el cliente
```csharp
using ARCA.SDK;
using ARCA.SDK.Configuration;

var config = new ArcaConfig
{
    Cuit = 20123456789,
    CertificatePath = "certificado.pfx",
    CertificatePassword = "tu_password",
    Environment = ArcaEnvironment.Homologacion // o Produccion
};

var client = new ArcaClient(config);
```

### 2. Consultar último comprobante
```csharp
var ultimoNumero = await client.ObtenerUltimoComprobanteAsync(
    puntoVenta: 1,
    tipoComprobante: 11  // Factura C
);

Console.WriteLine($"Último número: {ultimoNumero}");
Console.WriteLine($"Próximo a usar: {ultimoNumero + 1}");
```

### 3. Autorizar un comprobante
```csharp
using ARCA.SDK.Models;

var comprobante = new Comprobante
{
    PuntoVenta = 1,
    TipoComprobante = 11,  // Factura C
    Numero = ultimoNumero + 1,
    Concepto = 1,  // Productos
    TipoDocumento = 99,  // Consumidor Final
    NumeroDocumento = 0,
    FechaEmision = DateTime.Now,
    ImporteTotal = 1000m,
    ImporteNeto = 1000m,
    ImporteNoGravado = 0,
    ImporteExento = 0,
    ImporteIVA = 0,
    ImporteTributos = 0,
    MonedaId = "PES",
    MonedaCotizacion = 1,
    CondicionIVAReceptor = 5  // Consumidor Final
};

var resultado = await client.AutorizarComprobanteAsync(comprobante);

if (resultado.Exitoso)
{
    Console.WriteLine($"✓ CAE: {resultado.CAE}");
    Console.WriteLine($"✓ Vencimiento: {resultado.FechaVencimientoCAE:dd/MM/yyyy}");
}
else
{
    Console.WriteLine($"✗ Error: {resultado.MensajeError}");
}
```

## Características

- ✅ **Autenticación WSAA** con caché automático de tokens
- ✅ **WSFE** - Facturación electrónica (Facturas A, B, C)
- ✅ **Multi-targeting**: .NET Framework 4.8, .NET Standard 2.0, .NET 8
- ✅ **Certificados**: Soporte para .pfx y .pem
- ✅ **Manejo robusto de errores** con excepciones específicas
- ✅ **Async/await** en todos los métodos

## Requisitos

- Certificado digital emitido por ARCA (ex AFIP)
- CUIT habilitado para facturación electrónica
- .NET Framework 4.8 o superior / .NET Core 2.0+ / .NET 5+

## Documentación

📖 [Documentación completa en GitHub](https://github.com/mjfernandez-dev/ARCA.SDK)

## Licencia

MIT License - Ver [LICENSE](https://github.com/mjfernandez-dev/ARCA.SDK/blob/main/LICENSE) para más detalles

## Soporte

- 🐛 [Reportar un bug](https://github.com/mjfernandez-dev/ARCA.SDK/issues)
- 💡 [Solicitar feature](https://github.com/mjfernandez-dev/ARCA.SDK/issues)
- 📧 Contacto: [mjfernandez.dev@gmail.com]