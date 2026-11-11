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
