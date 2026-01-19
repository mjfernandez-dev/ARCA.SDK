# Guía de Contribución y Arquitectura - ARCA.SDK

Esta guía está dirigida a desarrolladores que quieren entender la arquitectura interna del SDK, contribuir al proyecto, o mantenerlo en el futuro.

## 📋 Tabla de Contenidos

1. [Estructura del Proyecto](#estructura-del-proyecto)
2. [Arquitectura Técnica](#arquitectura-técnica)
3. [Flujo de Autenticación](#flujo-de-autenticación)
4. [Flujo de Autorización](#flujo-de-autorización)
5. [Decisiones de Diseño](#decisiones-de-diseño)
6. [Guía de Desarrollo](#guía-de-desarrollo)
7. [Testing](#testing)
8. [Deployment](#deployment)

---

## 🏗️ Estructura del Proyecto
```
ARCA.SDK/
├── ARCA.SDK.sln                    # Solución principal
│
├── ARCA.SDK.Core/                  # ⭐ Proyecto principal
│   ├── Configuration/
│   │   └── ArcaConfig.cs           # Configuración del cliente
│   │
│   ├── Models/                     # Modelos de dominio
│   │   ├── Comprobante.cs          # Modelo principal de factura
│   │   ├── AlicuotaIVA.cs         # Alícuota de IVA
│   │   ├── Tributo.cs              # Tributos adicionales
│   │   ├── ComprobanteAsociado.cs  # Para notas de crédito/débito
│   │   └── AutorizacionResult.cs   # Resultado de autorización
│   │
│   ├── Services/                   # Lógica de negocio
│   │   ├── AuthService.cs          # Autenticación WSAA
│   │   ├── AuthCache.cs            # Caché de tokens
│   │   ├── FacturacionService.cs   # Facturación WSFE
│   │   └── LoginTicketRequest.cs   # Generación de TRA
│   │
│   ├── Clients/                    # Clientes SOAP
│   │   ├── WsaaClient.cs           # Cliente WSAA
│   │   ├── WsaaModels.cs           # Modelos WSAA
│   │   ├── WsfeClient.cs           # Cliente WSFE
│   │   └── WsfeModels.cs           # Modelos WSFE
│   │
│   ├── Exceptions/                 # Excepciones personalizadas
│   │   ├── ArcaException.cs        # Base
│   │   ├── ArcaAuthException.cs    # Autenticación
│   │   ├── ArcaValidationException.cs
│   │   └── ArcaComprobanteRechazadoException.cs
│   │
│   ├── Utils/
│   │   └── CertificateHelper.cs    # Manejo de certificados
│   │
│   ├── ArcaClient.cs               # 🎯 Punto de entrada público
│   └── ArcaClientFactory.cs        # Factory para creación
│
├── ARCA.SDK.COM/                   # Wrapper COM (futuro)
│   └── (para Visual FoxPro/VB6)
│
└── ARCA.SDK.Tests/                 # Tests unitarios
    ├── ArcaClientTests.cs
    └── AuthServiceTests.cs
```

---

## 🔧 Arquitectura Técnica

### Capas del SDK
```
┌─────────────────────────────────────────────────────────┐
│                    CAPA PÚBLICA                         │
│  ArcaClient, ArcaClientFactory, Models                  │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                CAPA DE SERVICIOS                        │
│  AuthService, FacturacionService                        │
│  (Lógica de negocio, validaciones, transformaciones)   │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│               CAPA DE CLIENTES SOAP                     │
│  WsaaClient, WsfeClient                                 │
│  (Construcción SOAP, parsing XML, HTTP)                 │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                SERVICIOS DE ARCA                        │
│  WSAA (autenticación), WSFE (facturación)               │
└─────────────────────────────────────────────────────────┘
```

### Responsabilidades por Capa

**ArcaClient (Pública)**
- API simple y limpia para el usuario
- Coordina servicios internos
- No contiene lógica de negocio

**Services (Interna)**
- Lógica de negocio
- Validaciones
- Conversión entre modelos del SDK y modelos SOAP
- Caché de tokens

**Clients (Interna)**
- Construcción de mensajes SOAP
- Parsing de respuestas XML
- Manejo de HTTP

---

## 🔐 Flujo de Autenticación (WSAA)

### Diagrama de Secuencia
```
Usuario → ArcaClient → FacturacionService → AuthService → WsaaClient → ARCA
                                                ↓
                                          AuthCache
                                                ↓
                                       CertificateHelper
                                                ↓
                                      LoginTicketRequest
```

### Paso a Paso

1. **Usuario solicita autorización de comprobante**
```csharp
   await client.AutorizarComprobanteAsync(factura);
```

2. **FacturacionService necesita credenciales**
```csharp
   var (token, sign) = await _authService.ObtenerCredencialesAsync("wsfe");
```

3. **AuthService verifica caché**
   - Clave de caché: `{CUIT}_{servicio}_{ambiente}`
   - Si existe y no expiró (margen 5 min) → retorna del caché
   - Si no existe o expiró → continúa autenticación

4. **AuthService carga certificado** (primera vez)
```csharp
   _certificate = CertificateHelper.LoadCertificate(
       _config.CertificatePath,
       _config.PrivateKeyPath,
       _config.CertificatePassword
   );
```

5. **AuthService genera TRA (LoginTicketRequest)**
```csharp
   var tra = LoginTicketRequest.Generate(servicio, cuit, _certificate);
```
   
   Internamente:
   - Genera XML con estructura requerida por ARCA
   - Firma el XML con la clave privada del certificado (RSA + SHA256)
   - Incluye el certificado público en el XML

6. **WsaaClient envía TRA al WSAA**
```
   POST https://wsaahomo.afip.gov.ar/ws/services/LoginCms
   Content-Type: text/xml
   
   <soap:Envelope>
     <soap:Body>
       <wsaa:loginCms>
         <wsaa:in0>[TRA FIRMADO]</wsaa:in0>
       </wsaa:loginCms>
     </soap:Body>
   </soap:Envelope>
```

7. **WSAA valida y retorna credenciales**
```xml
   <loginCmsReturn>
     <token>PD94bWw...</token>
     <sign>YmFzZTY0...</sign>
     <expirationTime>2026-01-19T12:00:00.000-03:00</expirationTime>
   </loginCmsReturn>
```

8. **AuthService cachea las credenciales**
```csharp
   _cache.Set(cacheKey, token, sign, expiration);
```

9. **Token y Sign listos para usar en WSFE**

### Tiempos de Vida

- **Token WSAA**: ~12 horas (depende de ARCA)
- **Caché interno**: Hasta 5 minutos antes de expiración
- **Certificado en memoria**: Mientras viva la instancia de AuthService

---

## 📋 Flujo de Autorización (WSFE)

### Diagrama de Secuencia
```
Usuario → ArcaClient → FacturacionService → AuthService (token/sign)
                            ↓
                       Conversión modelo
                            ↓
                       WsfeClient
                            ↓
                         WSFE ARCA
                            ↓
                     Parseo respuesta
                            ↓
                     AutorizacionResult
```

### Paso a Paso

1. **Usuario crea comprobante**
```csharp
   var factura = new Comprobante
   {
       PuntoVenta = 1,
       TipoComprobante = 1,
       // ... más datos
   };
```

2. **FacturacionService valida**
```csharp
   ValidarComprobante(comprobante);
```
   
   Validaciones:
   - Punto de venta > 0
   - Tipo de comprobante > 0
   - Importe total > 0
   - Si concepto = 2 o 3: fechas de servicio obligatorias

3. **FacturacionService obtiene credenciales**
```csharp
   var (token, sign) = await _authService.ObtenerCredencialesAsync("wsfe");
```

4. **Conversión a modelo WSFE**
```csharp
   var wsfeComprobante = ConvertirAWsfeComprobante(comprobante);
```
   
   Transformaciones:
   - `DateTime` → `"yyyyMMdd"` (string)
   - `List<AlicuotaIVA>` → `WsfeAlicuotaIVA[]`
   - `decimal` → formato con 2 decimales

5. **WsfeClient construye SOAP request**
```xml
   <ar:FECAESolicitar>
     <ar:Auth>
       <ar:Token>...</ar:Token>
       <ar:Sign>...</ar:Sign>
       <ar:Cuit>...</ar:Cuit>
     </ar:Auth>
     <ar:FeCAEReq>
       <ar:FeCabReq>
         <ar:CantReg>1</ar:CantReg>
         <ar:PtoVta>1</ar:PtoVta>
         <ar:CbteTipo>1</ar:CbteTipo>
       </ar:FeCabReq>
       <ar:FeDetReq>
         <!-- Detalles del comprobante -->
       </ar:FeDetReq>
     </ar:FeCAEReq>
   </ar:FECAESolicitar>
```

6. **WSFE procesa y retorna**
```xml
   <FECAEDetResponse>
     <Resultado>A</Resultado>  <!-- A=Aprobado, R=Rechazado -->
     <CAE>72081816325877</CAE>
     <CAEFchVto>20260128</CAEFchVto>
     <CbteDesde>100</CbteDesde>
   </FECAEDetResponse>
```

7. **FacturacionService parsea y crea resultado**
```csharp
   return new AutorizacionResult
   {
       Exitoso = true,
       CAE = "72081816325877",
       FechaVencimientoCAE = new DateTime(2026, 1, 28),
       NumeroComprobante = 100
   };
```

---

## 🎯 Decisiones de Diseño

### 1. Multi-Targeting

**Decisión:** Soportar `netstandard2.0`, `net48` y `net8.0`

**Razón:**
- `netstandard2.0`: Máxima compatibilidad (funciona en .NET Framework y .NET Core)
- `net48`: Compatibilidad con sistemas legacy argentinos
- `net8.0`: Aprovechar características modernas

**Implicación:**
- Compilación condicional con `#if NET5_0_OR_GREATER`
- No podemos usar todas las APIs modernas
- Certificados .crt/.key solo funcionan en .NET 5+

### 2. Async/Await Obligatorio

**Decisión:** Toda la API pública es async

**Razón:**
- Las llamadas SOAP son I/O-bound (red)
- Evita bloquear threads
- Permite `CancellationToken` para cancelar operaciones

**Implicación:**
- El usuario DEBE usar `await`
- No hay versiones síncronas de los métodos

### 3. Caché de Tokens Automático

**Decisión:** `AuthService` cachea tokens automáticamente

**Razón:**
- Evita llamadas innecesarias al WSAA
- Mejora performance (autenticación es lenta)
- Transparente para el usuario

**Implicación:**
- Usa `ConcurrentDictionary` (thread-safe)
- Margen de 5 minutos antes de expiración
- El usuario puede limpiar caché manualmente si necesita

### 4. Excepciones Tipadas

**Decisión:** Excepciones específicas por tipo de error

**Razón:**
- Permite `catch` específicos
- Mejor debugging
- Mensajes de error claros

**Jerarquía:**
```
ArcaException (base)
├── ArcaAuthException
├── ArcaValidationException
└── ArcaComprobanteRechazadoException
```

### 5. Models Ricos vs DTOs Simples

**Decisión:** Modelos con listas y objetos complejos

**Razón:**
- API más intuitiva
- IntelliSense ayuda al desarrollador
- Validaciones en tiempo de compilación

**Ejemplo:**
```csharp
// ✅ Modelo rico
factura.AlicuotasIVA.Add(new AlicuotaIVA { ... });

// ❌ DTO simple (rechazado)
factura.IvaId = 5;
factura.IvaBase = 1000;
```

### 6. Servicios Internos

**Decisión:** `AuthService`, `FacturacionService` son `internal`

**Razón:**
- El usuario solo interactúa con `ArcaClient`
- Flexibilidad para cambiar implementación interna
- API pública más simple

**Excepción:**
- Tests pueden acceder vía `InternalsVisibleTo`

---

## 🛠️ Guía de Desarrollo

### Configurar Entorno

1. **Requisitos**
   - Visual Studio 2022 (o VS Code + .NET SDK)
   - .NET 8 SDK
   - Git

2. **Clonar**
```bash
   git clone https://github.com/tu-usuario/ARCA.SDK.git
   cd ARCA.SDK
```

3. **Restaurar dependencias**
```bash
   dotnet restore
```

4. **Compilar**
```bash
   dotnet build
```

5. **Ejecutar tests**
```bash
   dotnet test
```

### Agregar un Nuevo Servicio de ARCA

Ejemplo: Agregar servicio de Cotizaciones

1. **Crear modelos en `Clients/`**
```csharp
   // WsfexModels.cs
   internal class WsfexCotizacionResponse
   {
       public string? MonId { get; set; }
       public decimal MonCotiz { get; set; }
   }
```

2. **Crear cliente SOAP en `Clients/`**
```csharp
   // WsfexClient.cs
   internal class WsfexClient
   {
       public async Task<decimal> ConsultarCotizacionAsync(...)
       {
           // Implementación
       }
   }
```

3. **Crear servicio en `Services/`**
```csharp
   // CotizacionService.cs
   internal class CotizacionService
   {
       private readonly WsfexClient _client;
       
       public async Task<decimal> ObtenerCotizacionAsync(...)
       {
           // Lógica
       }
   }
```

4. **Exponer en `ArcaClient`**
```csharp
   public class ArcaClient
   {
       private readonly CotizacionService _cotizacionService;
       
       public async Task<decimal> ConsultarCotizacionAsync(...)
       {
           return await _cotizacionService.ObtenerCotizacionAsync(...);
       }
   }
```

5. **Agregar tests**
```csharp
   // CotizacionServiceTests.cs
```

### Convenciones de Código

**Nombres:**
- Clases públicas: `PascalCase`
- Métodos: `PascalCase` + `Async` si es asíncrono
- Parámetros: `camelCase`
- Constantes: `UPPER_CASE`

**Comentarios XML:**
```csharp
/// <summary>
/// Descripción breve
/// </summary>
/// <param name="nombre">Descripción del parámetro</param>
/// <returns>Qué retorna</returns>
```

**Excepciones:**
```csharp
// ✅ Lanzar excepción tipada
throw new ArcaValidationException("CUIT inválido");

// ❌ NO lanzar Exception genérica
throw new Exception("Error");
```

---

## 🧪 Testing

### Estructura de Tests
```
ARCA.SDK.Tests/
├── ArcaClientTests.cs          # Tests del cliente público
├── AuthServiceTests.cs         # Tests de autenticación
└── (futuros)
    ├── FacturacionServiceTests.cs
    └── IntegrationTests.cs
```

### Tipos de Tests

**1. Tests Unitarios**
- Validan lógica aislada
- Usan mocks cuando es necesario
- Rápidos y determinísticos

**2. Tests de Integración** (futuro)
- Conectan con ambiente de homologación de ARCA
- Requieren certificado de testing
- Más lentos, se ejecutan manualmente

### Ejecutar Tests
```bash
# Todos los tests
dotnet test

# Con detalle
dotnet test --logger "console;verbosity=detailed"

# Solo una clase
dotnet test --filter "FullyQualifiedName~ArcaClientTests"
```

### Agregar un Test
```csharp
[Fact]
public async Task MetodoX_ConCondicionY_RetornaZ()
{
    // Arrange (preparar)
    var config = new ArcaConfig { ... };
    var client = new ArcaClient(config);
    
    // Act (ejecutar)
    var resultado = await client.MetodoX();
    
    // Assert (verificar)
    Assert.NotNull(resultado);
    Assert.Equal(valorEsperado, resultado);
}
```

---

## 🚀 Deployment

### Empaquetar NuGet

1. **Actualizar versión en `.csproj`**
```xml
   <Version>0.2.0-alpha</Version>
```

2. **Compilar en Release**
```bash
   dotnet build -c Release
```

3. **Crear paquete**
```bash
   dotnet pack -c Release
```

4. **Publicar a NuGet** (cuando esté listo)
```bash
   dotnet nuget push bin/Release/ARCA.SDK.0.2.0-alpha.nupkg -s https://api.nuget.org/v3/index.json -k [API_KEY]
```

### Versionado Semántico

Usamos [SemVer](https://semver.org/):
- **MAJOR**: Cambios incompatibles en API pública
- **MINOR**: Nuevas funcionalidades compatibles
- **PATCH**: Bug fixes

Ejemplos:
- `0.1.0-alpha`: Primera versión alpha
- `0.2.0-alpha`: Nueva funcionalidad
- `1.0.0`: Primera versión estable
- `1.0.1`: Bug fix
- `1.1.0`: Nueva funcionalidad

---

## 📝 Checklist para Pull Requests

- [ ] Código compila sin errores ni warnings
- [ ] Tests existentes pasan
- [ ] Nuevos tests agregados para código nuevo
- [ ] Comentarios XML en métodos públicos
- [ ] README.md actualizado si hay cambios en API pública
- [ ] CONTRIBUTING.md actualizado si hay cambios arquitectónicos
- [ ] Commit messages claros y descriptivos

---

## 🐛 Debugging Tips

### Ver requests/responses SOAP

Agregar en `WsaaClient` o `WsfeClient`:
```csharp
// Antes de enviar
Console.WriteLine("REQUEST:");
Console.WriteLine(soapRequest);

// Después de recibir
Console.WriteLine("RESPONSE:");
Console.WriteLine(responseContent);
```

### Ver tokens cacheados

En `AuthService`:
```csharp
public void DebugCache()
{
    // Agregar método temporal para inspeccionar caché
}
```

### Problemas comunes

**"No se pudo autenticar"**
- Verificar que el certificado sea válido
- Verificar CUIT
- Verificar ambiente (homologación vs producción)

**"Comprobante rechazado"**
- Ver observaciones en `resultado.Observaciones`
- Verificar que importes sumen correctamente
- Verificar que alícuotas de IVA sean correctas

**"Token expirado"**
- Limpiar caché: `client.LimpiarCache()`

---

## 📚 Recursos

- [Documentación WSAA](https://www.afip.gob.ar/ws/documentacion/ws-autenticacion-y-autorizacion.asp)
- [Documentación WSFE](https://www.afip.gob.ar/ws/documentacion/ws-factura-electronica.asp)
- [Especificación XML Signature](https://www.w3.org/TR/xmldsig-core/)

---

## 🙋 FAQ para Desarrolladores

**P: ¿Por qué no usamos WCF?**
R: WCF no es compatible con .NET Core/.NET 5+. Construimos SOAP manualmente para compatibilidad total.

**P: ¿Por qué HttpClient en vez de WebRequest?**
R: `HttpClient` es async-first y la forma moderna recomendada por Microsoft.

**P: ¿Puedo contribuir?**
R: ¡Sí! Abrí un issue primero para discutir grandes cambios.

**P: ¿Cuándo saldrá versión 1.0?**
R: Cuando tengamos cobertura completa de WSFE, tests de integración, y usuarios lo hayan probado en producción.

---

**¡Gracias por contribuir a ARCA.SDK!** 🇦🇷