# Checklist manual

Pruebas que no se pueden automatizar (atajo, overlay, portapapeles, DPI, etc.). Cada tarea del [plan](plan.md) agrega aquí sus casos.

**Cómo se usa:** antes de dar por terminada una tarea (y completo en T17), se recorre su sección y se marca cada caso con `[x]` si se comportó como se espera. Si algo falla, se anota qué pasó junto al caso.

---

## T02. Bandeja, instancia única y errores

- [x] Al abrir el programa aparece el ícono en la bandeja (puede estar dentro de la flecha `^` de íconos ocultos). (R2)
- [x] Clic derecho en el ícono: el menú muestra **Capturar**, **Configuración** (deshabilitada) y **Salir**. (R2)
- [x] **Capturar** muestra un aviso provisional. (R2)
- [x] Abrir el .exe por segunda vez no agrega otro ícono ni otro proceso en el Administrador de tareas. (R3)
- [x] En compilación de desarrollo, **Provocar error (prueba)**: aparece una notificación de error y el programa sigue vivo (el menú sigue respondiendo). (R32)
- [x] Después de provocar el error, el archivo `%APPDATA%\CapturaTool\logs\CapturaTool_AAAA-MM-DD.log` contiene el error con su detalle. (R32)
- [x] **Salir** quita el ícono de la bandeja y termina el proceso.
- [x] En el .exe publicado (`Release`) no aparece la opción **Provocar error (prueba)**.

## T03. Atajo global

- [x] Con el foco en el **navegador**, `Ctrl + Alt + S` muestra la notificación "Captura iniciada (prueba)". (R4)
- [x] Lo mismo con el foco en **Slack**. (R4)
- [x] Lo mismo con el foco en el **Explorador de archivos**. (R4)
- [x] El menú **Capturar** muestra la misma notificación. (R4)
- [x] Mantener `Ctrl + Alt + S` apretado no repite la notificación.
- [x] **Atajo ocupado:** con CapturaTool cerrado, abrir PowerShell y ejecutar el bloque de abajo (deja `Ctrl + Alt + S` tomado mientras esa ventana siga abierta). Abrir CapturaTool: aparece el aviso "atajo no disponible" y el menú **Capturar** sigue funcionando. Cerrar la ventana de PowerShell al terminar. (R5)

```powershell
Add-Type -Namespace W -Name U -MemberDefinition '[DllImport("user32.dll")] public static extern bool RegisterHotKey(IntPtr h, int id, int m, int vk);'
[W.U]::RegisterHotKey([IntPtr]::Zero, 1, 3, 0x53)   # True = atajo tomado
Read-Host "Atajo tomado. Presiona Enter para soltarlo"
```

## T04. Captura y selección

En cada caso, la ventana "Vista previa (provisional)" muestra la captura y sus medidas en píxeles.

**Selección (R6–R9)**

- [x] `Ctrl + Alt + S`: la pantalla se congela y se oscurece, con cursor en cruz. (R6)
- [x] El overlay cubre también la barra de tareas.
- [x] Arrastrar hacia abajo-derecha, arriba-izquierda, arriba-derecha y abajo-izquierda: la zona se ve sin oscurecer y la vista previa muestra exactamente esa zona. (R7)
- [x] Un clic sin arrastrar captura el monitor completo donde se hizo clic; la vista previa muestra la resolución nativa del monitor (por ejemplo 1920 × 1080). (R8)
- [x] Una selección diminuta (un "temblor" del mouse de 1–3 px) cuenta como clic. (R8)
- [x] `Esc` cancela: se cierran los overlays y no se abre nada. (R9)
- [x] Presionar `Ctrl + Alt + S` con la selección abierta no abre otra.
- [x] Capturar desde el menú **Capturar**: el menú no aparece en la captura.

**Congelar (R6)**

- [x] Abrir un menú desplegable (por ejemplo, clic derecho en el escritorio), presionar `Ctrl + Alt + S`: el menú aparece en la captura.
- [x] Dejar visible un tooltip (pasar el mouse sobre un botón) y presionar `Ctrl + Alt + S`: el tooltip aparece en la captura.

**Escalado (NF4)** — cambiar en Configuración de Windows → Pantalla → Escala

