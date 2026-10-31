namespace HelpDesk.Application.Abstracciones;

/// <summary>
/// Confirma los cambios de la operación. Se llama después de toda validación, así que una operación rechazada
/// no modifica datos (decisión 97, RNF-04).
/// </summary>
public interface IUnidadDeTrabajo
{
    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
