namespace GestionOrden.Domain;

public sealed class DetalleOrden
{
    public int Id { get; set; }
    public int OrdenId { get; set; }
    public Orden Orden { get; set; } = null!;
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
