using GestionOrden.Api.Extensions;
using GestionOrden.Application;
using GestionOrden.Database;
using GestionOrden.Domain;
using GestionOrden.Infrastructure.ServicioCalculo;
using GestionOrden.Persistencia;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using GestionOrden.Api.Validadores;

// Cargar variables de entorno desde .env antes de construir el WebApplication
DotEnvExtensions.LoadDotEnv();

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(opciones =>
{
    opciones.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    opciones.SingleLine = true;
});

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter("Microsoft", LogLevel.Warning)
        .AddFilter("System", LogLevel.Warning)
        .AddFilter("Program", LogLevel.Debug) // Tu categoría
        .AddConsole(); // Enviar logs a la consola
});
// Ejecutar migraciones de base de datos (Evolve) al iniciar
// Esto usará la connection string "Default" del configuration
try
{
    //GestionOrden.Database.EvolveRunner.RunEvolve(builder.Configuration);
    builder.Services.AddTransient<EvolveRunner>();
}
catch
{
    // si la migración falla, dejamos que el host controle el fallo o se puede escoger continuar
    throw;
}

// Add services to the container.
builder.Services.AddSwaggerDocumentation();

builder.Services.AddScoped<CategoriasServicio>();
builder.Services.AddScoped<ProductosServicio>();
builder.Services.AddScoped<OrdenesServicio>();

builder.Services.AddHttpClient<IServicioInternoOrdenes, ServicioInternoOrdenesHttp>((ambito, cliente) =>
{
    var opciones = ambito.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpcionesServicioInternoOrdenes>>().Value;
    cliente.BaseAddress = new Uri(opciones.UrlBase.TrimEnd('/') + "/");
    cliente.Timeout = TimeSpan.FromSeconds(opciones.SegundosEspera);
    if (!cliente.DefaultRequestHeaders.Contains(opciones.NombreCabeceraClave))
    {
        cliente.DefaultRequestHeaders.Add(opciones.NombreCabeceraClave, opciones.ValorClave);
    }
});

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CrearCategoriaSolicitudValidador>();

// JWT configuration - read secret and optional issuer/audience from environment
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException("JWT_SECRET env var is required for JWT authentication.");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "GestionOrden";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "GestionOrdenAudience";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

// Database registration for Identity - reuse the same connection string used by migrations
var dbConnection = builder.Configuration[CampaignCatalog.Database.MigrationConstantes.ConnectionStringKey]
                ?? throw new InvalidOperationException("DB connection is required via environment variable 'DB-CONNECTION'.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(dbConnection));

builder.Services.AddIdentity<UsuarioAplicacion, IdentityRole>(options =>
{
    // reasonable defaults for demo; tune for production
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy("FrontAngular", politica =>
    {
        politica.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwaggerDocumentation();

using (var scope = app.Services.CreateScope())
{
    var databaseIntegrator = scope.ServiceProvider.GetRequiredService<EvolveRunner>();
    databaseIntegrator.RunEvolve();
}

// Leer prefijo de ruta para Swagger desde variable de entorno (opcional)
var swaggerRoutePrefix = Environment.GetEnvironmentVariable("SWAGGER_ROUTE_PREFIX");

//// 2. Crear el logger específico para esta clase
//ILogger logger = loggerFactory.CreateLogger<Program>();

//// 3. ¡Escribir logs!
//logger.LogInformation("La aplicación de consola ha iniciado.");
//logger.LogWarning("Esto es una advertencia.");
//logger.LogError("Esto es un mensaje de error.");

app.UseCors("FrontAngular");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Mapear controllers para que los endpoints de los controllers sean accesibles
app.MapControllers();

app.Run();
