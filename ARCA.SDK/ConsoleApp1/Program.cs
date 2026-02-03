using ARCA.SDK;
using ARCA.SDK.Configuration;
using ARCA.SDK.Exceptions;
using ARCA.SDK.Models;

Console.WriteLine("=== ARCA.SDK - Test de Integración ===\n");

// ============================================
// CONFIGURACIÓN - COMPLETÁ CON TUS DATOS
// ============================================

var config = new ArcaConfig
{
    Environment = ArcaEnvironment.Homologacion,
    Cuit = 20396127823,
    CertificatePath = @"D:\04_Descargas\certificado.crt",
    PrivateKeyPath = @"D:\04_Descargas\clave.key",
    CertificatePassword = ""
};

// Validar que completaste los datos
if (config.Cuit == 0)
{
    Console.WriteLine("❌ ERROR: Debés completar tu CUIT en el código");
    Console.WriteLine("Presioná Enter para salir...");
    Console.ReadLine();
    return;
}

if (string.IsNullOrEmpty(config.CertificatePath))
{
    Console.WriteLine("❌ ERROR: Debés completar la ruta al certificado");
    Console.WriteLine("Presioná Enter para salir...");
    Console.ReadLine();
    return;
}

if (!File.Exists(config.CertificatePath))
{
    Console.WriteLine($"❌ ERROR: No se encontró el certificado en: {config.CertificatePath}");
    Console.WriteLine("Presioná Enter para salir...");
    Console.ReadLine();
    return;
}

Console.WriteLine($"CUIT: {config.Cuit}");
Console.WriteLine($"Certificado: {config.CertificatePath}");
Console.WriteLine($"Ambiente: {config.Environment}\n");

try
{
    // ============================================
    // TEST 1: Crear Cliente
    // ============================================
    Console.WriteLine("📦 Creando cliente ARCA...");
    var client = ArcaClientFactory.Create(config);
    Console.WriteLine("✅ Cliente creado exitosamente\n");

    // ============================================
    // TEST 2: Consultar Último Comprobante
    // ============================================
    Console.WriteLine("📋 TEST 2: Consultando último comprobante...");
    Console.Write("Punto de venta (ej: 1): ");
    var puntoVentaStr = Console.ReadLine();

    if (!int.TryParse(puntoVentaStr, out int puntoVenta) || puntoVenta <= 0)
    {
        Console.WriteLine("❌ Punto de venta inválido");
        return;
    }

    Console.Write("Tipo de comprobante (1=Factura A, 6=Factura B, 11=Factura C): ");
    var tipoComprobanteStr = Console.ReadLine();

    if (!int.TryParse(tipoComprobanteStr, out int tipoComprobante) || tipoComprobante <= 0)
    {
        Console.WriteLine("❌ Tipo de comprobante inválido");
        return;
    }

    Console.WriteLine($"\nConsultando último comprobante para PV:{puntoVenta} Tipo:{tipoComprobante}...");

    var ultimoNumero = await client.ObtenerUltimoComprobanteAsync(puntoVenta, tipoComprobante);

    Console.WriteLine($"✅ Último número autorizado: {ultimoNumero}");
    Console.WriteLine($"   Próximo número a usar: {ultimoNumero + 1}\n");

    // ============================================
    // TEST 3: Autorizar Comprobante
    // ============================================
    Console.WriteLine("📝 TEST 3: ¿Querés autorizar un comprobante de prueba? (S/N): ");
    var respuesta = Console.ReadLine()?.ToUpper();

    if (respuesta == "S")
    {
        var factura = new Comprobante
        {
            PuntoVenta = puntoVenta,
            TipoComprobante = tipoComprobante,
            Numero = ultimoNumero + 1,
            Concepto = 1,  // 1 = Productos
            TipoDocumento = 99,  // 99 = Consumidor Final (más seguro para testing)
            NumeroDocumento = 0,
            FechaEmision = DateTime.Today,

            // Importes de prueba
            ImporteTotal = 1000.00m,
            ImporteNeto = 1000.00m,
            ImporteIVA = 0,  // Factura C no discrimina IVA si es tipo 11
            ImporteNoGravado = 0,
            ImporteExento = 0,
            ImporteTributos = 0,

            MonedaId = "PES",
            MonedaCotizacion = 1,
            CondicionIVAReceptor = 5
        };

        // Si es Factura A o B (no C), agregar IVA
        if (tipoComprobante == 1 || tipoComprobante == 6)
        {
            factura.ImporteNeto = 826.45m;  // Base imponible
            factura.ImporteIVA = 173.55m;   // IVA 21%
            factura.ImporteTotal = 1000.00m;

            factura.AlicuotasIVA.Add(new AlicuotaIVA
            {
                Codigo = 5,  // 5 = 21%
                BaseImponible = 826.45m,
                Importe = 173.55m
            });
        }

        Console.WriteLine("\n🚀 Autorizando comprobante...");
        Console.WriteLine($"   PV: {factura.PuntoVenta}");
        Console.WriteLine($"   Tipo: {factura.TipoComprobante}");
        Console.WriteLine($"   Número: {factura.Numero}");
        Console.WriteLine($"   Importe: ${factura.ImporteTotal:N2}\n");

        var resultado = await client.AutorizarComprobanteAsync(factura);

        if (resultado.Exitoso)
        {
            Console.WriteLine("✅ ¡COMPROBANTE AUTORIZADO!");
            Console.WriteLine($"   CAE: {resultado.CAE}");
            Console.WriteLine($"   Vencimiento CAE: {resultado.FechaVencimientoCAE:dd/MM/yyyy}");
            Console.WriteLine($"   Número: {resultado.NumeroComprobante}");

            if (resultado.Observaciones.Count > 0)
            {
                Console.WriteLine("\n⚠️  Observaciones:");
                foreach (var obs in resultado.Observaciones)
                {
                    Console.WriteLine($"   - {obs}");
                }
            }
        }
        else
        {
            Console.WriteLine("❌ COMPROBANTE RECHAZADO");
            Console.WriteLine($"   Error: {resultado.MensajeError}");
            Console.WriteLine($"   Código: {resultado.CodigoError}");

            if (resultado.Observaciones.Count > 0)
            {
                Console.WriteLine("\n   Observaciones:");
                foreach (var obs in resultado.Observaciones)
                {
                    Console.WriteLine($"   - {obs}");
                }
            }
        }
    }

    Console.WriteLine("\n✅ Testing completado exitosamente");
}
catch (ArcaAuthException ex)
{
    Console.WriteLine($"\n❌ ERROR DE AUTENTICACIÓN:");
    Console.WriteLine($"   {ex.Message}");
    if (!string.IsNullOrEmpty(ex.CodigoError))
        Console.WriteLine($"   Código: {ex.CodigoError}");
}
catch (ArcaValidationException ex)
{
    Console.WriteLine($"\n❌ ERROR DE VALIDACIÓN:");
    Console.WriteLine($"   {ex.Message}");
}
catch (ArcaException ex)
{
    Console.WriteLine($"\n❌ ERROR ARCA:");
    Console.WriteLine($"   {ex.Message}");
    if (!string.IsNullOrEmpty(ex.CodigoError))
        Console.WriteLine($"   Código: {ex.CodigoError}");
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ ERROR INESPERADO:");
    Console.WriteLine($"   {ex.Message}");
    Console.WriteLine($"\n   Stack trace:");
    Console.WriteLine($"   {ex.StackTrace}");
}

Console.WriteLine("\nPresioná Enter para salir...");
Console.ReadLine();