- [x] Al **100 %**: captura nítida, medidas iguales a la resolución del monitor.
- [x] Al **125 %**: captura nítida (texto sin borrosidad), medidas iguales a la resolución del monitor, sin desfase entre lo seleccionado y lo capturado.
- [x] Al **150 %**: igual que el caso anterior.
- [x] **Dos monitores con escalado distinto** (si se tienen): cada overlay cubre exactamente su monitor; seleccionar en cada uno captura lo correcto y nítido.

**Memoria**

- [x] Después de cerrar la vista previa, la memoria del proceso (Administrador de tareas) vuelve aproximadamente al nivel anterior a la captura.

## T05. Editor mínimo

- [x] Un clic (pantalla completa) abre el editor maximizado y la captura se ve **entera**, sin barras de desplazamiento. (R11)
- [x] Una zona pequeña (por ejemplo 300 × 200) se ve a **tamaño real** y nítida, con la ventana ajustada a ella. (R11)
- [x] Una zona diminuta (por ejemplo 40 × 40) abre una ventana de al menos 400 × 300 con la captura centrada a tamaño real.
- [x] Al cambiar el tamaño de la ventana, la captura se reajusta para caber y nunca se agranda más allá del tamaño real. (R11)
- [x] Con dos monitores, el editor se abre en el monitor donde se hizo la selección.
- [x] Con un monitor al 125 % o 150 %: la captura de pantalla completa se ve entera y una zona pequeña se ve nítida.
- [x] Después de cerrar el editor, la memoria del proceso vuelve aproximadamente al nivel de reposo (puede tardar unos segundos).

## T06. Copiar al portapapeles

- [x] `Ctrl + C` en el editor: aparece "Copiado al portapapeles" y el editor se cierra. (R21)
- [x] Pegar en **Slack** con `Ctrl + V`: funciona **10 de 10** veces (una captura nueva cada vez). (R21)
- [x] Pegar en **GitHub** (un comentario de issue en el navegador): funciona **10 de 10** veces. (R21)
- [x] La imagen pegada se ve igual de nítida que la captura (mismas medidas).
- [x] Después de copiar, usar **Salir** en CapturaTool: la imagen se puede seguir pegando.
- [x] **Portapapeles bloqueado:** ejecutar en PowerShell el bloque de abajo (bloquea el portapapeles 15 segundos) y, mientras tanto, presionar `Ctrl + C` en el editor: tras un segundo aparece el aviso "No se pudo copiar…", **no** aparece la notificación de éxito y el editor sigue abierto con la captura. (R22)
- [x] Al terminar el bloqueo, `Ctrl + C` de nuevo funciona y cierra el editor. (R22)
- [x] El log contiene "No se pudo copiar al portapapeles tras 10 intentos" con el detalle del error. (R22)

```powershell
Add-Type -Namespace W -Name C -MemberDefinition '[DllImport("user32.dll")] public static extern bool OpenClipboard(IntPtr h); [DllImport("user32.dll")] public static extern bool CloseClipboard();'
[W.C]::OpenClipboard([IntPtr]::Zero)   # True = portapapeles bloqueado
Start-Sleep -Seconds 15
[W.C]::CloseClipboard()
```

## Hito: esqueleto completo (mediciones)

**Fecha:** 2026-11-10 · **Build:** Release publicado (ReadyToRun) · **Equipo:** 2 monitores 1920 × 1080 al 100 %, 12 procesadores lógicos · **Método:** cronómetro interno (líneas `Medición:` del log) y medición del proceso por script; atajo y mouse simulados.

| Meta | Resultado | ¿Cumple? |
|---|---|---|
| **NF1:** selección visible en menos de 200 ms | 227, 203, 284, 236, 275 ms (5 capturas) | ❌ No (todas por encima) |
| **NF2:** editor visible en menos de 300 ms | Pantalla completa: 284, 331, 286 ms · Zona 600 × 400: 235, 235 ms | ❌ No (1 de 5 por encima; pantalla completa al límite) |
| **NF3:** CPU en reposo ~0 % | 0 ms de CPU en 60 s | ✅ Sí |
| **NF3:** memoria en reposo menos de 50 MB | Al arrancar: 6,4 MB privado activo (columna por defecto del Administrador de tareas) / 42,5 MB conjunto de trabajo. Tras 5 capturas: 11,6 MB / 59 MB | ✅ Sí con la columna del Administrador de tareas · ⚠️ el conjunto de trabajo total pasa de 50 MB tras usarlo |

