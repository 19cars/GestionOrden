
using GestionOrden.Domain;
using GestionOrden.Infrastructure;
using GestionOrden.Infrastructure.Contratos;
using GestionOrden.Persistencia;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GestionOrden.Application;

public sealed class CategoriasServicio
{
    private readonly ApplicationDbContext _contexto;

    public CategoriasServicio(ApplicationDbContext contexto) => _contexto = contexto;

    public async Task<ResultadoPaginado<CategoriaRespuesta>> ListarAsync(
        int numeroPagina,
        int tamanoPagina,
        CancellationToken cancelacion)
    {
        var consulta = _contexto.Categorias.AsNoTracking().OrderBy(c => c.Nombre);
        var total = await consulta.CountAsync(cancelacion);
        var elementos = await consulta
            .Skip((numeroPagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(c => new CategoriaRespuesta(
                c.Id,
                c.Nombre,
                c.Activo,
                c.FechaCreacion,
                c.CreadoPor,
                c.FechaModificacion,
                c.ModificadoPor))
            .ToListAsync(cancelacion);

        return new ResultadoPaginado<CategoriaRespuesta>(elementos, total, numeroPagina, tamanoPagina);
    }

    public async Task<CategoriaRespuesta?> ObtenerAsync(int id, CancellationToken cancelacion)
    {
        return await _contexto.Categorias.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoriaRespuesta(
                c.Id,
                c.Nombre,
                c.Activo,
                c.FechaCreacion,
                c.CreadoPor,
                c.FechaModificacion,
                c.ModificadoPor))
            .FirstOrDefaultAsync(cancelacion);
    }

    public async Task<CategoriaRespuesta> CrearAsync(CrearCategoriaSolicitud solicitud, string usuario, CancellationToken cancelacion)
    {
        var entidad = new Categoria { Nombre = solicitud.Nombre.Trim(), Activo = true };
        entidad.AplicarCreacion(usuario);
        _contexto.Categorias.Add(entidad);
        await _contexto.SaveChangesAsync(cancelacion);
        return new CategoriaRespuesta(
            entidad.Id,
            entidad.Nombre,
            entidad.Activo,
            entidad.FechaCreacion,
            entidad.CreadoPor,
            entidad.FechaModificacion,
            entidad.ModificadoPor);
    }

    public async Task<CategoriaRespuesta> ActualizarAsync(
        int id,
        ActualizarCategoriaSolicitud solicitud,
        string usuario,
        CancellationToken cancelacion)
    {
        var entidad = await _contexto.Categorias.FirstOrDefaultAsync(c => c.Id == id, cancelacion);
        if (entidad is null)
        {
            throw new ExcepcionReglaNegocio(StatusCodes.Status404NotFound, "NO_ENCONTRADO", "La categoría no existe.");
        }

        entidad.Nombre = solicitud.Nombre.Trim();
        entidad.Activo = solicitud.Activo;
        entidad.AplicarModificacion(usuario);
        await _contexto.SaveChangesAsync(cancelacion);

        return new CategoriaRespuesta(
            entidad.Id,
            entidad.Nombre,
            entidad.Activo,
            entidad.FechaCreacion,
            entidad.CreadoPor,
            entidad.FechaModificacion,
            entidad.ModificadoPor);
    }

    public async Task DesactivarAsync(int id, string usuario, CancellationToken cancelacion)
    {
        var entidad = await _contexto.Categorias.FirstOrDefaultAsync(c => c.Id == id, cancelacion);
        if (entidad is null)
        {
            throw new ExcepcionReglaNegocio(StatusCodes.Status404NotFound, "NO_ENCONTRADO", "La categoría no existe.");
        }

        var tieneActivos = await _contexto.Productos.AnyAsync(
            p => p.CategoriaId == id && p.Activo,
            cancelacion);

        if (tieneActivos)
        {
            throw new ExcepcionReglaNegocio(
                StatusCodes.Status409Conflict,
                "CATEGORIA_CON_PRODUCTOS_ACTIVOS",
                "No se puede desactivar la categoría porque tiene productos activos asociados.");
        }

        entidad.Activo = false;
        entidad.AplicarModificacion(usuario);
        await _contexto.SaveChangesAsync(cancelacion);
    }
}
