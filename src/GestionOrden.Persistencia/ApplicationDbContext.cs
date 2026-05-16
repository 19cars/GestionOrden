using GestionOrden.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestionOrden.Persistencia;

public class ApplicationDbContext : IdentityDbContext<UsuarioAplicacion>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Orden> Ordenes => Set<Orden>();
    public DbSet<DetalleOrden> DetallesOrden => Set<DetalleOrden>();
    protected override void OnModelCreating(ModelBuilder modelador)
    {
        base.OnModelCreating(modelador);

        modelador.Entity<Categoria>(entidad =>
        {
            entidad.ToTable("Categorias");
            entidad.Property(c => c.Nombre).HasMaxLength(200).IsRequired();
            entidad.Property(c => c.CreadoPor).HasMaxLength(450).IsRequired();
            entidad.Property(c => c.ModificadoPor).HasMaxLength(450);
        });

        modelador.Entity<Producto>(entidad =>
        {
            entidad.ToTable("Productos");
            entidad.Property(p => p.Nombre).HasMaxLength(200).IsRequired();
            entidad.Property(p => p.Descripcion).HasMaxLength(2000);
            entidad.Property(p => p.Precio).HasPrecision(18, 2);
            entidad.Property(p => p.CreadoPor).HasMaxLength(450).IsRequired();
            entidad.Property(p => p.ModificadoPor).HasMaxLength(450);
            entidad.Property(p => p.FilaVersion).IsRowVersion();
            entidad.HasIndex(p => p.CategoriaId);
            entidad.HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelador.Entity<Orden>(entidad =>
        {
            entidad.ToTable("Ordenes");
            entidad.Property(o => o.Estado).HasMaxLength(50).IsRequired();
            entidad.Property(o => o.Total).HasPrecision(18, 2);
            entidad.Property(o => o.CreadoPor).HasMaxLength(450).IsRequired();
            entidad.Property(o => o.ModificadoPor).HasMaxLength(450);
        });

        modelador.Entity<DetalleOrden>(entidad =>
        {
            entidad.ToTable("DetallesOrden");
            entidad.Property(d => d.PrecioUnitario).HasPrecision(18, 2);
            entidad.Property(d => d.Subtotal).HasPrecision(18, 2);
            entidad.HasOne(d => d.Orden)
                .WithMany(o => o.Detalles)
                .HasForeignKey(d => d.OrdenId)
                .OnDelete(DeleteBehavior.Cascade);
            entidad.HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
