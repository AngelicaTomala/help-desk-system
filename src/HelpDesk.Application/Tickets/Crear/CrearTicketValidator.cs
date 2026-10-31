using FluentValidation;
using HelpDesk.Domain.Entidades;
using HelpDesk.Domain.Enumeraciones;

namespace HelpDesk.Application.Tickets.Crear;

/// <summary>
/// Formato de la entrada de crear (RN-01, RN-02, RNF-05). Recibe los textos ya recortados por el handler
/// (decisiones 31 y 79) y usa las constantes de Domain (ADR-003).
/// </summary>
public sealed class CrearTicketValidator : AbstractValidator<CrearTicketComando>
{
    public CrearTicketValidator()
    {
        RuleFor(comando => comando.Titulo)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El título es obligatorio.")
            .Length(Ticket.TituloLongitudMinima, Ticket.TituloLongitudMaxima)
            .WithMessage($"El título debe tener entre {Ticket.TituloLongitudMinima} y {Ticket.TituloLongitudMaxima} caracteres.");

        RuleFor(comando => comando.Descripcion)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .Length(Ticket.DescripcionLongitudMinima, Ticket.DescripcionLongitudMaxima)
            .WithMessage($"La descripción debe tener entre {Ticket.DescripcionLongitudMinima} y {Ticket.DescripcionLongitudMaxima} caracteres.");

        RuleFor(comando => comando.Categoria)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("La categoría es obligatoria.")
            .Must(ValoresEnum.EsValido<Categoria>).WithMessage("La categoría no es válida.");

        RuleFor(comando => comando.Prioridad)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("La prioridad es obligatoria.")
            .Must(ValoresEnum.EsValido<Prioridad>).WithMessage("La prioridad no es válida.");
    }
}
