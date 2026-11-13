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

## T10. Resaltador

- [x] La barra muestra **Resaltador** después de **Flecha**. (R12)
- [x] Elegir amarillo y resaltar una línea de texto de la captura: el trazo es semitransparente y **el texto se sigue leyendo**. (R17)
- [x] El trazo sigue el mouse (trazo libre), con puntas redondeadas, mientras se arrastra. (R17)
- [x] Hacer un trazo que se cruce consigo mismo (por ejemplo, un "8"): **no** quedan manchas más oscuras en los cruces; el tono es igual en todo el trazo. (R17)
- [x] Dos resaltados distintos que se superponen sí se ven más oscuros donde se cruzan (son dos anotaciones).
- [x] En una captura de pantalla completa (reducida), el trazo pasa exactamente por donde pasó el mouse.
- [x] Un trazo largo, de lado a lado de la pantalla, se dibuja fluido.
- [x] Respeta el color elegido; `Ctrl + Z` y `Ctrl + Y` funcionan con resaltados. (R13, R18, R19)
- [x] Un clic sin arrastrar no crea nada.

## T11. Texto

- [x] La barra muestra **Texto** después de **Resaltador**; con esa herramienta el cursor del mouse es una "I". (R12)
- [x] Clic dentro de la captura: aparece un cursor del color elegido; al escribir **"bug"** se ve el texto ahí mismo. (R14)
- [x] `Enter` confirma el texto. (R14)
- [x] Escribir otro texto y hacer **clic fuera** (en otra parte de la captura): se confirma, y ese clic no empieza otro texto. (R14)
- [x] Escribir y presionar `Esc`: se borra **solo ese texto**; la captura y las demás anotaciones siguen ahí. (R14)
- [x] `Retroceso` borra la última letra.
- [x] Acentos y ñ funcionan: escribir "Aquí falla, ñandú".
- [x] Cambiar de herramienta mientras se escribe confirma el texto; cambiar de color mientras se escribe cambia el color del texto.
- [x] Clic con la herramienta Texto y `Enter` sin escribir nada: no se crea ninguna anotación (`Ctrl + Z` no deshace nada invisible).
- [x] El texto es de una sola línea, en negrita, y se lee bien sobre la captura; en una captura de pantalla completa (reducida) se ve proporcional y en el lugar del clic. (R14)
- [x] `Ctrl + Z` y `Ctrl + Y` funcionan con textos ya confirmados; **mientras se escribe** no deshacen nada. (R18, R19)
- [x] `Ctrl + C` mientras se escribe: el texto se confirma primero (no se pierde) y luego se copia. *(La imagen copiada lleva anotaciones a partir de T12.)*

## T12. Imagen final con anotaciones

- [x] Dibujar **una anotación de cada tipo** (rectángulo, círculo, flecha, resaltador, texto), `Ctrl + C` y pegar en **Slack**: se ven igual que en el editor y en el mismo lugar. (R21)
- [x] Lo mismo pegando en **GitHub** (comentario de un issue en el navegador). (R21)
- [x] Captura de **pantalla completa** (en el editor se ve reducida): en la imagen pegada, las anotaciones quedan exactamente donde se pusieron y la imagen tiene la resolución completa del monitor.
- [x] El texto se lee bien en la imagen pegada; el resaltador deja ver lo de abajo.
- [x] `Ctrl + C` mientras se escribe un texto: el texto aparece en la imagen pegada.
- [x] Deshacer una anotación y copiar: la anotación deshecha **no** aparece en la imagen pegada.
- [x] Una captura sin anotaciones se copia y pega igual que antes.

## T13. Cerrar y descartar

