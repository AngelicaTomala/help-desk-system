using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Domain.Excepciones;

namespace HelpDesk.Domain;

/// <summary>
/// Horas de SLA por prioridad (RN-10).
/// </summary>
public static class PoliticaSla
{
    public static int HorasPara(Prioridad prioridad) => prioridad switch
    {
        Prioridad.Critica => 2,
        Prioridad.Alta => 4,
        Prioridad.Media => 8,
        Prioridad.Baja => 24,
        _ => throw new InvarianteVioladaException($"La prioridad {(int)prioridad} no está definida."),
    };
}
