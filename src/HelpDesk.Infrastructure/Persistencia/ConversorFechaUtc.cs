using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HelpDesk.Infrastructure.Persistencia;

/// <summary>
/// Guarda la fecha sin cambios (ya llega en UTC, RNF-08) y la lee con <see cref="DateTimeKind.Utc"/>,
/// porque <c>datetime2</c> se lee como <see cref="DateTimeKind.Unspecified"/> (contrato §1.6).
/// </summary>
public class ConversorFechaUtc() : ValueConverter<DateTime, DateTime>(
    fecha => fecha,
    fecha => DateTime.SpecifyKind(fecha, DateTimeKind.Utc));
