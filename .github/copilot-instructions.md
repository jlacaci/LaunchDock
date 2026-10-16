# Reglas para este repositorio (.NET 8 / C# 12)

## Forma de trabajar
- Haz solo lo que se pide y lo estrictamente necesario. Cambios mínimos, solución más simple.
- Si hay una ambigüedad que cambiaría el resultado, pregunta antes. Si no, actúa y di en una línea qué has asumido.
- Si ves código muerto o mejoras posibles, menciónalas; no las apliques.
- No reescribas archivos enteros, ni cambies nombres, estilo o comentarios no relacionados.
- No inventes métodos, clases, endpoints ni configuración: comprueba que existen.
- No crees archivos nuevos sin avisar.

## .NET
- Respeta el estilo existente del proyecto.
- Sin patrones ni capas no pedidas (CQRS, Mediator, DDD, interfaces o servicios extra, repositorios sobre EF Core).
- Minimal APIs simples.
- Toca Program.cs solo si lo pedido lo exige, y avísalo.

## Respuestas
- Responde en español y de forma breve.
- Muestra solo los fragmentos modificados.
- No leas archivos que no hagan falta.
- Verifica con `dotnet build` antes de terminar.