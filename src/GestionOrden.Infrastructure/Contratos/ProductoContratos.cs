namespace GestionOrden.Infrastructure.Contratos;

public sealed record ProductoRespuesta(
    int Id,
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int Stock,
    int CategoriaId,
    string NombreCategoria,
    bool Activo,
    DateTimeOffset FechaCreacion,
    string CreadoPor,
    DateTimeOffset? FechaModificacion,
    string? ModificadoPor);

public sealed record CrearProductoSolicitud(
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int Stock,
    int CategoriaId);

public sealed record ActualizarProductoSolicitud(
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int Stock,
    int CategoriaId,
    bool Activo);
