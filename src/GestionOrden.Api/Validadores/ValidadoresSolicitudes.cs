using FluentValidation;
using GestionOrden.Infrastructure.Contratos;

namespace GestionOrden.Api.Validadores;

public sealed class CrearCategoriaSolicitudValidador : AbstractValidator<CrearCategoriaSolicitud>
{
    public CrearCategoriaSolicitudValidador()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
    }
}

public sealed class ActualizarCategoriaSolicitudValidador : AbstractValidator<ActualizarCategoriaSolicitud>
{
    public ActualizarCategoriaSolicitudValidador()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
    }
}

public sealed class CrearProductoSolicitudValidador : AbstractValidator<CrearProductoSolicitud>
{
    public CrearProductoSolicitudValidador()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Descripcion).MaximumLength(2000);
        RuleFor(x => x.Precio).GreaterThan(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoriaId).GreaterThan(0);
    }
}

public sealed class ActualizarProductoSolicitudValidador : AbstractValidator<ActualizarProductoSolicitud>
{
    public ActualizarProductoSolicitudValidador()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Descripcion).MaximumLength(2000);
        RuleFor(x => x.Precio).GreaterThan(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoriaId).GreaterThan(0);
    }
}

public sealed class CrearOrdenSolicitudValidador : AbstractValidator<CrearOrdenSolicitud>
{
    public CrearOrdenSolicitudValidador()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductoId).GreaterThan(0);
            item.RuleFor(i => i.Cantidad).GreaterThan(0);
        });
    }
}

public sealed class IniciarSesionSolicitudValidador : AbstractValidator<IniciarSesionSolicitud>
{
    public IniciarSesionSolicitudValidador()
    {
        RuleFor(x => x.Usuario).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Contrasena).NotEmpty();
    }
}