- [x] **Sin anotaciones + `Esc`:** el editor se cierra sin preguntar. (R27)
- [x] **Sin anotaciones + X de la ventana:** se cierra sin preguntar. (R27)
- [x] **Con anotaciones + `Esc`:** pregunta "¿Descartar la captura?" con **No** marcado; `Enter` (No) deja todo igual; **Sí** cierra. (R27)
- [x] **Con anotaciones + X de la ventana:** pregunta igual; **No** deja todo igual; **Sí** cierra. (R27)
- [x] Deshacer todas las anotaciones con `Ctrl + Z` y presionar `Esc`: cierra sin preguntar.
- [x] Escribiendo un texto, `Esc` borra solo ese texto y el editor sigue abierto; un segundo `Esc` ya pide cerrar.
- [x] Escribiendo un texto (sin confirmar) y cerrando con la X: pregunta antes de cerrar.
- [x] Con anotaciones, `Ctrl + C`: copia y cierra **sin** preguntar.
- [x] **Atajo con el editor abierto, detrás de otra ventana:** el editor pasa al frente y no aparece la selección. (R10)
- [x] **Atajo con el editor minimizado:** el editor se restaura (maximizado si lo estaba) y pasa al frente. (R10)
- [x] El menú **Capturar** con el editor abierto hace lo mismo que el atajo. (R10)
- [x] Después de cerrar el editor, el atajo vuelve a abrir la selección.

## T14. Guardar en archivo

- [x] Con anotaciones, `Ctrl + S`: aparece la notificación "Guardado como captura_AAAA-MM-DD_HH-MM-SS.png" y el editor se cierra sin preguntar. (R23, R26)
- [x] El archivo está en `Imágenes\Capturas`, con ese nombre, y al abrirlo tiene las anotaciones. (R23, R24)
- [x] El portapapeles **no** cambió (lo que había copiado antes sigue ahí). (R23)
- [x] El archivo es PNG y se ve igual de nítido que la captura. (NF5)
- [x] Guardar dos capturas muy rápido (en el mismo segundo): la segunda queda con `_2`, ninguna se pisa. (R24)
- [x] Borrar la carpeta `Capturas` y guardar: se vuelve a crear y el archivo queda ahí. (R25)
- [x] `Ctrl + S` mientras se escribe un texto: el texto aparece en el archivo.
- [x] Con el portapapeles bloqueado (bloque de PowerShell de T06), `Ctrl + C`: el aviso ahora sugiere también `Ctrl + S`, y `Ctrl + S` guarda la captura. (R22)
- [x] *(Opcional; ya lo cubre una prueba automática)* **Carpeta sin permisos:** ejecutar el primer bloque de abajo (quita el permiso de escribir en `Capturas`), `Ctrl + S`: aparece "No hay permiso para escribir…" y el editor sigue abierto. Después, **ejecutar el segundo bloque** para devolver el permiso. (R25)

```powershell
# Quitar temporalmente el permiso de escribir en Capturas
$c = Join-Path ([Environment]::GetFolderPath('MyPictures')) 'Capturas'
New-Item -ItemType Directory -Force $c | Out-Null
icacls $c /deny "$($env:USERNAME):(WD)"
```

```powershell
# Devolver el permiso (¡no olvidar!)
$c = Join-Path ([Environment]::GetFolderPath('MyPictures')) 'Capturas'
icacls $c /remove:d $env:USERNAME
```

## T15. Configuración persistente

Archivo: `%APPDATA%\CapturaTool\settings.json`. Para abrirlo: `Win + R` → `%APPDATA%\CapturaTool` → Enter.

- [x] Sin `settings.json`, la app arranca y `Ctrl + S` guarda en `Imágenes\Capturas`. (R29)
- [x] Crear `settings.json` con otra carpeta (ejemplo abajo), salir con **Salir**, volver a abrir: `Ctrl + S` guarda en esa carpeta. (R31)
- [x] Escribir texto sin sentido en `settings.json` y volver a abrir la app: arranca normal, guarda en `Imágenes\Capturas` y el log del día tiene "No se pudo leer la configuración". (R31)
- [x] Al terminar, borrar `settings.json` (o dejarlo con la carpeta que quieras).

```json
{
  "saveFolder": "C:\Users\<usuario>\Desktop\PruebaCapturas",
  "startWithWindows": true
}
```

## T16. Ventana de configuración + arranque con Windows

