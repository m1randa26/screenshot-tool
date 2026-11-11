# ADR 0003: Distribución: ejecutable dependiente del runtime

**Fecha:** 2026-11-10 · **Estado:** ✅ Aceptada · **Depende de:** [ADR 0001](0001-tecnologia.md)

## Contexto

NF7 pide un solo ejecutable, sin instalador. .NET permite publicar de dos formas:

- **Dependiente del runtime:** un .exe pequeño (menos de 1 MB) que necesita .NET 10 Desktop Runtime instalado.
- **Autocontenido:** un .exe grande (~60–150 MB) que incluye el runtime.

El único usuario previsto ya tiene .NET 10 instalado.

## Decisión

Se publica como **un solo archivo dependiente del runtime** (`PublishSingleFile=true`, `SelfContained=false`).

## Consecuencias

**Positivas**
- Ejecutable mínimo.
- Las actualizaciones de seguridad de .NET llegan a través de Windows Update, sin volver a publicar.

**Negativas / costos**
- No funciona en una PC sin .NET 10 Desktop Runtime.

**Reversibilidad: alta.** Para compartirlo con alguien sin .NET basta con cambiar `SelfContained=true` en la configuración de publicación, sin tocar el código.
