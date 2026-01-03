# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MelonAccessibilityLib is a C# library that adds screen reader accessibility to Unity games via MelonLoader mods. It provides speech and braille output, text cleaning utilities, and P/Invoke wrappers for UniversalSpeech with SAPI fallback.

## Build Commands

```bash
# Build all target frameworks (net6.0, net472, net35)
dotnet build

# Build specific framework
dotnet build -f net6.0
dotnet build -f net472
dotnet build -f net35

# Release build
dotnet build -c Release

# Create NuGet package
dotnet pack -c Release
```

Build outputs are located at `bin/{Debug|Release}/{net6.0|net472|net35}/MelonAccessibilityLib.dll`.

## Testing

No test framework is currently configured. If adding tests, use standard `dotnet test` commands.

## Architecture

### Component Overview

- **SpeechManager** (`SpeechManager.cs`): High-level static API for speech and braille output with duplicate prevention, repeat functionality, and text formatting
- **UniversalSpeechWrapper** (`UniversalSpeechWrapper.cs`): Low-level P/Invoke wrapper for UniversalSpeech.dll with SAPI fallback; provides `Speak()` and `DisplayBraille()` methods
- **TextCleaner** (`TextCleaner.cs`): Removes Unity rich text tags and normalizes text; supports custom string and regex replacements via `AddReplacement()` and `AddRegexReplacement()`
- **AccessibilityLog/IAccessibilityLogger** (`IAccessibilityLogger.cs`): Logging facade with pluggable logger interface
- **Net35Extensions** (`Net35Extensions.cs`): Polyfills for .NET 3.5 compatibility (e.g., `IsNullOrWhiteSpace`)

### Data Flow

```
Consumer (MelonMod)
    │
    ├─ Sets AccessibilityLog.Logger
    ├─ Calls SpeechManager.Initialize()
    └─ Calls SpeechManager.Output()
          │
          ├─ Duplicate suppression (time-based)
          ├─ TextCleaner.Clean() (strips rich text, applies custom replacements)
          ├─ UniversalSpeechWrapper.Speak() (P/Invoke)
          └─ UniversalSpeechWrapper.DisplayBraille() (if EnableBraille)
```

### Extensibility Points

- **Custom Logger**: Implement `IAccessibilityLogger` interface
- **Text Formatting**: Set `SpeechManager.FormatTextOverride` delegate
- **Repeat Logic**: Set `SpeechManager.ShouldStoreForRepeatPredicate` delegate
- **Custom Text Types**: Use constants starting from `TextType.CustomBase` (100)
- **Text Cleaning**: Use `TextCleaner.AddReplacement()` and `TextCleaner.AddRegexReplacement()` for custom text transformations
- **Braille Control**: Set `SpeechManager.EnableBraille` to toggle braille output (default: true)

## Key Conventions

- All public classes are static (no instance creation)
- Private fields use `_camelCase` prefix
- P/Invoke constants use `ALL_CAPS`
- Single namespace: `MelonAccessibilityLib`
- Comprehensive XML documentation on all public members
- Multi-target build: net35 is limited to C# 7.3 features

## Platform Requirements

- Windows only (P/Invoke and SAPI are Windows-specific)
- UniversalSpeech.dll must be deployed alongside consuming mod
- Supports screen readers: NVDA, JAWS, Window-Eyes, System Access, Supernova, ZoomText
