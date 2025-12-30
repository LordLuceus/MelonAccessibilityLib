namespace MelonAccessibilityLib
{
    /// <summary>
    /// Simple logging interface for accessibility output.
    /// Implement this interface to integrate with your mod's logging system (e.g., MelonLoader).
    /// </summary>
    public interface IAccessibilityLogger
    {
        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void Msg(string message);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void Warning(string message);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void Error(string message);
    }

    /// <summary>
    /// Static logger configuration. Set the Logger property during mod initialization.
    /// </summary>
    public static class AccessibilityLog
    {
        /// <summary>
        /// The logger instance used by the library. Set this during mod initialization.
        /// If null, logging is silently skipped.
        /// </summary>
        public static IAccessibilityLogger Logger { get; set; }

        internal static void Msg(string message)
        {
            Logger?.Msg(message);
        }

        internal static void Warning(string message)
        {
            Logger?.Warning(message);
        }

        internal static void Error(string message)
        {
            Logger?.Error(message);
        }
    }
}
