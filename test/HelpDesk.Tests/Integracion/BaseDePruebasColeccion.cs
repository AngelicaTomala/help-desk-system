namespace HelpDesk.Tests.Integracion;

/// <summary>
/// Agrupa las pruebas que usan la base de pruebas: comparten la fixture y no se ejecutan en paralelo
/// entre sí ni con el resto de las pruebas.
/// </summary>
[CollectionDefinition(Nombre, DisableParallelization = true)]
public class BaseDePruebasColeccion : ICollectionFixture<BaseDePruebasFixture>
{
    public const string Nombre = "BaseDePruebas";
}