> Desde que abras esta versión, Windows inicia sola la versión de desarrollo al entrar a tu sesión (la casilla viene marcada). Para comprobar la entrada del registro:
>
> ```powershell
> reg query "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v CapturaTool
> ```

- [x] Menú del ícono → **Configuración**: se abre la ventana con la carpeta actual y la casilla "Iniciar con Windows" marcada. (R28, R30)
- [x] Con la ventana abierta, **Configuración** otra vez: no se abre otra, la misma pasa al frente.
- [x] **Cambiar…** abre el selector de carpetas de Windows; elegir otra carpeta → **Guardar** → `Ctrl + S` guarda en la carpeta nueva. (R28)
- [x] Con un editor ya abierto, cambiar la carpeta y después `Ctrl + S` en ese editor: guarda en la carpeta nueva. (R28)
- [x] Salir y volver a abrir la app: la carpeta elegida se conserva. (R31)
- [x] Cambiar la carpeta y presionar **Cancelar** (o `Esc`): no cambia nada.
- [x] Desmarcar "Iniciar con Windows" → **Guardar**: `reg query` dice que no existe la entrada; volver a marcarla → la entrada aparece con la ruta del .exe entre comillas. (R30)
- [x] Con la casilla marcada, **cerrar sesión o reiniciar Windows**: CapturaTool arranca solo (ícono en la bandeja, `Ctrl + Alt + S` funciona). (R1)
- [x] **Mover el .exe:** salir de la app, copiar la carpeta `src\CapturaTool\bin\Debug\net10.0-windows\win-x64` a otro lugar (por ejemplo el Escritorio) y abrir `CapturaTool.exe` desde la copia: `reg query` muestra la ruta nueva. Después salir, abrir de nuevo la versión normal (vuelve a la ruta original) y borrar la copia. (R1)

## T17. Rendimiento y memoria (mediciones)

**Fecha:** 2026-11-12 · **Build:** Release publicado (ReadyToRun) · **Equipo:** el mismo del hito · **Método:** script que simula el atajo y el mouse; en cada captura dibuja rectángulo, círculo, flecha, resaltador y texto, deshace, rehace y copia con `Ctrl + C`. Se combinan pantalla completa y zonas de 300 × 200, 600 × 400 y 1200 × 700. Tiempos de las líneas `Medición:` del log; memoria del proceso por script.

**Primera medición (editor completo, sin cambios):**

| Meta | Resultado | ¿Cumple? |
|---|---|---|
| **NF1:** selección en menos de 250 ms | 138–199 ms | ✅ |
| **NF2:** editor en menos de 300 ms | 197–370 ms; 4 de 20 por encima, todas de pantalla completa | ❌ |
| **NF3:** CPU en reposo | 0 ms de CPU en 60 s | ✅ |
| **NF3:** memoria | Al arrancar 7,7 MB; tras 20 capturas **103 MB, sin bajar** después de 90 s | ❌ |

**Diagnóstico de memoria:** la memoria crecía solo al copiar (las capturas descartadas no la subían) y no era memoria de .NET. En una prueba aislada, `Clipboard.SetDataObject` de WinForms con un `Bitmap` dejaba la imagen entera sin liberar en cada copia (+8 MB y +1 objeto GDI por copia de 1920 × 1080, sin límite).

**Diagnóstico de NF2:** el paso más lento era crear la barra de herramientas (50–160 ms), que se reacomodaba con cada botón agregado. Además, la caché del lienzo se dibujaba dos veces de más en tamaños provisorios (~20 ms en pantalla completa).

**Cambios (aprobados por el usuario):**
- El portapapeles se escribe con las funciones nativas de Windows (`NativeClipboard`), con los mismos dos formatos, reintentos y verificación (architecture.md v1.1).
- Al cerrar cada editor, una limpieza de .NET (`GCCollectionMode.Aggressive`, 1–3 ms) devuelve la memoria a Windows.
- La barra de herramientas se acomoda una sola vez (`SuspendLayout`/`ResumeLayout`) y la caché del lienzo se dibuja al pintar, no en cada cambio de tamaño.

