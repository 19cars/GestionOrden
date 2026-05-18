using GestionOrden.Domain;
using GestionOrden.Infrastructure;
using GestionOrden.Infrastructure.Contratos;
using GestionOrden.Persistencia;
using GestionOrden.ContratosInternos.Ordenes;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GestionOrden.Infrastructure.ServicioCalculo;

namespace GestionOrden.Application;

public sealed class OrdenesServicio
{
    private readonly ApplicationDbContext _contexto;
    private readonly IServicioInternoOrdenes _servicioInterno;
    private readonly ILogger<OrdenesServicio> _logger;

    public OrdenesServicio(
        ApplicationDbContext contexto,
        IServicioInternoOrdenes servicioInterno,
        ILogger<OrdenesServicio> logger)
    {
        _contexto = contexto;
        _servicioInterno = servicioInterno;
        _logger = logger;
    }

    public async Task<ResultadoPaginado<OrdenResumenRespuesta>> ListarAsync(
        int numeroPagina,
        int tamanoPagina,
        CancellationToken cancelacion)
    {
        var consulta = _contexto.Ordenes.AsNoTracking().OrderByDescending(o => o.Fecha);
        var total = await consulta.CountAsync(cancelacion);
        var elementos = await consulta
            .Skip((numeroPagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(o => new OrdenResumenRespuesta(o.Id, o.Fecha, o.Estado, o.Total, o.CreadoPor))
            .ToListAsync(cancelacion);

        return new ResultadoPaginado<OrdenResumenRespuesta>(elementos, total, numeroPagina, tamanoPagina);
    }

    public async Task<OrdenDetalleRespuesta?> ObtenerDetalleAsync(int id, CancellationToken cancelacion)
    {
        var orden = await _contexto.Ordenes.AsNoTracking()
            .Include(o => o.Detalles)
            .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(o => o.Id == id, cancelacion);

        if (orden is null)
        {
            return null;
        }

        var detalles = orden.Detalles
            .OrderBy(d => d.Id)
            .Select(d => new DetalleOrdenLineaRespuesta(
                d.ProductoId,
                d.Producto.Nombre,
                d.Cantidad,
                d.PrecioUnitario,
                d.Subtotal))
            .ToList();

        return new OrdenDetalleRespuesta(
            orden.Id,
            orden.Fecha,
            orden.Estado,
            orden.Total,
            orden.FechaCreacion,
            orden.CreadoPor,
            orden.FechaModificacion,
            orden.ModificadoPor,
            detalles);
    }

    public async Task<OrdenDetalleRespuesta> CrearAsync(CrearOrdenSolicitud solicitud, string usuario, CancellationToken cancelacion)
    {
        var itemsInternos = solicitud.Items
            .Select(i => new ItemOrdenInterno(i.ProductoId, i.Cantidad))
            .ToList();

        var respuestaInterna = await _servicioInterno.ValidarYCalcularAsync(
            new SolicitudValidacionOrdenInterna(itemsInternos),
            cancelacion);

        if (respuestaInterna is null)
        {
            throw new ExcepcionReglaNegocio(
                StatusCodes.Status502BadGateway,
                "SERVICIO_INTERNO",
                "El servicio interno de validación de órdenes no está disponible.");
        }

        if (!respuestaInterna.Exito || respuestaInterna.Lineas is null || respuestaInterna.Lineas.Count == 0)
        {
            throw new ExcepcionReglaNegocio(
                StatusCodes.Status400BadRequest,
                "VALIDACION_ORDEN",
                "La orden no cumple las reglas de negocio.",
                respuestaInterna.Errores);
        }

        await using var transaccion = await _contexto.Database.BeginTransactionAsync(cancelacion);
        Orden orden = null!;
        try
        {
            var ahora = DateTimeOffset.UtcNow;
            orden = new Orden
            {
                Fecha = ahora,
                Estado = "Registrada",
                Total = respuestaInterna.Total
            };
            orden.AplicarCreacion(usuario);

            foreach (var linea in respuestaInterna.Lineas)
            {
                var producto = await _contexto.Productos.FirstOrDefaultAsync(
                    p => p.Id == linea.ProductoId,
                    cancelacion);

                if (producto is null)
                {
                    throw new ExcepcionReglaNegocio(
                        StatusCodes.Status400BadRequest,
                        "PRODUCTO_NO_EXISTE",
                        $"El producto {linea.ProductoId} no existe.");
                }

                if (producto.Stock < linea.Cantidad)
                {
                    throw new ExcepcionReglaNegocio(
                        StatusCodes.Status409Conflict,
                        "STOCK_INSUFICIENTE",
                        $"Stock insuficiente para el producto '{producto.Nombre}'.");
                }

                producto.Stock -= linea.Cantidad;
                producto.AplicarModificacion(usuario);

                orden.Detalles.Add(new DetalleOrden
                {
                    ProductoId = producto.Id,
                    Cantidad = linea.Cantidad,
                    PrecioUnitario = linea.PrecioUnitario,
                    Subtotal = linea.Subtotal
                });
            }

            _contexto.Ordenes.Add(orden);
            await _contexto.SaveChangesAsync(cancelacion);
            await transaccion.CommitAsync(cancelacion);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaccion.RollbackAsync(cancelacion);
            _logger.LogWarning(ex, "Conflicto de concurrencia al persistir la orden.");
            throw new ExcepcionReglaNegocio(
                StatusCodes.Status409Conflict,
                "CONCURRENCIA",
                "Los datos del producto cambiaron mientras se procesaba la orden. Intente nuevamente.");
        }
        catch
        {
            await transaccion.RollbackAsync(cancelacion);
            throw;
        }

        return await ObtenerDetalleAsync(orden.Id, cancelacion)
               ?? throw new InvalidOperationException("No se pudo cargar la orden creada.");
    }
}
