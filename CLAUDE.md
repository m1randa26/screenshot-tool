# CapturaTool

Herramienta de capturas de pantalla para Windows 11. Vive en la bandeja; `Ctrl + Alt + S` → seleccionar zona o monitor → editor (texto, rectángulo, círculo, flecha, resaltador) → `Ctrl + C` copia / `Ctrl + S` guarda.

## Documentos (fuente de verdad)

Leer antes de trabajar. Si el código y los documentos no coinciden, **los documentos mandan**.

- `docs/brief.md`: problema, alcance, criterios de "terminado"
- `docs/spec.md`: requisitos numerados (R1–R32, NF1–NF7), flujos, casos borde
- `docs/architecture.md`: componentes, modelo de datos, integraciones, pruebas, riesgos
- `docs/adr/`: decisiones y su porqué (no reabrirlas sin una razón nueva)
- `docs/plan.md`: tareas en orden (Fase 4)

## Stack

C# · .NET 10 (LTS) · WinForms · xUnit · un .exe dependiente del runtime, con ReadyToRun · DPI PerMonitorV2

## Comandos

```bash
dotnet build
dotnet test
dotnet run --project src/CapturaTool
dotnet publish src/CapturaTool -c Release
dotnet publish src/CapturaTool -c Release -p:Channel=Insider   # versión Insider (ADR 0004)
```

## Convenciones

- Nombres en el código en **inglés**; comentarios y textos visibles para el usuario en **español**.
- La estructura de carpetas sigue `docs/architecture.md` §6. No usar `System` ni `Settings` como nombres de carpeta o namespace.
- La lógica (`AnnotationDocument`, `Annotations`, `ViewTransform`, `FileNamer`, `AppSettings`) **no depende de ventanas**. Las ventanas usan la lógica, nunca al revés.
- Todo `Bitmap`, `Graphics`, `Pen`, `Brush` y `Font` se libera con `using`/`Dispose`, y tiene un dueño claro (architecture.md, "Regla de liberación de memoria").
- Las anotaciones se guardan en coordenadas de **imagen**; la conversión con la pantalla la hace solo `ViewTransform`.
- Código **simple** por encima de ingenioso: el usuario está aprendiendo C#. Nada de patrones avanzados sin necesidad. Comentar solo lo que no sea obvio.
- No explicar conceptos de C# sin que se pida; si el usuario pregunta, explicar en español y con lenguaje sencillo.

## Reglas de trabajo

1. **Una tarea a la vez**, tomada de `docs/plan.md`. Cada tarea dice qué requisitos cumple (por ejemplo: "cumple R18, R19").
2. **Planear antes de escribir código**: proponer el enfoque, esperar aprobación y luego implementar.
3. **Toda lógica nueva lleva su prueba automática** antes de darse por terminada. `dotnet build` y `dotnet test` deben pasar.
4. Lo que no se puede probar automáticamente (atajo, overlay, portapapeles, DPI) se agrega a `docs/checklist-manual.md`.
5. **Nunca hacer commit sin aprobación.** Al terminar una tarea: resumir qué cambió, qué requisitos cubre y qué no se pudo verificar; luego esperar.
6. Mensajes de commit: `tipo: descripción en español (R##)`. Tipos: `feat`, `fix`, `test`, `docs`, `refactor`, `chore`.
7. **No agregar paquetes NuGet** sin preguntar.
8. **No modificar la spec, la arquitectura ni los ADRs por cuenta propia.** Si algo no cuadra, detenerse y plantearlo.
9. **Si algo falla dos veces seguidas**, detenerse y volver a planear en lugar de seguir parchando.
10. No implementar nada de "Fuera de alcance" (spec §7), aunque parezca fácil.
11. **Ramas (ADR 0004):** lo nuevo se hace en `insiders`; `main` solo recibe correcciones urgentes y lo que ya pasó la prueba Insider. Antes de empezar una tarea, comprobar en qué rama se está. Toda corrección en `main` se une también a `insiders`. `insiders` se une a `main` solo con un *pull request* aprobado por el usuario.
