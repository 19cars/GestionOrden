using GestionOrden.Domain.Core;

namespace GestionOrden.Domain;

public sealed class Orden : IEntidadAuditable
{
    public int Id { get; set; }
    public DateTimeOffset Fecha { get; set; }
    public string Estado { get; set; } = "Registrada";
    public decimal Total { get; set; }

    public DateTimeOffset FechaCreacion { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public DateTimeOffset? FechaModificacion { get; set; }
    public string? ModificadoPor { get; set; }

    public ICollection<DetalleOrden> Detalles { get; set; } = new List<DetalleOrden>();
}
