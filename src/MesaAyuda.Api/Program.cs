using MesaAyuda.Api.Persistencia;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// La ruta del archivo se puede cambiar con la variable de entorno Almacenamiento__RutaDatos.
var almacenamiento = builder.Configuration.GetSection(OpcionesAlmacenamiento.Seccion).Get<OpcionesAlmacenamiento>()
    ?? new OpcionesAlmacenamiento();
var rutaDatos = Path.Combine(builder.Environment.ContentRootPath, almacenamiento.RutaDatos);
builder.Services.AddSingleton<IRepositorioTickets>(new RepositorioTicketsJson(rutaDatos));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/salud", () => Results.Ok(new { estado = "ok" }));

app.Run();
