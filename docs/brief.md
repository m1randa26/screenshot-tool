# Brief: Herramienta de capturas para Windows

**Fecha:** 2026-11-10 · **Estado:** ✅ Aprobado · **Fase:** 0 (Definición del problema)

## Problema

La herramienta de captura de Windows falla con frecuencia (error "No se pudo copiar") y no permite agregar texto, solo figuras. Con un uso de 10 a 15 capturas diarias, estas fallas y limitaciones hacen perder tiempo de forma constante.

## Usuario

Una persona (yo). Uso intensivo, de 10 a 15 capturas al día, para documentar trabajo en **Slack** y **GitHub**.

## Flujo principal

1. Atajo de teclado
2. Seleccionar zona (o pantalla completa)
3. **Se abre siempre el editor**
4. Anotar si hace falta
5. **Copiar** al portapapeles → pegar en Slack/GitHub, **o guardar** en la carpeta fija → subir desde el Explorador de archivos

## Alcance v1

- Captura: **zona seleccionada** y **pantalla completa**
- Editor: **texto corto** (etiquetas como "bug", "botón", "1"), **rectángulo**, **círculo**, **flecha**, **resaltador**
- **Al menos 8 colores**
- **Deshacer (`Ctrl + Z`)**: prioridad alta
- **Copiar al portapapeles** con una tecla
- **Guardar con una tecla** en una carpeta fija, con nombre automático por fecha y hora (para que se ordenen solos)
- Configuración mínima para **cambiar la carpeta de guardado**

## Criterios de "terminado"

1. Copiar al portapapeles **nunca falla en silencio**: o funciona, o avisa y la captura no se pierde.
2. Desde el atajo hasta tener la imagen lista para pegar **sin anotar**: 2 teclas o clics como máximo después de seleccionar.
3. El editor abre **sin espera perceptible**.
4. Cualquier anotación se puede deshacer sin volver a capturar.
5. Uso el programa **una semana completa** en lugar de la herramienta de Windows sin regresar a ella.

## Fuera de alcance v1

- Video / GIF
- Captura de ventana específica
- Subir a la nube o compartir con un enlace
- Historial de capturas
- Mac / Linux
- Instalador formal (basta un ejecutable)
- Elegir carpeta y nombre en cada guardado

## Preguntas abiertas (se resuelven en la spec)

- [x] ¿Qué atajo de teclado? → **`Ctrl + Alt + S`** (verificado sin conflictos). Un solo atajo entra al modo de selección; ahí se elige zona (arrastrar) o pantalla completa (opción B).
- [x] ¿El programa vive en segundo plano (ícono junto al reloj) o se abre cada vez? → **Segundo plano** (deducido: el atajo global y el criterio 3 lo requieren). **Arranca solo al encender Windows** (confirmado). En el modo de selección, **un clic sin arrastrar = pantalla completa**. Menú del ícono: **Capturar / Configuración / Salir**.
- [x] ¿Copiar y guardar a la vez con una sola acción, o son acciones separadas? → **Separadas.** `Ctrl + C` copia y cierra. `Ctrl + S` solo guarda (no copia) y cierra. `Esc` pregunta antes de descartar.

> Todas las preguntas abiertas están resueltas. El detalle vive en [`spec.md`](spec.md).

## Origen de las decisiones

| Decisión | Razón (de la entrevista) |
|---|---|
| El portapapeles es crítico | Las capturas se pegan en Slack/GitHub con `Ctrl + V` |
| La velocidad es requisito | Se usa de 10 a 15 veces al día; cada paso extra se multiplica |
| El editor se abre siempre (opción A) | Elección del usuario |
| Texto simple, sin formato | Solo se usan etiquetas cortas |
| Guardar entra a v1 | Caso real: evidencia de flujos con varias capturas |
| Carpeta fija configurable | Elegir destino cada vez es pérdida de tiempo |

---

## Ciclo 2: identidad visual

**Fecha:** 2026-11-12 · **Estado:** ✅ Aprobado

### Problema

El programa usa el ícono genérico de Windows (un recuadro de aplicación). En la bandeja se confunde con otros programas, y en la barra de tareas y el Explorador no se distingue qué es.

### Alcance

- Un **logo propio**, simple y reconocible a 16 px.
- El logo se usa en el **ícono de la bandeja**, las **ventanas** (editor y Configuración), la **barra de tareas** y el **archivo .exe**.

### Criterios de "terminado"

1. En la bandeja se reconoce CapturaTool de un vistazo, sin pasar el mouse por encima.
2. El logo se ve nítido con escalado de 100 %, 125 % y 150 %.
3. El logo se distingue con la barra de tareas en tema claro y en tema oscuro.

### Fuera de alcance

- Cambiar el diseño de las ventanas, la barra de herramientas o los colores del editor.
- Íconos para las herramientas del editor (siguen siendo botones con texto).
- Un logo animado o que cambie según el estado del programa.

---

## Ciclo 3: Insiders

**Fecha:** 2026-11-13 · **Estado:** ✅ Aprobado · **Rama:** `insiders` ([ADR 0004](adr/0004-rama-insiders.md))

### Problema

- Para copiar **texto que está en una imagen** (un mensaje de error en una captura, código en un video, un PDF escaneado) hay que escribirlo a mano.
- Capturar exige el teclado. Cuando la mano está en el mouse, los **botones laterales** quedan sin usar.
- La interfaz funciona pero se ve **anticuada**: botones de solo texto, sin tema oscuro, sin el aspecto de Windows 11.
- Probar estas funciones no debe poner en riesgo la versión estable que se usa todos los días.

### Alcance

- **Versión Insider** instalada junto a la estable, sin chocar con ella (ADR 0004).
- **Leer texto:** un atajo propio → seleccionar una zona → el texto queda en el portapapeles, sin pasar por el editor.
- **Botones laterales del mouse:** uno abre la captura normal y el otro, leer texto.
- **Interfaz moderna, en WinForms:** barra con íconos, tema claro y oscuro según Windows y aspecto de Windows 11, en el editor y en Configuración.

### Criterios de "terminado"

1. Leer texto: desde el atajo hasta pegar el texto, **sin ningún paso extra** después de seleccionar. El texto sale correcto en capturas de texto de pantalla normal, en español y en inglés.
2. Con **solo el mouse**: botón lateral → seleccionar → editor (o texto en el portapapeles), sin tocar el teclado.
3. La interfaz sigue el **tema de Windows** (claro u oscuro) y se ve como un programa de Windows 11.
4. La estable y la Insider **conviven** en la misma PC sin interferir.
5. Las funciones nuevas mantienen las metas de rapidez y memoria (NF1–NF3).
6. Cada función lleva **3 días de uso real sin fallos** en la Insider antes de pasar a `main`.

### Fuera de alcance

- Texto escrito a mano, traducción, o leer texto de una captura que ya está abierta en el editor.
- Elegir qué botón del mouse hace cada cosa (los botones laterales quedan fijos).
- Cambiar de framework (WPF, WinUI 3): la interfaz se moderniza dentro de WinForms (ADR 0002 sin cambios).
- Rediseñar el flujo de captura: los pasos y atajos actuales se mantienen.
