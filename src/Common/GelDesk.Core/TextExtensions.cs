using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GelDesk
{
    public static class TextExtensions
    {
        #region Conversion

        public static TEnum ToEnum<TEnum>(this string value, TEnum defaultResult = default(TEnum))
            where TEnum : struct
        {
            return ToEnum<TEnum>(value, true, defaultResult);
        }

        public static TEnum ToEnum<TEnum>(this string value, bool ignoreCase, TEnum defaultResult = default(TEnum))
            where TEnum : struct
        {
            TEnum result;
            if (Enum.TryParse<TEnum>(value, ignoreCase, out result))
                return result;
            return defaultResult;
        }

        #endregion

        #region Formatting

        public static string FormatSafe(this string format, params object[] args)
        {
            if (format != null)
            {
                try { return String.Format(format, args); }
                catch { return SR.ErrorFormatBad + " " + format; }
            }
            return null;
        }

        public static string FormatWith(this string value, params object[] args)
        {
            return String.Format(value, args);
        }

        public static string JoinLines(this IEnumerable<string> value)
        {
            var sb = new StringBuilder();
            foreach (var item in value)
                sb.AppendLine(item);
            return sb.ToString();
        }

        #endregion

        #region Nullity

        public static string IfNull(this string value, string defaultValue)
        {
            return value ?? defaultValue;
        }

        public static string IfNullOrEmpty(this string value, string defaultValue)
        {
            return String.IsNullOrEmpty(value) ?
                defaultValue :
                value;
        }

        public static string IfNullOrWhitespace(this string value, string defaultValue)
        {
            return String.IsNullOrWhiteSpace(value) ?
                defaultValue :
                value;
        }

        public static bool NullOrEmpty(this string value)
        {
            return String.IsNullOrEmpty(value);
        }

        public static bool NullOrWhiteSpace(this string value)
        {
            return String.IsNullOrWhiteSpace(value);
        }

        #endregion

        #region Parsing

        /// <summary>
        /// Given a sequence of monotonically increasing string patterns, find the next value or value fills a gap.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="baseValue">The base value that represents the string pattern without any numerical formatting.</param>
        /// <param name="format">The format used to pattern the string numerically.</param>
        /// <param name="firstId">The first numerical id to begin searching after <see cref="baseValue"/>.</param>
        /// <returns></returns>
        /// <example>
        /// <code><![CDATA[
        ///     var folderNames = new [] {
        ///         "New Folder", "New Folder (1)", "New Folder (3)"
        ///     };
        ///     var nextFolderName = folderNames.FindNextOrGapInSequence("New Folder");
        ///     Console.WriteLine(nextFolderName);
        ///     // Result: New Folder (2)
        /// ]]></code>
        /// </example>
        public static string FindNextOrGapInSequence(
            this IEnumerable<string> sequence,
            string baseValue,
            string format = "{0} ({1})",
            int firstId = 1
        )
        {
            var currentValue = baseValue;
            var nextId = firstId;
            while (sequence.Any(value => value == currentValue))
            {
                currentValue = String.Format(format, baseValue, nextId.ToString());
                nextId += 1;
            }
            return currentValue;
        }

        public static string ReplaceAll(this string value, char[] characters)
        {
            foreach (var item in characters)
                value = value.Replace(item.ToString(), "");
            return value;
        }

        public static string[] SplitCommandFromArguments(this string commandLine, bool removeQuotes = true)
        {
            if (commandLine == null)
                return new string[] { null };

            commandLine = commandLine.Trim();
            if (commandLine.Length == 0)
                return new string[] { commandLine };

            var chars = commandLine.ToCharArray();
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var commandPart = String.Empty;
            var hasQuotes = false;
            var i = 0;
            for (; i < chars.Length; i++)
            {
                if (chars[i] == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                    hasQuotes = true;
                }
                else if (chars[i] == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                    hasQuotes = true;
                }
                else if (!inSingleQuote && !inDoubleQuote && chars[i] == ' ')
                {
                    if (hasQuotes && removeQuotes)
                        commandPart = commandLine.Substring(1, i - 2);
                    else
                        commandPart = commandLine.Substring(0, i);
                    return new string[] {
                        commandPart,					// The Command
				        commandLine.Substring(i + 1),	// The Arguments
			        };
                }
            }
            return new string[] {
                // Just a Command
                hasQuotes ? commandLine.Substring(1, i - 2) : commandLine
	        };
        }
        public static string[] SplitWith(this string value, char delimiter)
        {
            return value.Split(new char[] { delimiter });
        }

        public static string[] SplitWith(this string value, char delimiter, StringSplitOptions options)
        {
            return value.Split(new char[] { delimiter }, options);
        }

        public static bool TrySplitPair(this string source, char separator, out string s1, out string s2)
        {
            int i = source.IndexOf(separator);

            if (i < 0)
            {
                s1 = "";
                s2 = "";

                return false;
            }

            s1 = source.Substring(0, i);
            s2 = source.Substring(i + 1);

            return true;
        }

        #endregion
    }
}
