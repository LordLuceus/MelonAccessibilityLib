using System;

namespace MelonAccessibilityLib
{
    /// <summary>
    /// High-level speech output manager.
    /// Handles formatting, duplicate prevention, and repeat functionality.
    /// </summary>
    public static class SpeechManager
    {
        // Repeat functionality
        private static string _currentSpeaker = "";
        private static string _currentText = "";
        private static int _currentType = TextType.Dialogue;

        // Duplicate prevention
        private static string _lastOutputMessage = "";
        private static DateTime _lastOutputTime = DateTime.MinValue;

        /// <summary>
        /// Time window in seconds during which duplicate messages are suppressed.
        /// Default is 0.5 seconds.
        /// </summary>
        public static double DuplicateWindowSeconds { get; set; } = 0.5;

        /// <summary>
        /// Whether to log all speech output. Default is true.
        /// </summary>
        public static bool EnableLogging { get; set; } = true;

        /// <summary>
        /// Initialize the speech system.
        /// </summary>
        /// <returns>True if initialization succeeded.</returns>
        public static bool Initialize()
        {
            return UniversalSpeechWrapper.Initialize();
        }

        /// <summary>
        /// Output text with optional speaker name. Handles formatting, duplicate prevention, and repeat storage.
        /// </summary>
        /// <param name="speaker">The speaker name (can be null or empty)</param>
        /// <param name="text">The text to speak</param>
        /// <param name="textType">The type of text being spoken (use TextType constants or custom values)</param>
        public static void Output(string speaker, string text, int textType = TextType.Dialogue)
        {
            if (Net35Extensions.IsNullOrWhiteSpace(text))
                return;

            string formattedText = FormatText(speaker, text, textType);

            // Duplicate prevention - skip if same text within window
            DateTime now = DateTime.UtcNow;
            if (
                formattedText == _lastOutputMessage
                && (now - _lastOutputTime).TotalSeconds < DuplicateWindowSeconds
            )
            {
                return;
            }

            _lastOutputMessage = formattedText;
            _lastOutputTime = now;

            // Store for repeat functionality (only for narrative content)
            if (ShouldStoreForRepeat(textType))
            {
                _currentSpeaker = speaker ?? "";
                _currentText = text;
                _currentType = textType;
            }

            // Output via speech
            UniversalSpeechWrapper.Speak(formattedText);

            if (EnableLogging)
            {
                string typeName =
                    TextTypeNames != null && TextTypeNames.ContainsKey(textType)
                        ? TextTypeNames[textType]
                        : textType.ToString();
                AccessibilityLog.Msg($"[{typeName}] {formattedText}");
            }
        }

        /// <summary>
        /// Announce text without a speaker name.
        /// </summary>
        /// <param name="text">The text to announce</param>
        /// <param name="textType">The type of text (use TextType constants or custom values)</param>
        public static void Announce(string text, int textType = TextType.System)
        {
            Output(null, text, textType);
        }

        /// <summary>
        /// Repeat the last dialogue or narrator text.
        /// </summary>
        public static void RepeatLast()
        {
            if (!Net35Extensions.IsNullOrWhiteSpace(_currentText))
            {
                string formattedText = FormatText(_currentSpeaker, _currentText, _currentType);
                UniversalSpeechWrapper.Speak(formattedText);

                if (EnableLogging)
                {
                    AccessibilityLog.Msg($"Repeating: '{formattedText}'");
                }
            }
            else
            {
                AccessibilityLog.Msg("Nothing to repeat");
            }
        }

        /// <summary>
        /// Stop any currently playing speech.
        /// </summary>
        public static void Stop()
        {
            UniversalSpeechWrapper.Stop();
        }

        /// <summary>
        /// Clear the stored repeat text.
        /// </summary>
        public static void ClearRepeatBuffer()
        {
            _currentSpeaker = "";
            _currentText = "";
            _currentType = TextType.Dialogue;
        }

        /// <summary>
        /// Optional dictionary mapping text type IDs to names for logging.
        /// Set this to provide readable names for custom text types.
        /// </summary>
        public static System.Collections.Generic.Dictionary<int, string> TextTypeNames { get; set; }

        /// <summary>
        /// Custom predicate to determine if a text type should be stored for repeat.
        /// If null, defaults to storing Dialogue and Narrator types.
        /// </summary>
        public static Func<int, bool> ShouldStoreForRepeatPredicate { get; set; }

        private static bool ShouldStoreForRepeat(int textType)
        {
            if (ShouldStoreForRepeatPredicate != null)
                return ShouldStoreForRepeatPredicate(textType);

            // Default: store dialogue and narrator text
            return textType == TextType.Dialogue || textType == TextType.Narrator;
        }

        /// <summary>
        /// Custom formatter for text output. If null, uses default formatting.
        /// Parameters: speaker, text, textType. Returns formatted string.
        /// </summary>
        public static Func<string, string, int, string> FormatTextOverride { get; set; }

        private static string FormatText(string speaker, string text, int textType)
        {
            text = TextCleaner.Clean(text);

            if (FormatTextOverride != null)
                return FormatTextOverride(speaker, text, textType);

            // Default: only Dialogue type gets speaker prefix
            if (textType == TextType.Dialogue && !Net35Extensions.IsNullOrWhiteSpace(speaker))
                return $"{speaker}: {text}";

            return text;
        }
    }

    /// <summary>
    /// Base text type constants. Define custom types starting from CustomBase (100).
    /// </summary>
    public static class TextType
    {
        /// <summary>Character dialogue with speaker name</summary>
        public const int Dialogue = 0;

        /// <summary>Narrator or descriptive text</summary>
        public const int Narrator = 1;

        /// <summary>Menu item text</summary>
        public const int Menu = 2;

        /// <summary>Menu choice or selection</summary>
        public const int MenuChoice = 3;

        /// <summary>System message or notification</summary>
        public const int System = 4;

        /// <summary>
        /// Base value for custom text types. Define your own types starting from this value.
        /// Example: public const int MyCustomType = TextType.CustomBase + 1;
        /// </summary>
        public const int CustomBase = 100;
    }
}
