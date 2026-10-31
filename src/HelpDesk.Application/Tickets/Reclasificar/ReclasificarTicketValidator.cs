using FluentValidation;
using HelpDesk.Domain.Enumeraciones;

namespace HelpDesk.Application.Tickets.Reclasificar;

/// <summary>
/// Formato de la entrada de reclasificar (RN-02, RN-13, CA-01.9). La categoría se convierte con
/// <see cref="ValoresEnum"/> (decisiones 58 y 96).
/// </summary>
public sealed class ReclasificarTicketValidator : AbstractValidator<ReclasificarTicketComando>
{
    public ReclasificarTicketValidator()
    {
        RuleFor(comando => comando.Categoria)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("La categoría es obligatoria.")
            .Must(ValoresEnum.EsValido<Categoria>).WithMessage("La categoría no es válida.");
    }
}
