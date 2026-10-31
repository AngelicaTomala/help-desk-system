using HelpDesk.Infrastructure.Persistencia;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HelpDesk.Tests.Integracion;

/// <summary>
/// Transacción que se revierte al terminar la prueba, para que los datos de una prueba no afecten a otra.
/// Cada contexto es independiente (sin compartir el rastreador de cambios), pero usa la misma conexión
/// y transacción, así que lee lo que guardó otro contexto.
/// </summary>
public sealed class TransaccionDePrueba : IAsyncDisposable
{
    private readonly SqlConnection _conexion;
    private readonly SqlTransaction _transaccion;

    private TransaccionDePrueba(SqlConnection conexion, SqlTransaction transaccion)
    {
        _conexion = conexion;
        _transaccion = transaccion;
    }

    public static async Task<TransaccionDePrueba> IniciarAsync(string cadenaConexion)
    {
        var conexion = new SqlConnection(cadenaConexion);
        await conexion.OpenAsync();
        var transaccion = (SqlTransaction)await conexion.BeginTransactionAsync();

        return new TransaccionDePrueba(conexion, transaccion);
    }

    public HelpDeskDbContext NuevoContexto(params IInterceptor[] interceptores)
    {
        var context = new HelpDeskDbContext(
            new DbContextOptionsBuilder<HelpDeskDbContext>().UseSqlServer(_conexion).AddInterceptors(interceptores).Options);
        context.Database.UseTransaction(_transaccion);

        return context;
    }

    public async ValueTask DisposeAsync()
    {
        await _transaccion.RollbackAsync();
        await _transaccion.DisposeAsync();
        await _conexion.DisposeAsync();
    }
}
