# Arquitectura: Herramienta de capturas para Windows

**Versión:** 1.2 · **Fecha:** 2026-11-12 · **Estado:** ✅ Aprobada · **Fase:** 2 (Arquitectura)
**Basado en:** [`spec.md`](spec.md) v1.1 · **Decisiones:** [ADR 0001](adr/0001-tecnologia.md), [ADR 0002](adr/0002-interfaz-winforms.md), [ADR 0003](adr/0003-distribucion.md)

> Este documento explica **cómo está organizado** el programa: sus piezas, qué hace cada una y cómo se comunican. Los requisitos (R*, NF*) remiten a la spec.
>
> **[P]** = propuesta de la IA pendiente de aprobación.

---

## 1. Stack

| Elemento | Elección | Referencia |
|---|---|---|
| Lenguaje y plataforma | C# sobre .NET 10 (LTS) | ADR 0001 |
| Interfaz | WinForms | ADR 0002 |
| Distribución | Un .exe, dependiente del runtime | ADR 0003 |
| Pruebas | xUnit **[P]** (elegido por ser el estándar más común, sin comparación formal; fácil de cambiar) | §7 |
| Compilación | *ReadyToRun* (compilación anticipada) para que la primera apertura del editor no pase de 350 ms | NF2 |
| Configuración | Archivo JSON en `%APPDATA%\CapturaTool\` **[P]** | R31 |

## 2. Vista general

El programa es **un solo proceso** que vive en la bandeja. No tiene ventana principal: las ventanas (selección, editor, configuración) se crean cuando se necesitan.

```
                    ┌──────────────────────────────┐
  Ctrl+Alt+S ──────▶│        TrayApp (núcleo)      │◀── Menú bandeja
                    │  ícono · atajo · coordinador │    (Capturar/Config/Salir)
                    └──────┬───────────────────────┘
                           │ 1. pide captura
                           ▼
                    ┌──────────────┐  2. imagen congelada   ┌────────────────┐
                    │ ScreenCapture│ ─────────────────────▶ │ SelectionOverlay│
                    └──────────────┘                        └───────┬────────┘
                                                                    │ 3. recorte elegido
                                                                    ▼
                    ┌──────────────────────────────────────────────────────┐
                    │                    EditorForm                        │
                    │   AnnotationDocument (anotaciones + deshacer/rehacer)│
                    └───────┬─────────────────────────────┬────────────────┘
                     Ctrl+C │                             │ Ctrl+S
                            ▼                             ▼
                    ┌───────────────┐             ┌───────────────┐
                    │ClipboardWriter│             │   FileSaver   │──▶ Settings
                    └───────┬───────┘             └───────┬───────┘   (carpeta)
                            └──────────┬──────────────────┘
                                       ▼
                                 Notifier (aviso)
