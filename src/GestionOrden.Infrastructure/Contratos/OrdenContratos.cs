namespace GestionOrden.Infrastructure.Contratos;

public sealed record OrdenItemSolicitud(int ProductoId, int Cantidad);

public sealed record CrearOrdenSolicitud(IReadOnlyList<OrdenItemSolicitud> Items);

public sealed record OrdenResumenRespuesta(
    int Id,
    DateTimeOffset Fecha,
    string Estado,
    decimal Total,
    string CreadoPor);

public sealed record DetalleOrdenLineaRespuesta(
    int ProductoId,
    string NombreProducto,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal);

public sealed record OrdenDetalleRespuesta(
    int Id,
    DateTimeOffset Fecha,
    string Estado,
    decimal Total,
    DateTimeOffset FechaCreacion,
    string CreadoPor,
    DateTimeOffset? FechaModificacion,
    string? ModificadoPor,
    IReadOnlyList<DetalleOrdenLineaRespuesta> Detalles);
