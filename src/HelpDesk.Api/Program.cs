using HelpDesk.Api.Configuracion;
using HelpDesk.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AgregarPresentacion();
builder.Services.AgregarAplicacion();
builder.Services.AgregarInfraestructura(builder.Configuration);
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
