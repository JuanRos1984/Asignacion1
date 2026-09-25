using System.Text.Json.Serialization;
using MesaAyuda.Api.Endpoints;
using MesaAyuda.Api.Errores;
using MesaAyuda.Api.Persistencia;
using MesaAyuda.Api.Registro;
using MesaAyuda.Api.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton(TimeProvider.System);

// Además de la consola, el log se escribe en archivos .txt diarios.
var registro = builder.Configuration.GetSection(OpcionesRegistroArchivo.Seccion).Get<OpcionesRegistroArchivo>()
    ?? new OpcionesRegistroArchivo();
var carpetaLogs = Path.Combine(builder.Environment.ContentRootPath, registro.Carpeta);
builder.Logging.AddProvider(new RegistroArchivoProvider(carpetaLogs, registro.NivelMinimo, TimeProvider.System));

// La ruta del archivo se puede cambiar con la variable de entorno Almacenamiento__RutaDatos.
var almacenamiento = builder.Configuration.GetSection(OpcionesAlmacenamiento.Seccion).Get<OpcionesAlmacenamiento>()
    ?? new OpcionesAlmacenamiento();
var rutaDatos = Path.Combine(builder.Environment.ContentRootPath, almacenamiento.RutaDatos);
builder.Services.AddSingleton<IRepositorioTickets>(new RepositorioTicketsJson(rutaDatos));

builder.Services.AddSingleton<ServicioTickets>();
builder.Services.AddSingleton<ServicioEstadisticas>();

builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorErrores>();

var app = builder.Build();

// El registro de peticiones va primero para ver el código final, incluso cuando hubo un error.
app.UseMiddleware<RegistroPeticionesMiddleware>();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/salud", () => Results.Ok(new { estado = "ok" }));
app.MapTickets();
app.MapEstadisticas();

app.Run();

// Permite a las pruebas de integración levantar la API en memoria.
public partial class Program;
