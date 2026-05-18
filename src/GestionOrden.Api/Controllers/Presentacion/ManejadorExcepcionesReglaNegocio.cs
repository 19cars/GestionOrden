using FluentValidation;
using GestionOrden.Infrastructure;

namespace GestionOrden.Api.Presentacion;

public sealed class ManejadorExcepcionesReglaNegocio
{
    private readonly RequestDelegate _siguiente;
    private readonly ILogger<ManejadorExcepcionesReglaNegocio> _logger;

    public ManejadorExcepcionesReglaNegocio(
        RequestDelegate siguiente,
        ILogger<ManejadorExcepcionesReglaNegocio> logger)
    {
        _siguiente = siguiente;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (ExcepcionReglaNegocio ex)
        {
            if (contexto.Response.HasStarted)
            {
                throw;
            }

            _logger.LogWarning(ex, "Regla de negocio: {Codigo}", ex.Codigo);
            contexto.Response.StatusCode = ex.CodigoEstado;
            await contexto.Response.WriteAsJsonAsync(new
            {
                codigo = ex.Codigo,
                mensaje = ex.Message,
                errores = ex.ErroresInternos
            });
        }
        catch (ValidationException vex)
        {
            if (contexto.Response.HasStarted)
            {
                throw;
            }

            _logger.LogWarning(vex, "Validación de entrada fallida.");
            contexto.Response.StatusCode = StatusCodes.Status400BadRequest;
            await contexto.Response.WriteAsJsonAsync(new
            {
                codigo = "VALIDACION_ENTRADA",
                mensaje = "La solicitud no cumple las validaciones.",
                errores = vex.Errors.Select(e => new { campo = e.PropertyName, mensaje = e.ErrorMessage })
            });
        }
    }
}
