# CLAUDE.md — Reglas de comportamiento para Claude en este repositorio

Estas reglas definen cómo debe actuar Claude al generar o modificar código en este proyecto.  
El objetivo es mantener el código simple, estable y con cambios mínimos.

---

## 1. Pensar antes de programar
Antes de escribir código, Claude debe:
- Explicar qué ha entendido de la petición.
- Identificar cualquier ambigüedad.
- Preguntar si algo no está claro.
- Proponer varias interpretaciones si existen dudas.
- Presentar un plan breve antes de implementarlo.

Claude no debe asumir nada sin confirmación explícita.

---

## 2. Simplicidad primero
Claude debe elegir siempre la solución más simple que cumpla el objetivo.

Prohibido:
- Sobreingeniería.
- Abstracciones innecesarias.
- Refactorizaciones no solicitadas.
- Añadir patrones, capas o configuraciones no pedidas.
- Cambiar estilos, comentarios o estructura si no se pidió.

Regla de oro:  
**Si puede hacerse en 5 líneas, no lo hagas en 20.**

---

## 3. Cambios quirúrgicos
Claude solo puede modificar:
- Lo que el usuario pidió explícitamente.
- Lo estrictamente necesario para que funcione.

Prohibido:
- “Mejorar” código cercano.
- Reescribir archivos enteros.
- Cambiar nombres, estilos o comentarios no relacionados.
- Eliminar código existente salvo petición explícita.

Si detecta código muerto o mejoras posibles, debe mencionarlo, no aplicarlo.

---

## 4. Ejecución guiada por objetivos
Claude debe:
- Convertir la petición en criterios verificables.
- Proponer un plan paso a paso.
- Ejecutar solo ese plan.
- Revisar al final que:
  - Cumple el objetivo.
  - No tocó nada más.
  - No introdujo complejidad innecesaria.

---

## 5. Reglas específicas para .NET 8 y C# 12
- Mantener el estilo existente del proyecto.
- No introducir patrones complejos (CQRS, Mediator, DDD, etc.) salvo petición explícita.
- No crear interfaces, servicios o capas adicionales sin autorización.
- Mantener Minimal APIs minimalistas.
- En EF Core:
  - No crear repositorios innecesarios.
  - No abstraer DbContext salvo petición explícita.
- No modificar Program.cs salvo petición directa.

---

## 6. Seguridad contra inventos
Claude no debe:
- Inventar métodos, clases, endpoints o configuraciones.
- Suponer estructuras que no existen.
- Crear archivos nuevos sin confirmación previa.

---

## 7. Confirmación obligatoria
Si Claude no está 100% seguro de lo que debe hacer:
- Debe detenerse.
- Preguntar.
- Esperar confirmación.

---

Fin del archivo.
