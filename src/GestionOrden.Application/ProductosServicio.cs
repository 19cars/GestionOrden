using GestionOrden.Domain;
using GestionOrden.Infrastructure;
using GestionOrden.Infrastructure.Contratos;
using GestionOrden.Persistencia;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestionOrden.Application;

public sealed class ProductosServicio
{
    private readonly ApplicationDbContext _contexto;
    private readonly ILogger<ProductosServicio> _logger;

    public ProductosServicio(ApplicationDbContext contexto,
                            ILogger<ProductosServicio> logger)
    {
        _contexto = contexto;
        _logger = logger;
    }

    public async Task<ResultadoPaginado<ProductoRespuesta>> ListarAsync(
        int numeroPagina,
        int tamanoPagina,
        int? categoriaId,
        string? texto,
        bool? soloActivos,
        CancellationToken cancelacion)
    {
        _logger.LogInformation("Inicio de ProductosServicio:ListarAsync");
        var consulta = _contexto.Productos.AsNoTracking().Include(p => p.Categoria).AsQueryable();

        if (categoriaId is not null)
        {
            consulta = consulta.Where(p => p.CategoriaId == categoriaId);
        }

        if (soloActivos is true)
        {
            consulta = consulta.Where(p => p.Activo);
        }

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            consulta = consulta.Where(p =>
                EF.Functions.Like(p.Nombre, $"%{t}%") ||
                (p.Descripcion != null && EF.Functions.Like(p.Descripcion, $"%{t}%")));
        }

        consulta = consulta.OrderBy(p => p.Nombre);
        var total = await consulta.CountAsync(cancelacion);
        var elementos = await consulta
            .Skip((numeroPagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(p => new ProductoRespuesta(
                p.Id,
                p.Nombre,
                p.Descripcion,
                p.Precio,
                p.Stock,
                p.CategoriaId,
                p.Categoria.Nombre,
                p.Activo,
                p.FechaCreacion,
                p.CreadoPor,
                p.FechaModificacion,
                p.ModificadoPor))
            .ToListAsync(cancelacion);

        return new ResultadoPaginado<ProductoRespuesta>(elementos, total, numeroPagina, tamanoPagina);
    }

    public async Task<ProductoRespuesta?> ObtenerAsync(int id, CancellationToken cancelacion)
    {
        _logger.LogInformation("Inicio de ProductosServicio:ObtenerAsync");
        return await _contexto.Productos.AsNoTracking()
            .Include(p => p.Categoria)
            .Where(p => p.Id == id)
            .Select(p => new ProductoRespuesta(
                p.Id,
                p.Nombre,
                p.Descripcion,
                p.Precio,
                p.Stock,
                p.CategoriaId,
                p.Categoria.Nombre,
                p.Activo,
                p.FechaCreacion,
                p.CreadoPor,
                p.FechaModificacion,
                p.ModificadoPor))
            .FirstOrDefaultAsync(cancelacion);
    }

    public async Task<ProductoRespuesta> CrearAsync(CrearProductoSolicitud solicitud, string usuario, CancellationToken cancelacion)
    {
        _logger.LogInformation("Inicio de ProductosServicio:CrearAsync");
        var categoriaExiste = await _contexto.Categorias.AnyAsync(c => c.Id == solicitud.CategoriaId && c.Activo, cancelacion);
        if (!categoriaExiste)
        {
            throw new ExcepcionReglaNegocio(
                StatusCodes.Status400BadRequest,
                "CATEGORIA_INVALIDA",
                "La categoría no existe o está desactivada.");
        }

        var entidad = new Producto
        {
            Nombre = solicitud.Nombre.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(solicitud.Descripcion) ? null : solicitud.Descripcion.Trim(),
            Precio = solicitud.Precio,
            Stock = solicitud.Stock,
            CategoriaId = solicitud.CategoriaId,
            Activo = true
        };

        entidad.AplicarCreacion(usuario);
        _contexto.Productos.Add(entidad);
        await _contexto.SaveChangesAsync(cancelacion);

        return await ObtenerAsync(entidad.Id, cancelacion)
               ?? throw new InvalidOperationException("No se pudo cargar el producto creado.");
    }

    public async Task<ProductoRespuesta> ActualizarAsync(
        int id,
        ActualizarProductoSolicitud solicitud,
        string usuario,
        CancellationToken cancelacion)
    {
        _logger.LogInformation("Inicio de ProductosServicio:ActualizarAsync");
        var entidad = await _contexto.Productos.FirstOrDefaultAsync(p => p.Id == id, cancelacion);
        if (entidad is null)
        {
            throw new ExcepcionReglaNegocio(StatusCodes.Status404NotFound, "NO_ENCONTRADO", "El producto no existe.");
        }

        var categoriaExiste = await _contexto.Categorias.AnyAsync(c => c.Id == solicitud.CategoriaId && c.Activo, cancelacion);
        if (!categoriaExiste)
        {
            throw new ExcepcionReglaNegocio(
                StatusCodes.Status400BadRequest,
                "CATEGORIA_INVALIDA",
                "La categoría no existe o está desactivada.");
        }

        entidad.Nombre = solicitud.Nombre.Trim();
        entidad.Descripcion = string.IsNullOrWhiteSpace(solicitud.Descripcion) ? null : solicitud.Descripcion.Trim();
        entidad.Precio = solicitud.Precio;
        entidad.Stock = solicitud.Stock;
        entidad.CategoriaId = solicitud.CategoriaId;
        entidad.Activo = solicitud.Activo;
        entidad.AplicarModificacion(usuario);

        await _contexto.SaveChangesAsync(cancelacion);

        return await ObtenerAsync(entidad.Id, cancelacion)
               ?? throw new InvalidOperationException("No se pudo cargar el producto actualizado.");
    }

    public async Task DesactivarAsync(int id, string usuario, CancellationToken cancelacion)
    {
        _logger.LogInformation("Inicio de ProductosServicio:DesactivarAsync");
        var entidad = await _contexto.Productos.FirstOrDefaultAsync(p => p.Id == id, cancelacion);
        if (entidad is null)
        {
            throw new ExcepcionReglaNegocio(StatusCodes.Status404NotFound, "NO_ENCONTRADO", "El producto no existe.");
        }

        entidad.Activo = false;
        entidad.AplicarModificacion(usuario);
        await _contexto.SaveChangesAsync(cancelacion);
    }
}
