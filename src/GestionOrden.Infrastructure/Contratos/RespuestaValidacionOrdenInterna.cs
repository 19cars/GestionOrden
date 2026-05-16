namespace CalculoOrden.Infrastructure.Contratos;

public sealed record RespuestaValidacionOrdenInterna(
    bool Exito,
    IReadOnlyList<LineaOrdenCalculada>? Lineas,
    decimal Total,
    IReadOnlyList<ErrorNegocioInterno>? Errores);