**Diagnóstico:** el tiempo se iba en congelar la pantalla con GDI (68–118 ms), crear y destruir ventanas (hasta 132 ms) y la interpolación bicúbica (34–52 ms). Nada de eso se resolvería cambiando de framework, así que se optimizó dentro de WinForms (opción A): ventanas de selección creadas una vez y reutilizadas, overlays que se ocultan en vez de destruirse e interpolación bilineal en la vista reducida.

**Después de optimizar (opción A), mismo método, 8 capturas:**

| Meta | Resultado | ¿Cumple? |
|---|---|---|
| **NF1:** selección visible en menos de 200 ms | 190, 239, 191, 209, 215, 216, 205, 188 ms (congelar 62–105 · mostrar + copia oscura 79–103 · dibujar 22–38) | ⚠️ Casi: 3 de 8 por debajo; el resto pasa por 5–39 ms |
| **NF2:** editor visible en menos de 300 ms | 166, 119, 115, 87, 229, 123, 153 ms | ✅ Sí (máximo 229 ms) |

**Decisión (2026-11-10):** se mantiene WinForms (ADR 0002 sin cambios). El usuario aprobó ajustar **NF1 a menos de 250 ms** (spec v1.2): el resto del tiempo es congelar la pantalla con GDI y mostrar las ventanas, y bajarlo más exigía hilos o una técnica de captura más compleja a cambio de unos 40 ms. Con la meta nueva, NF1 se cumple (máximo medido: 239 ms), igual que NF2 y NF3. Se vuelve a medir en T17 con el editor completo.

**Nota (T08, aprobada por el usuario):** la caché de dibujo del editor se guarda **ya reducida, al tamaño del lienzo**, y no a la resolución de la imagen como sugiere la redacción de architecture.md §4. Así, al mover el mouse solo se copia la caché 1:1 (reducir una imagen grande en cada movimiento costaba 34–52 ms con 1080p). La caché se regenera con agregar, deshacer, rehacer y al cambiar el tamaño de la ventana.

## T08. Rectángulo, círculo, paleta y deshacer

- [x] La barra muestra **Rectángulo** (elegido al abrir), **Círculo** y 8 colores, con **rojo** elegido al abrir. (R12, R13)
- [x] Arrastrar con **Rectángulo** dibuja un rectángulo: solo contorno, sin relleno, mismo grosor siempre. (R15)
- [x] Arrastrar con **Círculo** dibuja un óvalo: solo contorno, sin relleno. (R15)
- [x] Elegir otro color: las figuras siguientes salen de ese color; las anteriores no cambian. (R13)
- [x] Mientras se arrastra, la figura sigue al mouse **sin trabas ni parpadeo**, también en una captura de pantalla completa (que se ve reducida).
- [x] En una captura de pantalla completa, la figura empieza y termina exactamente donde está el mouse (sin desfase por el zoom).
- [x] Un clic sin arrastrar no crea nada (`Ctrl + Z` no "deshace" nada invisible).
- [x] Arrastrar hasta fuera de la captura: la figura se queda en el borde de la imagen.
- [x] `Ctrl + Z` varias veces quita las figuras de la última a la primera, hasta la captura original. (R18)
- [x] `Ctrl + Y` las vuelve a poner en orden. (R19)
- [x] Deshacer, dibujar una figura nueva y presionar `Ctrl + Y`: no vuelve lo deshecho. (R19)
- [x] Una captura pequeña se sigue viendo a tamaño real, con la barra arriba.
- [x] Con el monitor al **150 %**, la barra y los cuadritos de color se ven legibles y del tamaño adecuado.

## T09. Flecha

- [x] La barra muestra **Flecha** después de **Círculo**. (R12)
- [x] Arrastrar con **Flecha** dibuja una línea con punta triangular; la punta queda **donde se soltó el mouse**. (R16)
- [x] Funciona en las cuatro direcciones (derecha, izquierda, arriba, abajo) y en diagonal. (R16)
- [x] En una captura de pantalla completa (que se ve reducida), la punta coincide con el cursor al soltar.
- [x] La línea tiene el mismo grosor siempre y no asoma por delante de la punta; no se ve una rendija entre línea y punta. (R16)
- [x] Una flecha corta tiene la punta proporcionada (más pequeña), no un triángulo enorme.
- [x] Respeta el color elegido; `Ctrl + Z` y `Ctrl + Y` funcionan con flechas. (R13, R18, R19)
- [x] Un clic sin arrastrar no crea nada.
