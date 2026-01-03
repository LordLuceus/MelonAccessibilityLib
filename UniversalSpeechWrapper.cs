using System;
using System.Runtime.InteropServices;

namespace MelonAccessibilityLib
{
    /// <summary>
    /// Low-level P/Invoke wrapper for the UniversalSpeech library.
    /// Provides direct access to screen reader output with SAPI fallback.
    ///
    /// Requires UniversalSpeech.dll (32-bit) to be in the game directory.
    /// Download from: https://github.com/accessibleapps/UniversalSpeech
    /// </summary>
    public static class UniversalSpeechWrapper
    {
        private const string DLL_NAME = "UniversalSpeech.dll";

        // P/Invoke declarations for UniversalSpeech
        [DllImport(
            DLL_NAME,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode
        )]
        private static extern int speechSay(
            [MarshalAs(UnmanagedType.LPWStr)] string str,
            int interrupt
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int speechStop();

        [DllImport(
            DLL_NAME,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode
        )]
        private static extern int brailleDisplay(
            [MarshalAs(UnmanagedType.LPWStr)] string str
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int speechSetValue(int what, int value);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int speechGetValue(int what);

        // Constants from UniversalSpeech.h
        private const int SP_ENABLE_NATIVE_SPEECH = 0xFFFF;
        private const int SP_DETECT_SCREEN_READER = 0;

        private static bool _initialized = false;
        private static bool _dllAvailable = true;

        /// <summary>
        /// Whether the speech system has been initialized successfully.
        /// </summary>
        public static bool IsInitialized => _initialized;

        /// <summary>
        /// Initialize the speech system. Enables SAPI fallback if no screen reader is available.
        /// </summary>
        /// <returns>True if initialization succeeded, false otherwise.</returns>
        public static bool Initialize()
        {
            if (_initialized)
                return true;

            if (!_dllAvailable)
                return false;

            try
            {
                // Enable native speech engines (SAPI) as fallback
                speechSetValue(SP_ENABLE_NATIVE_SPEECH, 1);
                _initialized = true;
                AccessibilityLog.Msg("UniversalSpeech initialized");
                return true;
            }
            catch (DllNotFoundException ex)
            {
                _dllAvailable = false;
                AccessibilityLog.Error($"UniversalSpeech.dll not found: {ex.Message}");
                AccessibilityLog.Error("Ensure UniversalSpeech.dll is in the game directory");
                return false;
            }
            catch (Exception ex)
            {
                AccessibilityLog.Error($"Failed to initialize UniversalSpeech: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Speak the given text directly.
        /// </summary>
        /// <param name="text">The text to speak</param>
        /// <param name="interrupt">Whether to interrupt current speech</param>
        public static void Speak(string text, bool interrupt = false)
        {
            if (!_initialized || !_dllAvailable || Net35Extensions.IsNullOrWhiteSpace(text))
                return;

            try
            {
                speechSay(text, interrupt ? 1 : 0);
            }
            catch (DllNotFoundException)
            {
                _dllAvailable = false;
            }
            catch (Exception ex)
            {
                AccessibilityLog.Error($"Speech error: {ex.Message}");
            }
        }

        /// <summary>
        /// Display the given text on a braille display via the current screen reader.
        /// </summary>
        /// <param name="text">The text to display on braille</param>
        public static void DisplayBraille(string text)
        {
            if (!_initialized || !_dllAvailable || Net35Extensions.IsNullOrWhiteSpace(text))
                return;

            try
            {
                brailleDisplay(text);
            }
            catch (DllNotFoundException)
            {
                _dllAvailable = false;
            }
            catch (Exception ex)
            {
                AccessibilityLog.Error($"Braille display error: {ex.Message}");
            }
        }

        /// <summary>
        /// Stop any currently playing speech.
        /// </summary>
        public static void Stop()
        {
            if (!_dllAvailable)
                return;

            try
            {
                speechStop();
            }
            catch (DllNotFoundException)
            {
                _dllAvailable = false;
            }
            catch (Exception ex)
            {
                AccessibilityLog.Error($"Failed to stop speech: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if a screen reader is currently active (not using SAPI fallback).
        /// </summary>
        /// <returns>True if a screen reader is detected, false otherwise.</returns>
        public static bool IsScreenReaderActive()
        {
            if (!_initialized || !_dllAvailable)
                return false;

            try
            {
                return speechGetValue(SP_DETECT_SCREEN_READER) != 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
