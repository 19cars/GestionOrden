namespace GestionOrden.Infrastructure.Contratos;

public sealed record ResultadoPaginado<T>(
    IReadOnlyList<T> Elementos,
    int TotalElementos,
    int NumeroPagina,
    int TamanoPagina);
