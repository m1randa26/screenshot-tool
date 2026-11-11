# Spec: Herramienta de capturas para Windows

**Versión:** 1.2 · **Fecha:** 2026-11-10 · **Estado:** ✅ Aprobada · **Fase:** 1 (Especificación)
**Basado en:** [`brief.md`](brief.md)

> Esta spec describe **qué hace** el programa, no **con qué se construye**. La tecnología se decide en la Fase 2 (arquitectura).
>
> Marcas usadas:
> - **[P]** = *Propuesta*: decisión que sugirió la IA y que el usuario aún no aprueba.
> - **[?]** = *Pregunta*: falta información del usuario (ver §9).

---

## 1. Resumen

Programa para Windows que vive en segundo plano. Con `Ctrl + Alt + S` se selecciona una zona o la pantalla completa; la captura se abre siempre en un editor mínimo para anotar (texto, figuras, resaltador), y desde ahí se **copia** al portapapeles o se **guarda** en una carpeta fija, con una sola tecla.

## 2. Glosario

| Término | Significado |
|---|---|
| **Modo de selección** | Estado en que la pantalla se oscurece y el usuario elige qué capturar. |
| **Editor** | Ventana que muestra la captura y permite anotarla. |
| **Anotación** | Cualquier elemento agregado sobre la captura: texto, rectángulo, círculo, flecha o resaltado. |
| **Carpeta de guardado** | Carpeta fija donde `Ctrl + S` guarda las capturas. |
| **Bandeja** | Zona de íconos junto al reloj de Windows. |

## 3. Flujos

### F1: Capturar y pegar (flujo más común)
1. El usuario presiona `Ctrl + Alt + S`.
2. Aparece el modo de selección.
3. Arrastra para seleccionar una zona.
4. Se abre el editor con la captura.
5. Presiona `Ctrl + C`: la imagen pasa al portapapeles y el editor se cierra.
6. Pega en Slack/GitHub con `Ctrl + V`.

**Total después de seleccionar: 1 tecla.** (Criterio 2: máximo 2.)

### F2: Capturar, anotar y guardar (evidencia)
1. a 4. Igual que F1 (o un clic sin arrastrar para pantalla completa).
5. Agrega anotaciones; si se equivoca, `Ctrl + Z`.
6. Presiona `Ctrl + S`: se guarda en la carpeta de guardado y el editor se cierra.
7. Repite para cada paso del flujo. Sube los archivos desde el Explorador.

### F3: Descartar
1. Con el editor abierto, presiona `Esc` (o cierra la ventana).
2. Si hay anotaciones, el programa pregunta "¿Descartar la captura?". Si no hay anotaciones, se cierra sin preguntar.
3. Si confirma, el editor se cierra y la captura se pierde.

### F4: Cambiar carpeta de guardado
1. Clic derecho en el ícono de la bandeja → **Configuración**.
2. Elige una nueva carpeta con el selector de carpetas de Windows.
3. Las siguientes capturas se guardan ahí.

---

## 4. Requisitos funcionales

### 4.1 Segundo plano y sistema

| ID | Requisito |
|---|---|
| **R1** | El programa arranca automáticamente al iniciar sesión en Windows. |
| **R2** | Mientras corre, muestra un ícono en la bandeja. El menú (clic derecho) tiene: **Capturar**, **Configuración**, **Salir**. |
| **R3** | Solo puede haber una instancia corriendo. Si se abre otra vez, no se duplica. |
| **R4** | `Ctrl + Alt + S` activa el modo de selección desde cualquier programa. |
| **R5** | Si el atajo no se puede registrar (otro programa lo tomó), el programa avisa con una notificación al arrancar. El menú **Capturar** sigue funcionando como alternativa. |

### 4.2 Modo de selección

| ID | Requisito |
|---|---|
| **R6** | Al activarse, el programa **congela** lo que se ve en pantalla en ese instante y lo muestra oscurecido, con cursor en forma de cruz. Así se pueden capturar menús desplegables o tooltips que desaparecerían al mover el mouse. **[P]** |
| **R7** | **Arrastrar** selecciona una zona rectangular; la zona elegida se ve sin oscurecer. |
| **R8** | **Un clic sin arrastrar** captura la pantalla completa **del monitor donde se hizo clic**. |
| **R9** | `Esc` cancela el modo de selección sin abrir el editor. |
| **R10** | Si se presiona el atajo mientras hay un editor abierto, se ignora y el editor existente pasa al frente. **[P]** |

