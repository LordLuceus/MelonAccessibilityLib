namespace MelonAccessibilityLib
{
    /// <summary>
    /// Polyfills for .NET 3.5 compatibility.
    /// These methods are available in later .NET versions but not in net35.
    /// </summary>
    public static class Net35Extensions
    {
        /// <summary>
        /// Indicates whether a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value == null)
                return true;

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                    return false;
            }
            return true;
        }
    }
}
