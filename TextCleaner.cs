using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MelonAccessibilityLib
{
    /// <summary>
    /// Utility class to clean text for screen reader output.
    /// Removes Unity rich text tags, normalizes whitespace, and handles escape sequences.
    /// Consumers can extend cleaning behavior via <see cref="AddReplacement"/> and <see cref="AddRegexReplacement(string, string, RegexOptions)"/>.
    /// </summary>
    public static class TextCleaner
    {
        private static readonly object _lock = new object();
        private static readonly List<KeyValuePair<string, string>> _customReplacements = new List<KeyValuePair<string, string>>();
        private static readonly List<KeyValuePair<Regex, string>> _customRegexReplacements = new List<KeyValuePair<Regex, string>>();

        // Unity rich text tags
        private static readonly Regex ColorTagRegex = new Regex(
            @"<color[^>]*>|</color>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex SizeTagRegex = new Regex(
            @"<size[^>]*>|</size>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex BoldTagRegex = new Regex(
            @"<b>|</b>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex ItalicTagRegex = new Regex(
            @"<i>|</i>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex MaterialTagRegex = new Regex(
            @"<material[^>]*>|</material>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex QuadTagRegex = new Regex(
            @"<quad[^>]*>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        // Generic tag cleaner for any remaining tags
        private static readonly Regex GenericTagRegex = new Regex(
            @"<[^>]+>",
            RegexOptions.Compiled
        );

        // Whitespace normalization
        private static readonly Regex MultipleSpacesRegex = new Regex(
            @"\s+",
            RegexOptions.Compiled
        );

        /// <summary>
        /// Adds a custom string replacement that will be applied during text cleaning.
        /// Replacements are applied in the order they are added, after Unity rich text tags are removed.
        /// </summary>
        /// <param name="find">The string to find.</param>
        /// <param name="replace">The string to replace it with.</param>
        public static void AddReplacement(string find, string replace)
        {
            if (string.IsNullOrEmpty(find))
                return;

            lock (_lock)
            {
                _customReplacements.Add(new KeyValuePair<string, string>(find, replace ?? string.Empty));
            }
        }

        /// <summary>
        /// Adds a custom regex replacement that will be applied during text cleaning.
        /// Regex replacements are applied in the order they are added, after string replacements.
        /// </summary>
        /// <param name="pattern">The regex pattern to match.</param>
        /// <param name="replace">The replacement string (supports regex substitutions like $1).</param>
        /// <param name="options">Optional regex options. Defaults to <see cref="RegexOptions.None"/>.</param>
        public static void AddRegexReplacement(string pattern, string replace, RegexOptions options = RegexOptions.None)
        {
            if (string.IsNullOrEmpty(pattern))
                return;

            lock (_lock)
            {
                _customRegexReplacements.Add(new KeyValuePair<Regex, string>(
                    new Regex(pattern, options),
                    replace ?? string.Empty
                ));
            }
        }

        /// <summary>
        /// Adds a custom regex replacement that will be applied during text cleaning.
        /// Regex replacements are applied in the order they are added, after string replacements.
        /// </summary>
        /// <param name="regex">The compiled regex to use for matching.</param>
        /// <param name="replace">The replacement string (supports regex substitutions like $1).</param>
        public static void AddRegexReplacement(Regex regex, string replace)
        {
            if (regex == null)
                return;

            lock (_lock)
            {
                _customRegexReplacements.Add(new KeyValuePair<Regex, string>(regex, replace ?? string.Empty));
            }
        }

        /// <summary>
        /// Removes all custom string replacements (not regex replacements).
        /// </summary>
        public static void ClearReplacements()
        {
            lock (_lock)
            {
                _customReplacements.Clear();
            }
        }

        /// <summary>
        /// Removes all custom regex replacements (not string replacements).
        /// </summary>
        public static void ClearRegexReplacements()
        {
            lock (_lock)
            {
                _customRegexReplacements.Clear();
            }
        }

        /// <summary>
        /// Removes all custom replacements (both string and regex).
        /// </summary>
        public static void ClearAllCustomReplacements()
        {
            lock (_lock)
            {
                _customReplacements.Clear();
                _customRegexReplacements.Clear();
            }
        }

        /// <summary>
        /// Clean text by removing Unity rich text tags and normalizing whitespace.
        /// Custom replacements registered via <see cref="AddReplacement"/> and <see cref="AddRegexReplacement(string, string, RegexOptions)"/>
        /// are applied after tag removal and before whitespace normalization.
        /// </summary>
        public static string Clean(string input)
        {
            if (Net35Extensions.IsNullOrWhiteSpace(input))
                return string.Empty;

            string cleaned = input;

            // Remove Unity rich text tags
            cleaned = ColorTagRegex.Replace(cleaned, "");
            cleaned = SizeTagRegex.Replace(cleaned, "");
            cleaned = BoldTagRegex.Replace(cleaned, "");
            cleaned = ItalicTagRegex.Replace(cleaned, "");
            cleaned = MaterialTagRegex.Replace(cleaned, "");
            cleaned = QuadTagRegex.Replace(cleaned, "");

            // Remove any remaining HTML-style tags
            cleaned = GenericTagRegex.Replace(cleaned, "");

            // Handle escape sequences
            cleaned = UnescapeText(cleaned);

            // Apply custom replacements
            cleaned = ApplyCustomReplacements(cleaned);

            // Normalize whitespace
            cleaned = MultipleSpacesRegex.Replace(cleaned, " ");

            return cleaned.Trim();
        }

        private static string UnescapeText(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            StringBuilder sb = new StringBuilder(input);

            sb.Replace("\\n", "\n");
            sb.Replace("\\r", "\r");
            sb.Replace("\\t", "\t");
            sb.Replace("\\\"", "\"");
            sb.Replace("\\'", "'");
            sb.Replace("\\\\", "\\");

            return sb.ToString();
        }

        private static string ApplyCustomReplacements(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string result = input;

            lock (_lock)
            {
                // Apply string replacements first
                foreach (var replacement in _customReplacements)
                {
                    result = result.Replace(replacement.Key, replacement.Value);
                }

                // Apply regex replacements
                foreach (var replacement in _customRegexReplacements)
                {
                    result = replacement.Key.Replace(result, replacement.Value);
                }
            }

            return result;
        }

        /// <summary>
        /// Combine multiple lines into a single string, cleaning each line.
        /// </summary>
        public static string CombineLines(params string[] lines)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var line in lines)
            {
                if (!Net35Extensions.IsNullOrWhiteSpace(line))
                {
                    if (sb.Length > 0)
                        sb.Append(" ");
                    sb.Append(Clean(line));
                }
            }
            return sb.ToString();
        }
    }
}