### 4.3 Editor

| ID | Requisito |
|---|---|
| **R11** | Al terminar la selección, el editor se abre **siempre**, mostrando la captura a tamaño real (si es más grande que la pantalla, se ajusta para que quepa). |
| **R12** | Herramientas: **texto**, **rectángulo**, **círculo**, **flecha**, **resaltador**. Se eligen desde una barra visible en el editor. |
| **R13** | Paleta de **al menos 8 colores**. El color elegido se aplica a las siguientes anotaciones. Color inicial: **rojo**. **[P]** |
| **R14** | **Texto:** clic donde se quiere colocar → escribir → `Enter` o clic fuera para confirmar. Una sola línea y tamaño fijo legible. |
| **R15** | **Rectángulo / círculo:** se dibujan arrastrando. Solo contorno, sin relleno. Grosor fijo. **[P]** |
| **R16** | **Flecha:** se dibuja arrastrando del inicio a la punta. Grosor fijo. **[P]** |
| **R17** | **Resaltador:** **trazo libre** (como marcador) con color semitransparente que deja ver lo de abajo. |
| **R18** | `Ctrl + Z` deshace la última anotación. Se puede repetir hasta volver a la captura original. |
| **R19** | `Ctrl + Y` rehace la última anotación deshecha. Si después de deshacer se crea una anotación nueva, ya no se puede rehacer lo deshecho. |
| **R20** | Una vez creada, una anotación no se puede mover ni editar; para corregir se usa `Ctrl + Z`. **[P]** |

### 4.4 Salida: copiar, guardar, descartar

| ID | Requisito |
|---|---|
| **R21** | `Ctrl + C` copia la imagen **con sus anotaciones** al portapapeles, cierra el editor y muestra una notificación breve ("Copiado al portapapeles"). La notificación aparece **solo si la copia realmente funcionó**: después de copiar se verifica que la imagen esté en el portapapeles. |
| **R22** | Si copiar falla, el programa **reintenta automáticamente** unas veces. Si sigue fallando: **no cierra el editor**, muestra un mensaje claro y deja intentar de nuevo o guardar con `Ctrl + S`. *(Criterio 1: nunca fallar en silencio ni perder la captura.)* |
| **R23** | `Ctrl + S` guarda la imagen con anotaciones como **PNG** en la carpeta de guardado, **sin copiarla**, y cierra el editor. |
| **R24** | Nombre automático: `captura_AAAA-MM-DD_HH-MM-SS.png`. Si ya existe, se agrega un sufijo (`_2`, `_3`…). |
| **R25** | Si la carpeta no existe, se crea. Si no se puede escribir en ella, **no se cierra el editor** y se muestra un mensaje claro. |
| **R26** | Después de guardar, aparece una notificación breve con el nombre del archivo. |
| **R27** | `Esc` o cerrar la ventana: **si hay anotaciones**, pregunta antes de descartar; si no hay, cierra sin preguntar. |

### 4.5 Configuración

| ID | Requisito |
|---|---|
| **R28** | Desde **Configuración** se puede cambiar la carpeta de guardado. |
| **R29** | Carpeta inicial: `Imágenes\Capturas` del usuario. **[P]** |
| **R30** | Desde **Configuración** se puede desactivar el arranque automático con Windows. **[P]** |
| **R31** | La configuración se conserva entre reinicios. |

### 4.6 Errores

| ID | Requisito |
|---|---|
| **R32** | Si ocurre un error inesperado, el programa **no se cierra en silencio**: lo registra en un archivo de log y avisa con una notificación. |

---

## 5. Requisitos no funcionales

