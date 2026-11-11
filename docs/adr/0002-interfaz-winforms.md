# ADR 0002: Framework de interfaz: WinForms

**Fecha:** 2026-11-10 · **Estado:** ✅ Aceptada · **Depende de:** [ADR 0001](0001-tecnologia.md)

## Contexto

Con C# / .NET 10 hay dos opciones principales para la interfaz de escritorio: **WinForms** y **WPF**. El componente visual más importante es el editor (R11–R20), que en esencia consiste en *dibujar figuras sobre una imagen*. La spec exige rapidez (NF1, NF2) y poca memoria (NF3). El brief no pide una apariencia sofisticada.

## Opciones consideradas

| | WinForms | WPF |
|---|---|---|
| Memoria y arranque | Más ligero | Más pesado (riesgo para NF3) |
| Dibujar sobre una imagen | Directo (`Graphics` / GDI+) | Más capas de abstracción |
| Aprendizaje | Bajo | Medio/alto (XAML, *bindings*) |
| Apariencia | Sobria | Más pulida |

## Decisión

Se usa **WinForms**.

## Consecuencias

**Positivas**
- Dibujar anotaciones es directo, con poco código.
- Mejores posibilidades de cumplir NF2 y NF3.
- Un solo concepto nuevo que aprender (además de C#).

**Negativas / costos**
- Apariencia menos moderna.
- El suavizado de líneas y texto hay que activarlo de forma explícita (`SmoothingMode`, `TextRenderingHint`).
- El soporte de escalado por monitor (*PerMonitorV2*) se tiene que configurar a mano; es crítico para NF4.
