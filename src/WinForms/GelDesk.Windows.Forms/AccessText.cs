using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GelDesk
{
    /// <summary>
    /// Procedures for dealing with text that has embedded 
    /// <see cref="AccessKeyMarker"/> and 
    /// <see cref="WinFormsMarker"/> characters.
    /// </summary>
    public static class AccessText
    {
        public const char AccessKeyMarker = '_';
        public const char WinFormsMarker = '&';

        /// <summary>
        /// Returns the given text with GelDesk <see cref="AccessKeyMarker"/> 
        /// characters ('_') converted to the WinForms marker characters ('&')
        /// that are used for keyboard access.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Convert(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            // Escape the WinFormsMarker.
            string marker;
            text = EscapeWinForms(text, out marker);
            // Find and Replace the AccessKeyMarker with a WinFormsMarker.
            int num = FindAccessKeyMarker(text);
            if (num >= 0 && num < text.Length - 1)
                text = text.Remove(num, 1).Insert(num, marker);
            // Unescape AccessKeyMarker.
            text = Unescape(text);
            return text;
        }

        /// <summary>
        /// Returns the given text with all instances of the WinForms marker 
        /// character ('&') escaped (by doubling them).
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string EscapeWinForms(string text)
        {
            string marker;
            return EscapeWinForms(text, out marker);
        }
        static string EscapeWinForms(string text, out string marker)
        {
            marker = WinFormsMarker.ToString();
            text = text.Replace(marker, marker + marker);
            return text;
        }
        static int FindAccessKeyMarker(string text)
        {
            int length = text.Length;
            int num;
            for (int i = 0; i < length; i = num + 2)
            {
                num = text.IndexOf(AccessText.AccessKeyMarker, i);
                if (num == -1)
                    return -1;
                if (num + 1 < length && text[num + 1] != AccessText.AccessKeyMarker)
                    return num;
            }
            return -1;
        }
        static string RemoveAccessKeyMarker(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            // Remove the first marker that we can find.
            int num = FindAccessKeyMarker(text);
            if (num >= 0 && num < text.Length - 1)
                text = text.Remove(num, 1);
            // Unescape AccessKeyMarker.
            text = Unescape(text);
            return text;
        }

        /// <summary>
        /// Returns the given text with all instances of the 
        /// <see cref="AccessKeyMarker"/> character removed and unescaped.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Strip(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            text = RemoveAccessKeyMarker(text);
            return text;
        }
        static string Unescape(string text)
        {
            string marker = AccessKeyMarker.ToString();
            text = text.Replace(marker + marker, marker);
            return text;
        }
    }
}
