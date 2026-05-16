namespace GestionOrden.Domain.Core;

public interface IEntidadAuditable
{
    DateTimeOffset FechaCreacion { get; set; }
    string CreadoPor { get; set; }
    DateTimeOffset? FechaModificacion { get; set; }
    string? ModificadoPor { get; set; }
}
