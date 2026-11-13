# Semana de prueba (T18)

**Inicio:** jueves 2026-11-12 · **Fin:** jueves 2026-11-19 · **Criterio 5 del brief:** usar CapturaTool una semana completa en lugar de la herramienta de Windows, sin regresar a ella.

## Instalación

- **Ejecutable:** `%LOCALAPPDATA%\Programs\CapturaTool\CapturaTool.exe` (publicado en Release, ReadyToRun, versión del commit `286c9f8`).
- **Arranque con Windows:** entrada `CapturaTool` en `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`, apuntando a ese .exe. Para comprobarla:

  ```powershell
  reg query "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v CapturaTool
  ```

- **Configuración:** `%APPDATA%\CapturaTool\settings.json` · **Log:** `%APPDATA%\CapturaTool\logs\` (un archivo por día).
- **Ojo:** si durante la semana se abre la versión de desarrollo (`dotnet run`), esa pasa a ser la que arranca con Windows. Para volver, abrir de nuevo el .exe publicado.

## Ajustes de Windows (los hace el usuario)

- [ ] *(Opcional)* Impr Pant deja de abrir la herramienta de Windows: Configuración → Accesibilidad → Teclado → desactivar "Usar el botón Imprimir pantalla para abrir la captura de pantalla".
- [ ] *(Recomendado)* Ícono siempre visible: Configuración → Personalización → Barra de tareas → Otros iconos de la bandeja del sistema → activar CapturaTool.

## Qué cuenta como fallo

- Volver a la herramienta de Windows (Recortes, `Win + Shift + S`) por cualquier motivo, aunque sea "por costumbre".
- Una copia que no se pega, o que se pega distinta (sin anotaciones, borrosa, recortada).
- Algo que se siente lento (el atajo, el editor, copiar o guardar).
- Un aviso de error, un cierre inesperado, o que el programa no esté al encender la PC.
- Cualquier cosa molesta o confusa, aunque no sea un error.

## Registro

Anotar cada fallo apenas ocurra. Si hubo un aviso de error, revisar el log del día y copiar la línea.

| Fecha | Qué pasó | ¿Volví a la herramienta de Windows? | ¿Quedó en el log? |
|---|---|---|---|
| | | | |

## Al terminar la semana

- [ ] ¿Siete días sin volver a la herramienta de Windows? (criterio 5)
- [ ] Revisar el registro de arriba y los logs de la semana: ¿algún error que no se notó?
- [ ] En el Administrador de tareas, CapturaTool sigue por debajo de 50 MB después de días de uso. (NF3)
- [ ] Decidir qué hacer con cada fallo: corregirlo, dejarlo para una v2 o descartarlo.
- [ ] Si todo está bien: marcar T18 en `plan.md`. 🎯
