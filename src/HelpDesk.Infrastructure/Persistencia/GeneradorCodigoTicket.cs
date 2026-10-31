using HelpDesk.Application.Abstracciones;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Persistencia;

/// <summary>
/// Obtiene el siguiente valor de la SEQUENCE <c>dbo.CodigoTicket</c>, que garantiza códigos sin
/// duplicados aunque con huecos (RN-03, Enmienda de ADR-002).
/// </summary>
public class GeneradorCodigoTicket(HelpDeskDbContext context) : IGeneradorCodigoTicket
{
    public const string NombreSecuencia = "CodigoTicket";

    /// <summary>
    /// SQL constante, sin concatenación ni interpolación (RNF-02).
    /// </summary>
    public const string ConsultaSiguienteValor = "SELECT NEXT VALUE FOR dbo.CodigoTicket AS Value";

    public async Task<string> GenerarAsync(CancellationToken cancellationToken)
    {
        // Sin operadores LINQ: EF Core envolvería la consulta en una subconsulta,
        // donde SQL Server no admite NEXT VALUE FOR.
        var valores = await context.Database
            .SqlQueryRaw<int>(ConsultaSiguienteValor)
            .ToListAsync(cancellationToken);

        return FormatoCodigoTicket.Formatear(valores.Single());
    }
}
