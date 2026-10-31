namespace HelpDesk.Api.Documentacion;

/// <summary>
/// Marca un campo de texto del cuerpo que representa un enum: el documento OpenAPI lista sus valores (decisión 96,
/// RNF-09). Solo es documentación; la lectura y la validación no cambian.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ValoresEnumAttribute(Type tipoEnum) : Attribute
{
    public Type TipoEnum { get; } = tipoEnum;
}
