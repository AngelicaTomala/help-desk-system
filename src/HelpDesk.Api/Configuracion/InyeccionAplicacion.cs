using FluentValidation;
using HelpDesk.Api.Identidad;
using HelpDesk.Application.Abstracciones;
using HelpDesk.Application.Tickets.Crear;
using HelpDesk.Application.Tickets.Reclasificar;

namespace HelpDesk.Api.Configuracion;

/// <summary>
/// Registra los casos de uso de Application, uno por uno y sin escaneo de ensamblados. Cada caso de uso nuevo
/// agrega aquí su handler y su validador.
/// </summary>
public static class InyeccionAplicacion
{
    public static IServiceCollection AgregarAplicacion(this IServiceCollection services)
    {
        // Reloj del sistema; las pruebas lo reemplazan por FakeTimeProvider (RNF-08).
        services.AddSingleton(TimeProvider.System);

        // Usuario de la petición; lo fija el middleware de X-Usuario-Id (decisión 97).
        services.AddScoped<UsuarioActual>();
        services.AddScoped<IUsuarioActual>(provider => provider.GetRequiredService<UsuarioActual>());

        services.AddScoped<IValidator<CrearTicketComando>, CrearTicketValidator>();
        services.AddScoped<CrearTicketHandler>();

        services.AddScoped<IValidator<ReclasificarTicketComando>, ReclasificarTicketValidator>();
        services.AddScoped<ReclasificarTicketHandler>();

        return services;
    }
}
