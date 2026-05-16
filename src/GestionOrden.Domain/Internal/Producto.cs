using GestionOrden.Domain.Core;
using System;

namespace GestionOrden.Domain;

public sealed class Producto : IEntidadAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;
    public bool Activo { get; set; } = true;

    public byte[] FilaVersion { get; set; } = Array.Empty<byte>();

    public DateTimeOffset FechaCreacion { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public DateTimeOffset? FechaModificacion { get; set; }
    public string? ModificadoPor { get; set; }
}
