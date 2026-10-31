using System.Reflection;
using HelpDesk.Application.Abstracciones;
using HelpDesk.Domain.Entidades;
using HelpDesk.Infrastructure.Persistencia;

namespace HelpDesk.Tests.Arquitectura;

/// <summary>
/// Ensamblados y espacios de nombres de las capas de C4 §3 (RNF-07).
/// </summary>
internal static class Ensamblados
{
    public const string NamespaceDomain = "HelpDesk.Domain";
    public const string NamespaceApplication = "HelpDesk.Application";
    public const string NamespaceInfrastructure = "HelpDesk.Infrastructure";
    public const string NamespaceApi = "HelpDesk.Api";

    public const string NamespaceEfCore = "Microsoft.EntityFrameworkCore";
    public const string NamespaceAspNetCore = "Microsoft.AspNetCore";
    public const string NamespaceFluentValidation = "FluentValidation";

    public static readonly Assembly CapaDomain = typeof(Ticket).Assembly;
    public static readonly Assembly CapaApplication = typeof(IGeneradorCodigoTicket).Assembly;
    public static readonly Assembly CapaInfrastructure = typeof(HelpDeskDbContext).Assembly;

    // Por nombre: no depende de la visibilidad de Program (top-level statements).
    public static readonly Assembly CapaApi = Assembly.Load("HelpDesk.Api");
}
