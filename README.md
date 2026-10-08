# Mesa Ayuda — Instrucciones de ejecución

## Requisitos previos

- **.NET SDK 10.0 o superior** (los dos proyectos usan `<TargetFramework>net10.0</TargetFramework>`, y la solución usa el formato nuevo `.slnx`, que requiere SDK 10).
- Git.

Verifica tu versión con:
```
dotnet --version
```
Si muestra 8.x o menos, instala el SDK 10 desde https://dotnet.microsoft.com/download/dotnet/10.0

## 1. Clonar el repositorio

```
git clone https://github.com/JuanRos1984/Asignacion1.git
cd Asignacion1
```

## 2. Restaurar y compilar

```
dotnet restore
dotnet build
```

## 3. Ejecutar la API

```
dotnet run --project src/MesaAyuda.Api
```

> El flag `--project` es obligatorio: la solución tiene dos proyectos (API y pruebas), y `dotnet run` sin especificar cuál falla.

La API queda escuchando en **http://localhost:5131** (perfil `http` por defecto, definido en `launchSettings.json`).

## 4. Comprobar que funciona

```
curl http://localhost:5131/salud
```
Debe responder `{"estado":"ok"}`.

También puedes usar el archivo `src/MesaAyuda.Api/MesaAyuda.Api.http` (ya trae ejemplos de todas las rutas) directamente desde Visual Studio o VS Code.

## 5. Ejecutar las pruebas (opcional)

```
dotnet test
```
35 pruebas xUnit, todas unitarias — no necesitan la API corriendo.

## Sobre los datos

Este proyecto **no usa base de datos**. La persistencia es un archivo JSON en `src/MesaAyuda.Api/data/tickets.json`, y los logs se escriben en `src/MesaAyuda.Api/logs/`. Ambas carpetas se crean automáticamente al arrancar la aplicación — no hay que crearlas a mano, y no requieren ningún comando de migración.

No hay variables de entorno obligatorias ni credenciales que configurar.