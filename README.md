# MelonAccessibilityLib

A reusable library for adding screen reader accessibility to Unity games via MelonLoader mods.

## Features

- **UniversalSpeech integration** - P/Invoke wrapper for the UniversalSpeech library with SAPI fallback
- **High-level speech manager** - Duplicate prevention, repeat functionality, speaker formatting
- **Text cleaning** - Strips Unity rich text tags (`<color>`, `<size>`, `<b>`, etc.)
- **Logging abstraction** - Integrate with MelonLoader or any logging system
- **Multi-target support** - Builds for net6.0, net472, and net35 for broad compatibility with various games

## Requirements

- [UniversalSpeech.dll](https://github.com/qtnc/UniversalSpeech) in the game directory
- One of:
  - .NET 6.0 (IL2CPP games with MelonLoader 0.6+)
  - .NET Framework 4.7.2 (Mono games)
  - .NET Framework 3.5 (older Mono games)

## Installation

### Option 1: Project Reference

Add the project to your solution and reference it:

```xml
<ProjectReference Include="..\MelonAccessibilityLib\MelonAccessibilityLib.csproj" />
```

### Option 2: DLL Reference

Build the library and reference the DLL:

```xml
<Reference Include="MelonAccessibilityLib">
  <HintPath>path\to\MelonAccessibilityLib.dll</HintPath>
</Reference>
```

## Quick Start

### 1. Create a Logger Adapter

```csharp
using MelonLoader;
using MelonAccessibilityLib;

public class MelonLoggerAdapter : IAccessibilityLogger
{
    private readonly MelonLogger.Instance _logger;

    public MelonLoggerAdapter(MelonLogger.Instance logger)
    {
        _logger = logger;
    }

    public void Msg(string message) => _logger.Msg(message);
    public void Warning(string message) => _logger.Warning(message);
    public void Error(string message) => _logger.Error(message);
}
```

### 2. Initialize in Your Mod

```csharp
public class MyAccessibilityMod : MelonMod
{
    public override void OnInitializeMelon()
    {
        // Set up logging
        AccessibilityLog.Logger = new MelonLoggerAdapter(LoggerInstance);

        // Initialize speech system
        if (SpeechManager.Initialize())
        {
            LoggerInstance.Msg("Speech system ready");
        }
    }
}
```

### 3. Output Speech

```csharp
// Dialogue with speaker name
SpeechManager.Output("Phoenix", "Hold it!", TextType.Dialogue);
// Output: "Phoenix: Hold it!"

// Narrator text
SpeechManager.Output(null, "The court fell silent.", TextType.Narrator);
// Output: "The court fell silent."

// Menu/system announcements
SpeechManager.Announce("Save complete", TextType.System);

// Repeat last dialogue (bind to a key)
SpeechManager.RepeatLast();
```

## API Reference

### SpeechManager

| Method                            | Description                                            |
| --------------------------------- | ------------------------------------------------------ |
| `Initialize()`                    | Initialize the speech system. Returns true on success. |
| `Output(speaker, text, textType)` | Speak text with optional speaker name.                 |
| `Announce(text, textType)`        | Speak text without a speaker name.                     |
| `RepeatLast()`                    | Repeat the last dialogue/narrator text.                |
| `Stop()`                          | Stop current speech.                                   |
| `ClearRepeatBuffer()`             | Clear stored repeat text.                              |

| Property                        | Description                                           |
| ------------------------------- | ----------------------------------------------------- |
| `DuplicateWindowSeconds`        | Time window for duplicate suppression (default: 0.5s) |
| `EnableLogging`                 | Whether to log speech output (default: true)          |
| `ShouldStoreForRepeatPredicate` | Custom predicate for repeat storage                   |

### TextType Enum

| Value        | Description                                       |
| ------------ | ------------------------------------------------- |
| `Dialogue`   | Character dialogue (formatted as "Speaker: text") |
| `Narrator`   | Narrator/descriptive text                         |
| `Menu`       | Menu item text                                    |
| `MenuChoice` | Menu selection                                    |
| `System`     | System messages                                   |

### UniversalSpeechWrapper

Low-level access to UniversalSpeech:

```csharp
UniversalSpeechWrapper.Initialize();           // Initialize (called by SpeechManager)
UniversalSpeechWrapper.Speak("text", true);    // Speak with interrupt
UniversalSpeechWrapper.Stop();                 // Stop speech
UniversalSpeechWrapper.IsScreenReaderActive(); // Check if screen reader is running
```

### TextCleaner

```csharp
string clean = TextCleaner.Clean("<color=#ff0000>Red text</color>");
// Result: "Red text"

string combined = TextCleaner.CombineLines("Line 1", "<b>Line 2</b>", "Line 3");
// Result: "Line 1 Line 2 Line 3"
```

### AccessibilityLog

```csharp
// Set your logger implementation
AccessibilityLog.Logger = new MelonLoggerAdapter(loggerInstance);

// Or create a simple console logger for testing
AccessibilityLog.Logger = new ConsoleLogger();

public class ConsoleLogger : IAccessibilityLogger
{
    public void Msg(string message) => Console.WriteLine(message);
    public void Warning(string message) => Console.WriteLine($"[WARN] {message}");
    public void Error(string message) => Console.WriteLine($"[ERROR] {message}");
}
```

### .NET 3.5 Compatibility

The library includes `Net35Extensions` with polyfills for methods not available in .NET 3.5:

- `Net35Extensions.IsNullOrWhiteSpace(string)` - Use instead of `string.IsNullOrWhiteSpace`

## UniversalSpeech Setup

1. Download UniversalSpeech from [GitHub](https://github.com/qtnc/UniversalSpeech)
2. Copy `UniversalSpeech.dll` (32-bit or 64-bit depending on the game's architecture) to the game's root directory
3. The library will automatically use any active screen reader, or fall back to Windows SAPI

Supported screen readers:

- NVDA
- JAWS
- Window-Eyes
- System Access
- Supernova
- ZoomText
- SAPI (fallback)

## License

MIT License - see [LICENSE](LICENSE) for details.