| ID | Requisito |
|---|---|
| **NF1** | El modo de selección aparece en **menos de 250 ms** después del atajo. **[P]** *(Ajustada desde 200 ms en v1.2, ver §9.)* |
| **NF2** | El editor aparece en **menos de 300 ms** después de soltar la selección. *(Hace medible "sin espera perceptible", criterio 3.)* **[P]** |
| **NF3** | En reposo usa muy pocos recursos: **~0 % de CPU** y memoria baja (meta: menos de 50 MB). **[P]** |
| **NF4** | La captura conserva la **resolución nativa** y se ve correcta con escalado de pantalla (100 %, 125 %, 150 %). |
| **NF5** | Las imágenes no pierden calidad (PNG). |
| **NF6** | Solo **Windows 11**. |
| **NF7** | Se distribuye como un ejecutable, sin instalador. La configuración se guarda en la carpeta de datos del usuario. |

## 6. Casos borde

| Situación | Comportamiento esperado | Requisito |
|---|---|---|
| Copiar al portapapeles falla | Reintenta; si persiste, avisa y no cierra | R22 |
| Carpeta de guardado eliminada | Se vuelve a crear al guardar | R25 |
| Carpeta sin permisos o disco lleno | Avisa y no cierra el editor | R25 |
| Dos guardados en el mismo segundo | Sufijo `_2` | R24 |
| Atajo presionado con editor abierto | Se ignora; el editor pasa al frente | R10 |
| Atajo tomado por otro programa | Notificación; queda el menú Capturar | R5 |
| Zona de selección diminuta (p. ej. 2×2 px) | Se trata como clic → pantalla completa **[P]** | R8 |
| Pantalla con escalado 150 % | Captura nítida, sin desfase | NF4 |
| Varios monitores | La selección funciona en cualquier monitor; el clic captura solo el monitor donde se hizo | R7, R8 |
| Escribir texto y presionar `Esc` | Cancela solo ese texto, no la captura **[P]** | R14 |

## 7. Fuera de alcance v1

*(Del brief)* Video/GIF · captura de ventana específica · nube o enlaces · historial · Mac/Linux · instalador formal · elegir carpeta y nombre en cada guardado.

*(Nuevos, surgidos en la spec)* Mover o editar anotaciones ya creadas · elegir grosor de línea · texto multilínea o con formato · formatos distintos de PNG · recortar la captura dentro del editor.

## 8. Trazabilidad: criterios del brief → requisitos

| Criterio | Cubierto por |
|---|---|
| 1. Copiar nunca falla en silencio | R21 (notificación de éxito), R22 |
| 2. Máx. 2 teclas o clics para pegar sin anotar | F1 (1 tecla), R11, R21 |
| 3. Editor sin espera perceptible | R1, NF1, NF2 |
| 4. Todo se puede deshacer | R18, R19, R27 |
| 5. Una semana sin volver a la herramienta de Windows | Todo lo anterior + R1 (siempre disponible) + R32 (nunca muere en silencio) |

---

## 9. Historial de revisión

No quedan preguntas abiertas.

- **v1.2 (2026-11-10):** **NF1** se ajusta de 200 ms a **250 ms**, aprobado por el usuario en el hito del esqueleto. Medido con el .exe publicado: 188–239 ms tras optimizar. El resto del tiempo es congelar la pantalla (GDI) y mostrar las ventanas; bajarlo más exigía hilos o una técnica de captura más compleja, a cambio de unos 40 ms casi imperceptibles. Detalle en `checklist-manual.md`, sección "Hito".
- **v1.1 (2026-11-10):** a partir de la crítica de arquitectura se agregó **R32** (errores) y la verificación del portapapeles en **R21**.

### Resueltas (2026-11-10)
- **P1.** Clic sin arrastrar → monitor donde se hizo clic (R8).
- **P2.** Resaltador → trazo libre (R17).
- **P4.** `Esc` pregunta solo si hay anotaciones (R27).
- **P5.** Solo Windows 11 (NF6).
- **P6.** Notificación al guardar **y al copiar** (R21, R26).
- **P3.** Rehacer con `Ctrl + Y` → sí (R19).

### Propuestas [P]
Aprobadas por no tener objeciones (2026-11-10): R6, R10, R13, R15, R16, R20, R29, R30, NF1–NF3 y los casos borde marcados [P]. Se conserva la marca [P] en el texto para recordar que la decisión vino de la IA.
