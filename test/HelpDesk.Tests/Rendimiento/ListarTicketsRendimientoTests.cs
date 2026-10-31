using System.Diagnostics;
using System.Net;
using System.Text.Json;
using HelpDesk.Api.Tickets;
using HelpDesk.Domain.Entidades;
using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Tests.Api;
using HelpDesk.Tests.Integracion;
using Xunit.Abstractions;

namespace HelpDesk.Tests.Rendimiento;

/// <summary>
/// Prueba explícita de RNF-06 (decisión 110): con 10 000 tickets en la base de pruebas,
/// <c>GET /api/v1/tickets?pageSize=50</c> responde en menos de 1 s de promedio en 10 llamadas. Mide con el supervisor y
/// sin filtros, el peor caso: cuenta y ordena todos los tickets. Antes y después se borran los tickets (decisión 99).
/// Verifica un RNF, no un CA ni una RN: va sin Trait (decisión 36).
/// </summary>
[Collection(BaseDePruebasColeccion.Nombre)]
public class ListarTicketsRendimientoTests(
    HelpDeskApiFactory factory,
    BaseDePruebasFixture baseDePruebas,
    ITestOutputHelper salida) : IClassFixture<HelpDeskApiFactory>, IAsyncLifetime
{
    private const int CantidadTickets = 10_000;
    private const int Llamadas = 10;
    private const int TamanoPagina = 50;
    private const string Supervisor = "10";
    private const string Ruta = "/" + TicketsController.Ruta + "?pageSize=50";

    private static readonly TimeSpan Limite = TimeSpan.FromSeconds(1);
    private static readonly Prioridad[] Prioridades = Enum.GetValues<Prioridad>();
    private static readonly Categoria[] Categorias = Enum.GetValues<Categoria>();

    public Task InitializeAsync() => baseDePruebas.ReiniciarTicketsAsync();

    public Task DisposeAsync() => baseDePruebas.ReiniciarTicketsAsync();

    [PruebaExplicita]
    public async Task Listar_DiezMilTickets_PromedioMenorQueUnSegundo()
    {
        var siembra = Stopwatch.StartNew();
        await SembrarAsync();
        salida.WriteLine($"Siembra de {CantidadTickets} tickets: {siembra.ElapsedMilliseconds} ms");

        var cliente = factory.CrearClienteIdentificado(Supervisor);

        // Calentamiento fuera del promedio: JIT, modelo de EF Core y pool de conexiones.
        await ListarAsync(cliente);

        var tiempos = new List<TimeSpan>();
        for (var llamada = 1; llamada <= Llamadas; llamada++)
        {
            var reloj = Stopwatch.StartNew();
            await ListarAsync(cliente);
            tiempos.Add(reloj.Elapsed);
            salida.WriteLine($"Llamada {llamada}: {reloj.Elapsed.TotalMilliseconds:F1} ms");
        }

        var promedio = TimeSpan.FromTicks((long)tiempos.Average(tiempo => tiempo.Ticks));
        salida.WriteLine($"Promedio de {Llamadas} llamadas: {promedio.TotalMilliseconds:F1} ms (límite {Limite.TotalMilliseconds} ms)");

        Assert.True(promedio < Limite, $"El promedio fue {promedio.TotalMilliseconds:F1} ms.");
    }

    /// <summary>
    /// Tickets válidos construidos con Domain: solicitantes 1 a 5, prioridades y categorías rotadas, creados un minuto
    /// antes uno de otro; uno de cada tres asignado a los técnicos 6 a 8. Se guardan con un solo <c>SaveChanges</c>.
    /// </summary>
    private async Task SembrarAsync()
    {
        var referencia = factory.Reloj.GetUtcNow().UtcDateTime;
        var tickets = new List<Ticket>(CantidadTickets);

        for (var numero = 1; numero <= CantidadTickets; numero++)
        {
            var creacion = referencia.AddMinutes(-numero);
            var ticket = Ticket.Crear(
                $"HD-{numero:D6}",
                $"Ticket de rendimiento {numero}",
                "Ticket sembrado para la prueba de rendimiento de RNF-06.",
                Categorias[numero % Categorias.Length],
                Prioridades[numero % Prioridades.Length],
                solicitanteId: 1 + numero % 5,
                creacion);

            if (numero % 3 == 0)
            {
                ticket.Asignar(tecnicoId: 6 + numero % 3, creacion.AddSeconds(30));
            }

            tickets.Add(ticket);
        }

        await using var context = baseDePruebas.CrearContexto();
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        context.Tickets.AddRange(tickets);
        await context.SaveChangesAsync();
    }

    private static async Task ListarAsync(HttpClient cliente)
    {
        var respuesta = await cliente.GetAsync(Ruta);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        using var pagina = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        Assert.Equal(TamanoPagina, pagina.RootElement.GetProperty("items").GetArrayLength());
        Assert.Equal(CantidadTickets, pagina.RootElement.GetProperty("totalItems").GetInt32());
    }
}
