using HelpDesk.Api.Errores;

namespace HelpDesk.Api.Configuracion;

/// <summary>
/// Registra los controladores, el JSON del contrato y la traducción de errores a Problem Details (ADR-004, RNF-03).
/// </summary>
public static class InyeccionPresentacion
{
    public static IServiceCollection AgregarPresentacion(this IServiceCollection services)
    {
        services
            .AddControllers(options =>
            {
                // Toda la validación la hace el validador desde el handler, en el orden de la decisión 92: MVC no
                // responde 400 por un campo obligatorio ausente antes del 403.
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                options.Filters.Add(new FiltroContentTypeAusente());
            })
            .AddJsonOptions(options =>
            {
                ConfiguracionJson.Aplicar(options.JsonSerializerOptions);

                // ModelState conserva la JsonException, que RespuestaModeloInvalido necesita (decisión 93).
                options.AllowInputFormatterExceptionMessages = false;
            })
            .ConfigureApiBehaviorOptions(options =>
                options.InvalidModelStateResponseFactory = RespuestaModeloInvalido.Crear);

        // Los Problem Details que escribe IProblemDetailsService usan las mismas opciones.
        services.ConfigureHttpJsonOptions(options => ConfiguracionJson.Aplicar(options.SerializerOptions));

        // traceId en todos; detail e instance no forman parte del contrato (decisión 62).
        services.AddProblemDetails(options => options.CustomizeProblemDetails = contexto =>
        {
            contexto.ProblemDetails.Detail = null;
            contexto.ProblemDetails.Instance = null;
        });

        services.AddExceptionHandler<ManejadorExcepciones>();

        return services;
    }
}