```

## 3. Componentes

| Componente | Responsabilidad | Requisitos |
|---|---|---|
| **Program** | Punto de entrada. Garantiza una sola instancia, instala el ErrorHandler y arranca TrayApp. | R3, R32 |
| **ErrorHandler** + **Logger** | Atrapa cualquier error inesperado (en la interfaz o en segundo plano), lo escribe en `%APPDATA%\CapturaTool\logs\` y avisa con una notificación. El programa **nunca muere en silencio**. | R32 |
| **TrayApp** | Ícono y menú de la bandeja, registro del atajo global, coordina el flujo completo. | R2, R4, R5, R10 |
| **SingleInstance** | Impide abrir el programa dos veces. | R3 |
| **GlobalHotkey** | Registra `Ctrl + Alt + S` en Windows y avisa si no se pudo. | R4, R5 |
| **AutoStart** | Activa o desactiva el arranque con Windows. Si está activo, **reescribe la ruta del .exe en cada inicio** (por si se movió de carpeta). | R1, R30 |
| **ScreenCapture** | Toma una imagen de todos los monitores en el instante del atajo ("congelar"). | R6, NF4 |
| **SelectionOverlay** | Muestra la imagen congelada oscurecida; arrastrar = zona, clic = monitor completo, `Esc` = cancelar. | R6–R9 |
| **EditorForm** | Ventana del editor: barra de herramientas, paleta, lienzo y teclas. Dibuja usando una **imagen en caché** (ver §4.3). | R11–R17, R27 |
| **ViewTransform** | Convierte coordenadas entre **pantalla** (donde está el mouse) e **imagen** (donde se guardan las anotaciones), según el zoom con el que se muestra la captura. Lógica pura, sin ventanas. | R11 |
| **AnnotationDocument** | El "modelo" del editor: imagen base + lista de anotaciones + pilas de deshacer y rehacer. **No conoce la interfaz.** | R18–R20 |
| **Annotations** | Una clase por tipo: Rectangle, Ellipse, Arrow, Highlighter, Text. Cada una sabe dibujarse. | R12–R17 |
| **ImageRenderer** | Genera la imagen final (base + anotaciones). | R21, R23 |
| **ClipboardWriter** | Copia con reintentos y luego **verifica leyendo el portapapeles** que la imagen quedó ahí. Solo reporta éxito si la verificación pasa. | R21, R22 |
| **FileSaver** + **FileNamer** | Guarda el PNG; genera el nombre y resuelve duplicados; crea la carpeta. | R23–R25 |
| **Notifier** | Muestra avisos breves de Windows. | R5, R21, R26 |
| **AppSettings** + **SettingsForm** | Leer y guardar la configuración; ventana para cambiarla. | R28–R31 |
| **AppIcon** *(Ciclo 2)* | Carga el logo desde el `.exe` y lo entrega al tamaño que pide Windows (bandeja chica, ventanas). | R33 |

### Regla de dependencias

Las piezas de **lógica** (`AnnotationDocument`, `Annotations`, `ViewTransform`, `FileNamer`, `AppSettings`) **no dependen de ventanas**. Las ventanas usan la lógica, nunca al revés. Así la lógica se puede probar de forma automática (§7) y es más fácil de entender por separado.

## 4. Modelo de datos

### Anotaciones y deshacer/rehacer (R18–R20)

```
AnnotationDocument
├── BaseImage        (la captura original; nunca se modifica)
├── Annotations      (lista en orden de creación)
├── RedoStack        (anotaciones deshechas)
│
├── Add(a)   → agrega a Annotations y VACÍA RedoStack   (R19: lo deshecho se pierde)
├── Undo()   → mueve la última de Annotations a RedoStack
└── Redo()   → mueve la última de RedoStack a Annotations
```

Como las anotaciones no se editan después de crearse (R20), deshacer y rehacer se reducen a **mover elementos entre dos listas**. La imagen original no se toca nunca: en pantalla se redibuja *base + anotaciones*. Así no hay forma de "dañar" la captura con un error.

### Configuración (R28–R31)

```json
{
  "saveFolder": "C:\\Users\\<usuario>\\Pictures\\Capturas",
  "startWithWindows": true
}
```

Si el archivo no existe o está dañado, se usan los valores por defecto y no se bloquea el arranque. **[P]**

### Dibujo en el editor (rendimiento)

Redibujar una captura grande completa en cada movimiento del mouse traba el editor. Por eso:

```
Caché    = imagen base + anotaciones terminadas    (se regenera solo con Add / Undo / Redo)
Pantalla = Caché (escalada por ViewTransform) + figura que se está trazando ahora
```

Mientras arrastras, solo se dibuja la figura en curso sobre la caché. El lienzo usa doble búfer para evitar parpadeo.

### Coordenadas (R11)

Las anotaciones se guardan **siempre en coordenadas de la imagen**, nunca de la pantalla. Así, sin importar el zoom del editor, la imagen final queda exacta. `ViewTransform` hace las dos conversiones (pantalla → imagen al recibir el mouse; imagen → pantalla al dibujar).

### Regla de liberación de memoria

En .NET, las imágenes (`Bitmap`) y los objetos de dibujo (`Graphics`, `Pen`, `Brush`, `Font`) **se liberan a mano** con `Dispose` (o `using`). Una captura de pantalla completa ocupa decenas de MB; una fuga se nota después de días de uso.

- Todo `Bitmap` tiene **un dueño claro** que lo libera.
- La captura congelada se libera al cerrar el overlay; la imagen base y la caché, al cerrar el editor.
- Se verifica en el checklist: la memoria vuelve al nivel de reposo después de 20 capturas seguidas.
- Los íconos (`Icon`) también se liberan: **TrayApp** es dueño del logo durante toda la vida del programa y lo libera al salir. Las ventanas lo usan prestado (`Form.Icon`) y no lo liberan. *(Ciclo 2)*

## 5. Integraciones con Windows

Estas son las partes de **mayor riesgo**: es donde la herramienta de Windows te falla hoy.

| Necesidad | Mecanismo | Nota |
|---|---|---|
| **Portapapeles (R21, R22)** | Funciones nativas de Windows (`OpenClipboard`, `SetClipboardData`), con reintentos (unos 10 intentos, separados unos 100 ms). *Hasta la v1.0 era `Clipboard.SetDataObject`; ver el historial.* | La causa típica de "No se pudo copiar" es que **otro programa tiene el portapapeles abierto** en ese instante. Reintentar lo resuelve casi siempre. Se pone la imagen en **dos formatos** (mapa de bits estándar DIB + PNG) para que Slack y GitHub (navegador) la acepten bien. **[P]** |
| **Atajo global (R4)** | API `RegisterHotKey` de Windows | Devuelve error si otro programa ya lo tiene → R5. |
| **Escalado por monitor (NF4)** | Modo DPI *PerMonitorV2* en la configuración de la app | Sin esto, con escalado al 125–150 % la captura sale borrosa o desfasada. |
| **Captura (R6)** | Copiar la pantalla completa (todos los monitores) a una imagen en memoria al presionar el atajo | Esto es lo que permite "congelar" menús y tooltips. |
| **Overlay de selección** | Una ventana sin bordes, siempre encima, **por cada monitor** **[P]** | Una ventana por monitor evita problemas cuando cada monitor tiene un escalado distinto. |
| **Bandeja y avisos (R2, R21, R26)** | `NotifyIcon` de WinForms | En Windows 11 los avisos se muestran como notificaciones del sistema. |
| **Arranque con Windows (R1, R30)** | Entrada en el registro del usuario: `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` | No requiere permisos de administrador. |
| **Instancia única (R3)** | *Mutex* con nombre | Mecanismo estándar de Windows. |
| **Logo (R33)** *(Ciclo 2)* | Un archivo `.ico` con varios tamaños (16, 20, 24, 32, 40, 48, 64 y 256 px). En el `.csproj`: `ApplicationIcon` (ícono del `.exe` en el Explorador y la barra de tareas) y `EmbeddedResource` (para cargarlo desde el código). La bandeja usa el tamaño chico del sistema (`SystemInformation.SmallIconSize`), que cambia con el escalado. | Con cada tamaño dibujado aparte, Windows no estira uno chico y el logo queda nítido al 100–150 %. Sin paquetes NuGet. |

## 6. Estructura de carpetas **[P]**

```
captura-tool/
├── docs/                       ← brief, spec, arquitectura, ADRs
├── src/
│   └── CapturaTool/            ← la aplicación (WinForms)
│       ├── Program.cs
│       ├── TrayApp.cs
│       ├── Platform/           ← SingleInstance, GlobalHotkey, AutoStart, Notifier, ErrorHandler, Logger, NativeClipboard
│       ├── Capture/            ← ScreenCapture, SelectionOverlay
│       ├── Editor/             ← EditorForm, AnnotationDocument, ViewTransform, Annotations/
│       ├── Output/             ← ImageRenderer, ClipboardWriter, FileSaver, FileNamer
│       ├── Configuration/      ← AppSettings, SettingsForm
│       └── Resources/          ← CapturaTool.ico (Ciclo 2)
├── tests/
│   └── CapturaTool.Tests/      ← pruebas automáticas (xUnit)
├── tools/
│   └── IconMaker/              ← programa chico que dibuja el logo y genera el .ico (Ciclo 2; fuera de la solución)
└── CLAUDE.md                   ← reglas para la IA (Fase 3)
```

Los nombres en el código van en **inglés** (convención de C#) y los comentarios y textos visibles en **español**. **[P]**

> Las carpetas se llaman `Platform` y `Configuration` (y no `System` o `Settings`) a propósito: en C#, una carpeta suele convertirse en *namespace*, y un namespace `CapturaTool.System` taparía al `System` de .NET, mientras que uno `Settings` con una clase `Settings` adentro causa ambigüedades.

## 7. Estrategia de pruebas

| Tipo | Qué cubre | Cómo |
|---|---|---|
| **Automáticas** | AnnotationDocument (deshacer/rehacer), ViewTransform (conversión ida y vuelta con distintos zooms), FileNamer (formato, duplicados), AppSettings (valores por defecto, archivo dañado), ImageRenderer (la imagen final contiene las anotaciones), AppIcon (el logo está dentro del .exe y trae todos los tamaños) | xUnit, se corren con `dotnet test` |
| **Manuales (checklist)** | Atajo, overlay, multimonitor, **dos monitores con escalados distintos**, portapapeles con Slack/GitHub, arranque con Windows, avisos, **memoria tras 20 capturas**, error provocado → aparece en el log y se avisa | Lista en `docs/checklist-manual.md` (se crea en la Fase 4) |
| **Rendimiento** | NF1, NF2 (tiempos), NF3 (memoria) | Medición manual con cronómetro interno en modo debug y el Administrador de tareas |

Las integraciones con Windows son difíciles de automatizar; por eso la lógica se mantiene separada (§3): **todo lo que se puede probar automáticamente, se prueba**, y lo demás se cubre con el checklist.

## 8. Riesgos

| Riesgo | Impacto | Mitigación |
|---|---|---|
| El portapapeles sigue fallando a pesar de los reintentos | Alto (es el dolor principal) | R22: el editor no se cierra y se puede guardar. Se prueba temprano (Fase 4, primeras tareas). |
| Captura borrosa o desfasada con escalado o multimonitor | Alto | PerMonitorV2 + overlay por monitor; probarlo en el *walking skeleton* con dos monitores de escalado distinto. El soporte de WinForms para esto tiene detalles finos. |
| El programa falla en segundo plano sin que se note | Alto (rompe el criterio 5) | ErrorHandler + log + notificación (R32). |
| Fuga de memoria con el uso diario | Medio | Regla de liberación de memoria + prueba de 20 capturas. |
| Anotaciones corridas respecto del mouse con zoom | Alto (se ve en la primera captura de pantalla completa) | ViewTransform con pruebas automáticas. |
| El editor tarda más de 300 ms la primera vez | Medio | ReadyToRun + medir en el prototipo; si no alcanza, precargar el editor oculto al arrancar. |
| WinForms no cumple NF3 (memoria) | Medio | La ventaja frente a WPF es estimada, no medida: comprobarlo en el *walking skeleton*. |
| Aprender C# frena el avance | Medio | Código simple, comentado, tareas pequeñas (ADR 0001). |

---

## 9. Historial de revisión

- **2026-11-10:** propuestas [P] aprobadas.
- **2026-11-10 (crítica):** se agregaron ErrorHandler + Logger (R32), ViewTransform, caché de dibujo, regla de liberación de memoria, verificación del portapapeles, reescritura de la ruta de arranque, ReadyToRun y nuevos casos del checklist. La ventaja de WinForms se marcó como *estimada, por medir*.
- **v1.1 (2026-11-12, T17, aprobado por el usuario):** el portapapeles se escribe con las **funciones nativas de Windows** en vez de `Clipboard.SetDataObject`. Medido en T17: con `SetDataObject` y un `Bitmap`, cada copia dejaba la imagen entera sin liberar (+8 MB y +1 objeto GDI por copia de 1920 × 1080, sin límite), lo que rompía NF3. Con las funciones nativas la memoria queda estable. Se mantienen los reintentos, la verificación y los dos formatos (DIB + PNG). Nuevo componente `NativeClipboard` en `Platform/`. También en T17: la meta de NF2 pasa a 350 ms (spec v1.3).
- **v1.2 (2026-11-12, aprobada por el usuario):** Ciclo 2 (R33, logo). Nuevo componente `AppIcon`, carpeta `Resources/` con el `.ico`, herramienta `tools/IconMaker` para regenerarlo, regla de liberación para los íconos y prueba automática de los tamaños.
