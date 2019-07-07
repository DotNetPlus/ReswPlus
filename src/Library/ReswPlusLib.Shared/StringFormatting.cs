namespace ReswPlusLib
{
    /// <summary>
    /// Helper class used for C++/CX and C++/WinRT using C#/VB string formatting (eg: "Hello {0}")
    /// </summary>
    public static class StringFormatting
    {
#if WINDOWS_UWP
        [Windows.Foundation.Metadata.DefaultOverload]
#endif
        public static string FormatDotNet(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        public static string FormatDotNet(this string format, object arg)
        {
            return string.Format(format, arg);
        }

        public static string FormatDotNet(this string format, object arg1, object arg2)
        {
            return string.Format(format, arg1, arg2);
        }

        public static string FormatDotNet(this string format, object arg1, object arg2, object arg3)
        {
            return string.Format(format, arg1, arg2, arg3);
        }

        public static string FormatDotNet(this string format, object arg1, object arg2, object arg3, object arg4)
        {
            return string.Format(format, arg1, arg2, arg3, arg4);
        }

        public static string FormatDotNet(this string format, object arg1, object arg2, object arg3, object arg4, object arg5)
        {
            return string.Format(format, arg1, arg2, arg3, arg4, arg5);
        }
    }
}
