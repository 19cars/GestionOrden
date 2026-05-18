using FluentAssertions;
using GestionOrden.Application;
using GestionOrden.ContratosInternos.Ordenes;
using GestionOrden.Domain;
using GestionOrden.Infrastructure;
using GestionOrden.Infrastructure.Contratos;
using GestionOrden.Infrastructure.ServicioCalculo;
using GestionOrden.Persistencia;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class GestionOrdenesTests : IDisposable
{
    private readonly ApplicationDbContext _contexto;
    private readonly Mock<IServicioInternoOrdenes> _servicioInternoMock;
    private readonly Mock<ILogger<OrdenesServicio>> _loggerMock;
    private readonly OrdenesServicio _servicio;

    public GestionOrdenesTests()
    {
        // Configurar EF Core In-Memory
        var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Base de datos única por prueba
            .Options;

        _contexto = new ApplicationDbContext(opciones);
        _servicioInternoMock = new Mock<IServicioInternoOrdenes>();
        _loggerMock = new Mock<ILogger<OrdenesServicio>>();

        // Instanciar el servicio bajo prueba
        _servicio = new OrdenesServicio(_contexto, _servicioInternoMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _contexto.Database.EnsureDeleted();
        _contexto.Dispose();
    }

    [Fact]
    public async Task CrearAsync_CuandoTodoEsValido_DebeCrearOrdenYReducirStock()
    {
        // Arrange
        var usuario = "admin_user";
        var productoId = 1;

        // 1. Sembrar datos en la base de datos en memoria
        var producto = new Producto { Id = productoId, Nombre = "Caramelo", Stock = 10 };
        _contexto.Productos.Add(producto);
        await _contexto.SaveChangesAsync();

        var solicitud = new CrearOrdenSolicitud(new List<OrdenItemSolicitud> { new OrdenItemSolicitud(productoId, 2)});

        // 2. Configurar el Mock del servicio interno
        var respuestaInterna = new RespuestaValidacionOrdenInterna(true,
                                                                    new List<LineaOrdenCalculada>
                                                                    {
                                                                        new LineaOrdenCalculada(productoId,"", 1000, 2, 2000)
                                                                    },
                                                                    2000,
                                                                    null);
        
        _servicioInternoMock
            .Setup(s => s.ValidarYCalcularAsync(It.IsAny<SolicitudValidacionOrdenInterna>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(respuestaInterna);

        // Act
        var resultado = await _servicio.CrearAsync(solicitud, usuario, CancellationToken.None);

        // Assert (FluentAssertions)
        resultado.Should().NotBeNull();
        resultado.Total.Should().Be(2000);

        // Verificar que el stock en la BD disminuyó de 10 a 8
        var productoEnBd = await _contexto.Productos.FindAsync(productoId);
        productoEnBd!.Stock.Should().Be(8);
    }
}