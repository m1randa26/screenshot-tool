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
