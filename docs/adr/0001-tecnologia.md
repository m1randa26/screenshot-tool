# ADR 0001: Tecnología base: C# / .NET

**Fecha:** 2026-11-10 · **Estado:** ✅ Aceptada

## Contexto

La [spec v1.0](../spec.md) exige:

- **NF2:** el editor aparece en menos de 300 ms.
- **NF3:** en reposo usa ~0 % de CPU y menos de 50 MB de memoria.
- **NF6:** solo Windows 11 (no hace falta multiplataforma).
- **NF7:** un solo ejecutable, sin instalador.
- Integración con el sistema: atajo global (R4), ícono en la bandeja (R2), portapapeles confiable (R21, R22), captura nítida con escalado de pantalla (NF4) y arranque con Windows (R1).

El usuario conoce Astro, React y TypeScript, pero no C# ni Rust.

## Opciones consideradas

| Opción | Resultado | Motivo |
|---|---|---|
| Electron | ❌ Descartada | Más de 100 MB en reposo: rompe NF3 |
| Python | ❌ Descartada | El .exe empaquetado es pesado y arranca lento (NF2, NF7); soporte de escalado frágil (NF4) |
| C++ nativo | ❌ Descartada | Cumple la spec, pero es demasiado complejo para el tamaño del proyecto y difícil de revisar |
| Tauri + React | ⚖️ Finalista | Aprovecha el conocimiento de React; obliga a usar dos lenguajes, y el portapapeles (el dolor principal) quedaría en Rust |
| **C# / .NET** | ✅ **Elegida** | Encaje nativo con Windows en todo lo crítico; un solo lenguaje |

## Decisión

El programa se construye con **C# sobre .NET 10 (LTS)**, que ya está instalado en la máquina de desarrollo (SDK 10.0.401).

## Consecuencias

**Positivas**
- El atajo global, la bandeja, el portapapeles y el escalado se resuelven con APIs nativas de Windows bien documentadas.
- Un solo lenguaje en todo el proyecto: más fácil de entender y revisar.
- La funcionalidad crítica (copiar al portapapeles) queda en la tecnología con mejor soporte para ello.

**Negativas / costos**
- El usuario tiene que aprender C#. Para compensarlo: código simple, sin patrones avanzados innecesarios, y comentarios donde algo no sea obvio.
- La parte visual no reutiliza lo que ya sabe de React.

**Decisiones que dependen de esta** (próximos ADRs)
- Framework de interfaz (WinForms o WPF).
- Forma de distribuir el ejecutable (NF7).
