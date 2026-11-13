# ADR 0004: Rama Insiders y versión paralela

**Fecha:** 2026-11-13 · **Estado:** ✅ Aceptada · **Depende de:** [ADR 0003](0003-distribucion.md)

## Contexto

La versión de `main` es estable y se usa todos los días (semana de prueba, T18). Se quieren probar funciones nuevas (leer texto de la pantalla, botones del mouse, interfaz moderna) **sin poner en riesgo esa versión**, y poder usarlas en el día a día mientras maduran, como un programa "Insider".

Si las dos versiones se instalan a la vez en la misma PC, hoy chocarían en todo lo que identifica al programa:

- la **instancia única** (mutex): la segunda no arrancaría;
- el **atajo** `Ctrl + Alt + S`: solo uno puede registrarlo;
- la **configuración y el log** en `%APPDATA%\CapturaTool`;
- la entrada de **arranque con Windows** (`CapturaTool` en `HKCU\...\Run`), que cada una reescribiría con su ruta;
- el **nombre y el logo**: no se sabría cuál es cuál en la bandeja.

## Opciones consideradas

| | Una rama con identidad paralela | Una sola versión a la vez | Repositorio aparte |
|---|---|---|---|
| Probar sin perder la estable | ✅ Las dos instaladas | ❌ Hay que cambiar de una a otra | ✅ |
| Código duplicado | Ninguno: el mismo código, otra opción al publicar | Ninguno | Todo |
| Llevar correcciones de una a otra | `git merge` | `git merge` | A mano |
| Complejidad | Baja: un archivo con los datos de identidad | Mínima | Alta |

## Decisión

1. **Ramas:**
   - `main` es la versión **estable**. Solo recibe correcciones urgentes y lo que ya se probó en Insiders.
   - `insiders` es la rama de **pruebas**. Ahí se desarrollan las funciones nuevas, con el mismo flujo de siempre (docs → plan → una tarea a la vez → checklist → commit con aprobación).
2. **Pasar a estable:** al cerrar un ciclo de Insiders, cuando sus funciones llevan al menos **3 días de uso real sin fallos**, se une `insiders` a `main` con un *pull request* en GitHub, con la aprobación del usuario. Después se publica la versión estable.
   - Si una función no está lista, se espera o se saca de `insiders` antes de unir; no se pasan funciones a medias.
3. **Correcciones en `main`:** cada corrección que se haga en `main` se une también a `insiders`, para que las ramas no se separen.
4. **Identidad paralela:** el mismo código se publica de dos formas con la opción `-p:Channel=Insider`. Esa opción cambia solo los datos de identidad, que viven juntos en un único lugar del código:

   | | Estable | Insider |
   |---|---|---|
   | Nombre del `.exe` | `CapturaTool.exe` | `CapturaTool.Insider.exe` |
   | Nombre visible | CapturaTool | CapturaTool Insider |
   | Logo | El de R33 | El mismo con una marca de color propia |
   | Atajo | `Ctrl + Alt + S` | `Ctrl + Alt + Shift + S` |
   | Configuración y log | `%APPDATA%\CapturaTool` | `%APPDATA%\CapturaTool Insider` |
   | Instancia única | La suya | La suya |
   | Arranque con Windows | Entrada `CapturaTool` | Entrada `CapturaTool Insider` |
   | Carpeta de instalación | `%LOCALAPPDATA%\Programs\CapturaTool` | `%LOCALAPPDATA%\Programs\CapturaTool Insider` |

## Consecuencias

**Positivas**
- La versión estable no se toca mientras se prueban cosas nuevas.
- Se pueden comparar las dos versiones, y si la Insider falla, la estable sigue ahí.
- Sin código duplicado: una corrección sirve para las dos.

**Negativas / costos**
- Hay que acordarse de en qué rama se está trabajando y de llevar las correcciones de `main` a `insiders`.
- Con las dos instaladas hay dos íconos en la bandeja y dos atajos que recordar.
- La documentación de `insiders` va adelantada a la de `main` hasta que se unen.

**Reversibilidad: alta.** Dejar de usar Insiders es borrar la rama y desinstalar esa versión; la estable no depende de nada de esto.