**Medición final (mismo método, 20 capturas):**

| Meta | Resultado | ¿Cumple? |
|---|---|---|
| **NF1:** selección en menos de 250 ms | 130–221 ms | ✅ |
| **NF2:** editor en menos de 300 ms | Zonas 159–260 ms · pantalla completa 260–324 ms (1 de 20 por encima: 324) | ⚠️ → ✅ con la meta nueva de 350 ms |
| **NF3:** CPU en reposo | 0 ms en 60 s al arrancar y 0 ms en 30 s tras usarlo | ✅ |
| **NF3:** memoria | Al arrancar 9,6 MB; tras 20 capturas **24,3 MB**, igual a los 30 s y a los 90 s. Objetos GDI 31 → 34 | ✅ |

**Decisión (2026-11-12):** el usuario aprobó ajustar **NF2 a menos de 350 ms** (spec v1.3). En pantalla completa, unos 80 ms son crear y maximizar la ventana y unos 30 ms reducir la captura; bajarlo más exigía crear el editor por adelantado. Con la meta nueva, todas las mediciones cumplen.

**Pasada final con la versión nueva** (lo que cambió en T17 y lo que no se puede medir con el script):

- [x] `Ctrl + C` con anotaciones y pegar en **Slack**: se ve la imagen con las anotaciones. (R21)
- [x] Pegar en **GitHub** (un comentario en el navegador): sube la imagen. (R21)
- [x] Pegar en **Paint** (`Ctrl + V`): la imagen con las anotaciones, nítida y del tamaño correcto. (R21)
- [x] Con el portapapeles bloqueado (bloque de PowerShell de T06), `Ctrl + C`: el aviso aparece en ~1 s, el editor sigue abierto y `Ctrl + S` guarda. (R22)
- [x] Flujo completo: `Ctrl + Alt + S` → zona → anotar → `Ctrl + C` → pegar; otra captura → `Ctrl + S` → el archivo está en la carpeta configurada.
- [x] Con un monitor al **150 %**: captura nítida y editor legible. (NF4)
- [x] Menú "Provocar error (prueba)": aparece el aviso y el error queda en el log. (R32)
- [x] Después de varias capturas, el Administrador de tareas muestra CapturaTool por debajo de 50 MB. (NF3)

## T20. Logo en toda la app

> Si el Explorador sigue mostrando el ícono viejo del `.exe`, es su caché de íconos. Se refresca cerrando sesión o reiniciando; también al copiar el `.exe` a otra carpeta.

- [x] **Bandeja:** el ícono de CapturaTool es el logo (azul, esquinas blancas, flecha amarilla) y se reconoce sin pasar el mouse por encima. (R33, criterio 1 del Ciclo 2)
- [x] **Notificación:** al copiar o guardar, el aviso ya no muestra la "i" azul en el cuerpo. Los avisos de error mantienen la X roja.
  > El ícono del **encabezado** del aviso lo guarda Windows la primera vez que un `.exe` (según su ruta) muestra un aviso, y no lo actualiza después. La versión de desarrollo (`bin\Debug`) avisó antes de tener logo, así que ahí sigue el ícono genérico. Comprobado con un programa de prueba: el primer aviso fija el ícono; tener el ícono en la bandeja sin avisar no lo fija. El .exe publicado nunca mostró un aviso, así que el logo en el encabezado se comprueba en **T21**.
- [x] **Editor:** el logo aparece en la barra de título y en su botón de la barra de tareas.
- [x] **Configuración:** el logo aparece en la barra de título.
- [x] **Alt + Tab** con el editor abierto: se ve el logo.
- [x] **Explorador:** `CapturaTool.exe` muestra el logo en vista de íconos medianos y grandes.
- [x] Todo lo anterior al **150 %** de escalado: el logo se ve nítido, sin bordes borrosos. (Criterio 2 del Ciclo 2)
- [x] Con la barra de tareas en **tema claro** y en **tema oscuro** (Configuración → Personalización → Colores): el logo se distingue en los dos. (Criterio 3 del Ciclo 2)
