using MesaAyuda.Api.Persistencia;
using MesaAyuda.Api.Registro;

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

var app = builder.Build();

app.UseMiddleware<RegistroPeticionesMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/salud", () => Results.Ok(new { estado = "ok" }));

app.Run();
