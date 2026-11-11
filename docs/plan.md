# Plan de implementación

**Versión:** 1.0 · **Fecha:** 2026-11-10 · **Estado:** ✅ Aprobado · **Fase:** 4 (Plan)
**Basado en:** [`spec.md`](spec.md) v1.1 · [`architecture.md`](architecture.md) v1.0

> **Cómo se usa:** se toma **una tarea a la vez**, en orden. Cada tarea indica qué requisitos cumple y cuándo se considera terminada. Al terminarla, se marca `[x]` y se hace commit (con aprobación).
>
> **Orden:** primero lo más riesgoso. Las etapas 0 y 1 forman un **walking skeleton**: un programa feo pero completo de punta a punta (atajo → captura → editor → portapapeles) que valida los riesgos altos antes de invertir en el editor.

---

## Etapa 0: Base

- [x] **T01. Crear la solución**
  Proyecto WinForms (`src/CapturaTool`) + proyecto de pruebas xUnit (`tests/CapturaTool.Tests`). Configurar DPI *PerMonitorV2*, *ReadyToRun*, publicación en un solo archivo dependiente del runtime, `.gitignore` y `docs/checklist-manual.md` vacío.
  *Cumple:* NF4 (configuración), NF6, NF7
  *Hecho cuando:* `dotnet build` y `dotnet test` pasan (con una prueba de ejemplo) y la app abre y cierra sin error.

- [x] **T02. Programa en la bandeja, a prueba de fallos**
  `Program` + `SingleInstance` + `ErrorHandler`/`Logger` + `TrayApp` con ícono y menú (Capturar · Configuración, todavía deshabilitada · Salir).
  *Cumple:* R2, R3, R32
  *Hecho cuando:* el ícono aparece; abrir el .exe dos veces no duplica el programa; un error provocado a propósito queda en el log y se avisa con una notificación, sin que el programa muera.

## Etapa 1: Walking skeleton (riesgos altos)

- [x] **T03. Atajo global**
  `GlobalHotkey`: `Ctrl + Alt + S` y el menú **Capturar** disparan la misma acción (por ahora, solo una notificación de prueba). Avisar si el atajo no se puede registrar.
  *Cumple:* R4, R5
  *Hecho cuando:* el atajo funciona desde cualquier programa; con el atajo ocupado aparece el aviso y el menú sigue funcionando.

- [x] **T04. Captura y selección**
  `ScreenCapture` (todos los monitores, congelados) + `SelectionOverlay` (uno por monitor): arrastrar = zona, clic = monitor completo, `Esc` = cancelar. Una selección diminuta cuenta como clic.
  *Cumple:* R6, R7, R8, R9, NF4
  *Hecho cuando:* checklist manual: captura nítida al 100 %, 125 % y 150 %; dos monitores con escalado distinto; un menú desplegable se puede capturar.
  ⚠️ **Riesgo alto:** si el escalado mixto falla, detenerse y replantear antes de seguir.

- [x] **T05. Editor mínimo + coordenadas**
  `EditorForm` que muestra la captura ajustada al espacio disponible. `ViewTransform` con pruebas automáticas (conversión pantalla ↔ imagen con distintos zooms).
  *Cumple:* R11
  *Hecho cuando:* una captura de pantalla completa se ve entera en el editor; las pruebas de `ViewTransform` pasan.

- [x] **T06. Copiar al portapapeles (el dolor principal)**
  `ClipboardWriter`: dos formatos, reintentos, **verificación** posterior y notificación "Copiado al portapapeles". Si falla: mensaje claro y el editor sigue abierto. `Ctrl + C` cierra el editor si la copia funcionó.
  *Cumple:* R21 (sin anotaciones todavía), R22
  *Hecho cuando:* checklist manual: pegar en Slack y en GitHub funciona 10 de 10 veces; con el portapapeles bloqueado a propósito por otro programa, se avisa y no se pierde la captura.

### 🏁 Hito: esqueleto completo

Medir **NF1** (menos de 250 ms; era 200 ms hasta la spec v1.2), **NF2** (menos de 300 ms) y **NF3** (menos de 50 MB en reposo).
**Punto de decisión:** si no se cumplen, revisar el ADR 0002 (WinForms) **antes** de construir el editor. Corregir el rumbo aquí cuesta poco; después del editor, cuesta mucho.

## Etapa 2: Editor

- [x] **T07. Modelo de anotaciones + deshacer/rehacer**
  `AnnotationDocument` con pilas de deshacer y rehacer. Clase base de anotación. Solo lógica, con pruebas.
  *Cumple:* R18, R19, R20
  *Hecho cuando:* pruebas automáticas de agregar / deshacer / rehacer / "agregar después de deshacer vacía el rehacer".

