# Bitácora de sesión con el agente — Asignación 1

- **Agente:** Claude Code (modelo Claude Opus 5.5), en la aplicación de escritorio.
- **Fecha de la sesión:** 25 de septiembre de 2026.
- **Repositorio:** https://github.com/JuanRos1984/Asignacion1

## Qué le pedí

1. Leer el enunciado de la Asignación 1 y proponer un plan antes de escribir código.
2. Con el plan aprobado: construir una Web API de mesa de ayuda con persistencia en JSON y log en archivos `.txt`, en ramas por funcionalidad y con commits atómicos.
3. Escribir la descripción de cada pull request con las cuatro secciones: Qué cambia, Por qué, Cómo probarlo, Qué NO incluye.
4. Preparar los tres aportes al repositorio de mi pareja: `.gitignore`, README de ejecución y plantilla de pull request.

## Qué me devolvió

- Un plan con una decisión que no le pedí pero que estaba bien: dejar sin cubrir en el `.gitignore` las carpetas `data/` y `logs/`, y no escribir el README ni la plantilla, para que el aporte de mi pareja tuviera contenido real.
- 26 commits repartidos en seis ramas, cada una con su pull request (#1 a #6), y 35 pruebas xUnit en verde.
- Las descripciones de los pull requests, con la dependencia entre ellos anotada al inicio, porque las ramas quedaron encadenadas.

## Errores del agente, cómo los detecté y cómo los corregí

### Error 1 — Un commit que no compilaba

**Qué hizo.** Hizo tres commits iniciales en este orden: `.gitignore`, luego la Web API **junto con la solución `MesaAyuda.slnx`**, y luego el proyecto de pruebas. Pero la solución ya incluía el proyecto de pruebas, que en ese segundo commit todavía no existía. Quien hiciera `git checkout` de ese punto tendría una solución rota.

**Cómo lo detecté.** Al revisar `git ls-files` y el log antes del primer `push`: el archivo `.slnx` aparecía en el mismo commit que la API, y la carpeta `tests/` en el siguiente.

**Cómo lo corregí.** Como todavía no había hecho `push`, deshice los dos últimos commits con `git reset --soft HEAD~2` y los rehíce en otro orden: primero la API, después las pruebas y al final la solución que agrupa ambos proyectos (`aa91dd4`, `19c4508`, `e75aef8`).

### Error 2 — Un mensaje de error que contaba otra causa

**Qué hizo.** En `ManejadorErrores`, cualquier `BadHttpRequestException` respondía «Revisa que el JSON esté bien formado». Pero esa excepción también sale cuando lo que viene mal es un parámetro de la consulta, que no tiene nada que ver con el JSON.

**Cómo lo detecté.** Probando con `curl` el endpoint de estadísticas: `GET /api/estadisticas?desde=ayer` devolvió 400 con el mensaje del JSON, aunque esa petición no tiene cuerpo.

**Cómo lo corregí.** Le pedí cambiar el texto para que mencione tanto el cuerpo como los parámetros. Quedó en su propio commit, `88efc7f` «Aclara el mensaje de petición mal formada».

### Error 3 — Un comando que no llegó a ejecutarse

**Qué hizo.** Intentó crear siete archivos de C# en un solo comando de terminal con varios bloques `heredoc`. El shell rechazó el comando completo («unexpected EOF while looking for matching `'`») y no se creó ningún archivo.

**Cómo lo detecté.** Por el error del shell, y porque `git status` no mostraba archivos nuevos.

**Cómo lo corregí.** Creó los archivos uno por uno con la herramienta de escritura de archivos en lugar de la terminal, y verificó con `dotnet build` antes de hacer los commits.

## Lo que aprendí

El agente es rápido generando código que compila, pero el orden de los commits y los mensajes de error necesitan la misma revisión que el código: los dos primeros errores habrían pasado cualquier `dotnet build`.
