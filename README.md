# ARCA.SDK

[![.NET](https://img.shields.io/badge/.NET-Standard%202.0%20%7C%20Framework%204.8%20%7C%20.NET%208-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

SDK .NET para integrar sistemas con ARCA (ex AFIP). Librería que facilita la conexión y comunicación con los servicios de la Agencia de Recaudación y Control Aduanero de Argentina.

## 🚀 Características

- ✅ **Autenticación WSAA** con caché automático de tokens
- ✅ **Facturación Electrónica** (WSFE v1) - Autorización de comprobantes
- ✅ **Multi-targeting**: Compatible con .NET Standard 2.0, .NET Framework 4.8 y .NET 8+
- ✅ **Soporte para certificados**: .pfx, .p12, y .crt/.key (solo .NET 5+)
- ✅ **Async/await** nativo
- ✅ **Validaciones automáticas**
- ✅ **Excepciones tipadas** para mejor manejo de errores
- ✅ **Ambiente de homologación y producción**

## 📦 Instalación

### Opción 1: NuGet (próximamente)
```bash
dotnet add package ARCA.SDK
```

### Opción 2: Clonar y compilar
```bash
git clone https://github.com/tu-usuario/ARCA.SDK.git
cd ARCA.SDK
dotnet build
```

## 🔧 Requisitos

- .NET Standard 2.0+ / .NET Framework 4.8+ / .NET 6+ / .NET 8+
- Certificado digital emitido por ARCA (.pfx o .p12)
- CUIT registrado en ARCA

## 📚 Inicio Rápido

### 1. Configurar el cliente
```csharp
using ARCA.SDK;
using ARCA.SDK.Configuration;

// Crear cliente con configuración
var client = ArcaClientFactory.Create(config =>
{
    config.Environment = ArcaEnvironment.Homologacion; // o Produccion
    config.Cuit = 20123456789;
    config.CertificatePath = "path/to/certificado.pfx";
    config.CertificatePassword = "password"; // opcional
});
```

### 2. Consultar último comprobante
```csharp
// Obtener el último número de factura autorizada
long ultimoNumero = await client.ObtenerUltimoComprobanteAsync(
    puntoVenta: 1,
    tipoComprobante: 1  // 1 = Factura A
);

Console.WriteLine($"Último comprobante: {ultimoNumero}");
```

### 3. Autorizar una factura
```csharp
using ARCA.SDK.Models;

// Crear comprobante
var factura = new Comprobante
{
    PuntoVenta = 1,
    TipoComprobante = 1,        // 1 = Factura A
    Numero = ultimoNumero + 1,  // Siguiente número
    Concepto = 1,               // 1 = Productos
    TipoDocumento = 80,         // 80 = CUIT
    NumeroDocumento = 30123456789,
    FechaEmision = DateTime.Today,
    
    // Importes
    ImporteTotal = 12100.00m,
    ImporteNeto = 10000.00m,
    ImporteIVA = 2100.00m,
    ImporteNoGravado = 0,
    ImporteExento = 0,
    ImporteTributos = 0,
    
    // Moneda
    MonedaId = "PES",
    MonedaCotizacion = 1
};

// Agregar IVA 21%
factura.AlicuotasIVA.Add(new AlicuotaIVA
{
    Codigo = 5,              // 5 = 21%
    BaseImponible = 10000,
    Importe = 2100
});

// Autorizar
var resultado = await client.AutorizarComprobanteAsync(factura);

if (resultado.Exitoso)
{
    Console.WriteLine($"✅ CAE: {resultado.CAE}");
    Console.WriteLine($"📅 Vencimiento: {resultado.FechaVencimientoCAE:dd/MM/yyyy}");
}
else
{
    Console.WriteLine($"❌ Error: {resultado.MensajeError}");
}
```

## 📖 Ejemplos Completos

### Factura A con Servicios
```csharp
var factura = new Comprobante
{
    PuntoVenta = 1,
    TipoComprobante = 1,
    Numero = 100,
    Concepto = 2,  // 2 = Servicios
    TipoDocumento = 80,
    NumeroDocumento = 30123456789,
    FechaEmision = DateTime.Today,
    
    // Fechas obligatorias para servicios
    FechaServicioDesde = new DateTime(2026, 1, 1),
    FechaServicioHasta = new DateTime(2026, 1, 31),
    FechaVencimientoPago = DateTime.Today.AddDays(10),
    
    ImporteTotal = 12100.00m,
    ImporteNeto = 10000.00m,
    ImporteIVA = 2100.00m,
    
    MonedaId = "PES",
    MonedaCotizacion = 1
};

factura.AlicuotasIVA.Add(new AlicuotaIVA
{
    Codigo = 5,
    BaseImponible = 10000,
    Importe = 2100
});

var resultado = await client.AutorizarComprobanteAsync(factura);
```

### Factura C (Consumidor Final)
```csharp
var factura = new Comprobante
{
    PuntoVenta = 1,
    TipoComprobante = 11,  // 11 = Factura C
    Numero = 200,
    Concepto = 1,
    TipoDocumento = 99,    // 99 = Consumidor Final
    NumeroDocumento = 0,   // 0 para consumidor final
    FechaEmision = DateTime.Today,
    
    ImporteTotal = 10000.00m,
    ImporteNeto = 10000.00m,
    ImporteIVA = 0,         // Factura C no discrimina IVA
    
    MonedaId = "PES",
    MonedaCotizacion = 1
};

// Factura C no lleva alícuotas de IVA
var resultado = await client.AutorizarComprobanteAsync(factura);
```

### Nota de Crédito
```csharp
var notaCredito = new Comprobante
{
    PuntoVenta = 1,
    TipoComprobante = 3,   // 3 = Nota de Crédito A
    Numero = 50,
    Concepto = 1,
    TipoDocumento = 80,
    NumeroDocumento = 30123456789,
    FechaEmision = DateTime.Today,
    
    ImporteTotal = 12100.00m,
    ImporteNeto = 10000.00m,
    ImporteIVA = 2100.00m,
    
    MonedaId = "PES",
    MonedaCotizacion = 1
};

// Asociar a factura original
notaCredito.ComprobantesAsociados.Add(new ComprobanteAsociado
{
    Tipo = 1,           // 1 = Factura A
    PuntoVenta = 1,
    Numero = 100        // Número de factura original
});

notaCredito.AlicuotasIVA.Add(new AlicuotaIVA
{
    Codigo = 5,
    BaseImponible = 10000,
    Importe = 2100
});

var resultado = await client.AutorizarComprobanteAsync(notaCredito);
```

## 🔐 Certificados

### Usar certificado .pfx (recomendado)
```csharp
config.CertificatePath = "certificado.pfx";
config.CertificatePassword = "tu_password";
```

### Usar certificado .crt y .key (solo .NET 5+)
```csharp
config.CertificatePath = "certificado.crt";
config.PrivateKeyPath = "clave_privada.key";
config.CertificatePassword = "password_si_tiene";
```

### Convertir .crt/.key a .pfx

Si estás en .NET Framework 4.8 y tenés archivos separados:
```bash
openssl pkcs12 -export -out certificado.pfx -inkey clave_privada.key -in certificado.crt
```

## 🎯 Códigos ARCA Importantes

### Tipos de Comprobante

| Código | Descripción |
|--------|-------------|
| 1 | Factura A |
| 6 | Factura B |
| 11 | Factura C |
| 3 | Nota de Crédito A |
| 8 | Nota de Crédito B |
| 13 | Nota de Crédito C |

### Tipos de Documento

| Código | Descripción |
|--------|-------------|
| 80 | CUIT |
| 86 | CUIL |
| 96 | DNI |
| 99 | Consumidor Final |

### Conceptos

| Código | Descripción |
|--------|-------------|
| 1 | Productos |
| 2 | Servicios |
| 3 | Productos y Servicios |

### Alícuotas de IVA

| Código | Descripción |
|--------|-------------|
| 3 | 0% |
| 4 | 10.5% |
| 5 | 21% |
| 6 | 27% |

## ⚠️ Manejo de Errores
```csharp
using ARCA.SDK.Exceptions;

try
{
    var resultado = await client.AutorizarComprobanteAsync(factura);
    
    if (!resultado.Exitoso)
    {
        Console.WriteLine($"Error: {resultado.MensajeError}");
        
        if (resultado.Observaciones.Count > 0)
        {
            foreach (var obs in resultado.Observaciones)
            {
                Console.WriteLine($"  - {obs}");
            }
        }
    }
}
catch (ArcaAuthException ex)
{
    // Error de autenticación (certificado, token, etc.)
    Console.WriteLine($"Error de autenticación: {ex.Message}");
}
catch (ArcaValidationException ex)
{
    // Error de validación (datos incorrectos)
    Console.WriteLine($"Error de validación: {ex.Message}");
}
catch (ArcaComprobanteRechazadoException ex)
{
    // Comprobante rechazado por ARCA
    Console.WriteLine($"Comprobante rechazado: {ex.Message}");
    foreach (var obs in ex.Observaciones)
    {
        Console.WriteLine($"  - {obs}");
    }
}
catch (ArcaException ex)
{
    // Error general del SDK
    Console.WriteLine($"Error ARCA: {ex.Message}");
}
```

## 🧪 Testing

El SDK incluye homologación de ARCA para testing:
```csharp
// Usar ambiente de homologación
config.Environment = ArcaEnvironment.Homologacion;

// CUIT de testing de ARCA: 20409378472
// Certificado de testing disponible en ARCA
```

## 🏗️ Arquitectura
```
ARCA.SDK/
├── ARCA.SDK.Core/          # Librería principal
│   ├── Models/             # Modelos de datos
│   ├── Services/           # Lógica de negocio
│   ├── Clients/            # Clientes SOAP
│   ├── Configuration/      # Configuración
│   ├── Exceptions/         # Excepciones personalizadas
│   └── Utils/              # Utilidades
├── ARCA.SDK.COM/           # Wrapper COM (para VFP/VB6)
└── ARCA.SDK.Tests/         # Tests unitarios
```

## 🤝 Contribuir

Ver [CONTRIBUTING.md](CONTRIBUTING.md) para guía de desarrollo.

## 📄 Licencia

Este proyecto está bajo la licencia MIT. Ver [LICENSE](LICENSE) para más detalles.

## 🔗 Links Útiles

- [Documentación oficial ARCA](https://www.arca.gob.ar/ws/)
- [Especificación WSAA](https://www.afip.gob.ar/ws/documentacion/ws-autenticacion-y-autorizacion.asp)
- [Especificación WSFE](https://www.afip.gob.ar/ws/documentacion/ws-factura-electronica.asp)

## ⭐ Estado del Proyecto

**Versión actual:** 0.1.0-alpha

### Implementado

- ✅ Autenticación WSAA
- ✅ Autorización de comprobantes WSFE
- ✅ Consulta de último comprobante
- ✅ Soporte multi-targeting
- ✅ Manejo de certificados

### Próximamente

- 🔜 Consulta de cotizaciones
- 🔜 Consulta de padrones
- 🔜 WSFEX (facturación exportación)
- 🔜 CDC (constatación de comprobantes)
- 🔜 Wrapper COM completo
- 🔜 Publicación en NuGet

## 💬 Soporte

¿Encontraste un bug? ¿Tenés una sugerencia? [Abrí un issue](https://github.com/tu-usuario/ARCA.SDK/issues)

---

**Hecho con ❤️ para la comunidad de desarrolladores argentinos**