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
