using HelpDesk.Api.Configuracion;
using HelpDesk.Api.Identidad;
using HelpDesk.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AgregarPresentacion();
builder.Services.AgregarAplicacion();
builder.Services.AgregarInfraestructura(builder.Configuration);
// Documento OpenAPI de RNF-09; Swagger UI lo lee en Development.
builder.Services.AgregarDocumentacion();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Primero: traduce las excepciones a Problem Details (ADR-004) y da cuerpo a los 404 y 405 del enrutamiento (decisión 94).
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "HelpDesk API v1"));
}

app.UseHttpsRedirection();

// El middleware de X-Usuario-Id necesita el endpoint elegido y va antes de la acción (orden 401 → 404, §1.4).
app.UseRouting();
app.UseUsuarioActual();

app.UseAuthorization();

app.MapControllers();

app.Run();
