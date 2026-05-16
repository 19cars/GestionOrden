using GestionOrden.Domain.Core;

namespace GestionOrden.Domain;

public sealed class Categoria : IEntidadAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public DateTimeOffset FechaCreacion { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public DateTimeOffset? FechaModificacion { get; set; }
    public string? ModificadoPor { get; set; }

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