- [x] **T08. Caché de dibujo + rectángulo, círculo y paleta**
  Caché (base + anotaciones terminadas) con doble búfer. Herramientas rectángulo y círculo. Paleta de 8 colores, rojo por defecto. `Ctrl + Z` / `Ctrl + Y` conectados al editor.
  *Cumple:* R12 (parcial), R13, R15, R18, R19
  *Hecho cuando:* se dibuja sin trabas sobre una captura 4K; deshacer y rehacer funcionan en pantalla.

- [x] **T09. Flecha**
  *Cumple:* R12 (parcial), R16
  *Hecho cuando:* la flecha apunta donde soltaste el mouse, también con zoom.

- [x] **T10. Resaltador de trazo libre**
  Trazo semitransparente uniforme (sin manchas más oscuras donde el trazo se cruza consigo mismo).
  *Cumple:* R12 (parcial), R17
  *Hecho cuando:* se puede resaltar texto y se sigue leyendo debajo.

- [ ] **T11. Texto**
  Clic → escribir → `Enter` o clic fuera confirma. `Esc` cancela solo ese texto. Tamaño fijo y legible en la imagen final.
  *Cumple:* R12 (completo), R14
  *Hecho cuando:* se escribe "bug" sobre la captura y queda legible al pegarla.

- [ ] **T12. Imagen final con anotaciones**
  `ImageRenderer` genera base + anotaciones en resolución original. `Ctrl + C` ahora copia con anotaciones.
  *Cumple:* R21 (completo)
  *Hecho cuando:* una prueba automática verifica que la imagen final contiene las anotaciones en la posición correcta; al pegar en Slack se ven igual que en el editor.

- [ ] **T13. Cerrar y descartar**
  `Esc` / cerrar ventana: pregunta solo si hay anotaciones. El atajo con el editor abierto lo trae al frente.
  *Cumple:* R10, R27
  *Hecho cuando:* los cuatro casos (con y sin anotaciones × Esc o cerrar ventana) se comportan según la spec.

## Etapa 3: Guardar y configuración

- [ ] **T14. Guardar en archivo**
  `FileNamer` (con pruebas: formato, duplicados en el mismo segundo) + `FileSaver` (crea la carpeta, maneja errores) + notificación con el nombre del archivo. `Ctrl + S` guarda sin copiar y cierra.
  *Cumple:* R23, R24, R25, R26, NF5
  *Hecho cuando:* pruebas de `FileNamer` pasan; con una carpeta borrada se recrea; con una carpeta sin permisos se avisa y el editor no se cierra.

- [ ] **T15. Configuración persistente**
  `AppSettings` en JSON: valores por defecto, archivo inexistente o dañado.
  *Cumple:* R29, R31
  *Hecho cuando:* pruebas automáticas de los tres casos.

- [ ] **T16. Ventana de configuración + arranque con Windows**
  `SettingsForm` (carpeta de guardado + casilla de arranque). `AutoStart` en el registro, reescribiendo la ruta en cada inicio. Activar el menú **Configuración**.
  *Cumple:* R1, R28, R30
  *Hecho cuando:* al reiniciar Windows el programa arranca solo; cambiar la carpeta afecta el siguiente guardado; moviendo el .exe, el arranque sigue funcionando.

## Etapa 4: Cierre

- [ ] **T17. Rendimiento y memoria**
  Volver a medir NF1–NF3 con el editor completo. Prueba de 20 capturas seguidas: la memoria vuelve al nivel de reposo. Completar el checklist manual entero.
  *Cumple:* NF1, NF2, NF3
  *Hecho cuando:* todas las metas se cumplen y el checklist está completo, o cada desviación está documentada con una decisión.

- [ ] **T18. Publicar y empezar la semana de prueba**
  Publicar el .exe, activar el arranque automático y desactivar la tecla Impr Pant de Windows si se desea. Durante 7 días, usar solo CapturaTool y anotar cada fallo.
  *Cumple:* criterio 5 del brief
  *Hecho cuando:* una semana completa sin volver a la herramienta de Windows. 🎯

---

## Trazabilidad: requisitos → tareas

| Requisitos | Tareas |
|---|---|
| R1, R28, R30 | T16 |
| R2, R3, R32 | T02 |
| R4, R5 | T03 |
| R6–R9 | T04 |
| R10, R27 | T13 |
| R11 | T05 |
| R12 | T08–T11 |
| R13, R15 | T08 |
| R14 | T11 |
| R16 | T09 |
| R17 | T10 |
| R18–R20 | T07, T08 |
| R21 | T06, T12 |
| R22 | T06 |
| R23–R26 | T14 |
| R29, R31 | T15 |
| NF1–NF3 | Hito, T17 |
| NF4 | T01, T04 |
| NF5 | T14 |
| NF6, NF7 | T01, T18 |

Todos los requisitos de la spec v1.1 tienen al menos una tarea.
