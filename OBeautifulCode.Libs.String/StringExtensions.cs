﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="OBeautifulCode">
//   Copyright 2014 OBeautifulCode
// </copyright>
// <summary>
//   Adds some convenient extension methods to strings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace OBeautifulCode.Libs.String
{
    using System;
    using System.Data.Entity.Design.PluralizationServices;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using CuttingEdge.Conditions;

    /// <summary>
    /// Adds some convenient extension methods to strings.
    /// </summary>
    public static class StringExtensions
    {
        #region Fields (Private)

        /// <summary>
        /// Lock object for the pluralization service.
        /// </summary>
        private static readonly object PluralizationServiceLock = new object();

        /// <summary>
        /// Represents an ASCII character encoding of Unicode characters
        /// </summary>
        private static readonly Encoding AsciiEncoding = new ASCIIEncoding();

        /// <summary>
        /// Represents a UTF-16 encoding of Unicode characters
        /// </summary>
        private static readonly Encoding UnicodeEncoding = new UnicodeEncoding();

        /// <summary>
        /// Represents a UTF-8 encoding of Unicode characters.
        /// </summary>
        private static readonly Encoding Utf8Encoding = new UTF8Encoding();

        /// <summary>
        /// The service to make word plural.
        /// </summary>
        private static PluralizationService pluralizationService;

        #endregion

        #region Constructors

        #endregion

        #region Properties

        #endregion

        #region Public Methods

        /// <summary>
        /// Appends one string to the another (base) if the base string 
        /// doesn't already end with the string to append.
        /// </summary>
        /// <param name="value">The base string.</param>
        /// <param name="shouldEndWith">The string to append.</param>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        /// <exception cref="ArgumentNullException">shouldEndWith is null.</exception>
        /// <remarks>
        /// If the string to append is the empty string, this method will always return the base string.
        /// </remarks>
        /// <returns>
        /// The inputted string where the last character is a backslash.
        /// </returns>
        public static string AppendMissing(this string value, string shouldEndWith)
        {
            Condition.Requires(value, "value").IsNotNull();
            Condition.Requires(shouldEndWith, "shouldEndWith").IsNotNull();
            if (!value.EndsWith(shouldEndWith, StringComparison.CurrentCulture))
            {
                value = value + shouldEndWith;
            }

            return value;
        }

        /// <summary>
        /// Determines if a string is alpha numeric.
        /// </summary>
        /// <param name="value">The string to evaluate.</param>
        /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
        /// <remarks>
        /// An empty string ("") is considered alpha-numeric.
        /// </remarks>
        /// <returns>
        /// Returns true if the string is alpha-numeric, false if not.
        /// </returns>
        public static bool IsAlphanumeric(this string value)
        {
            Condition.Requires(value, "value").IsNotNull();
            var regexAlphaNum = new Regex("[^a-zA-Z0-9]");
            return !regexAlphaNum.IsMatch(value);
        }

        /// <summary>
        /// Pluralizes a word.
        /// </summary>
        /// <remarks>
        /// This method is a wrapper around <see cref="System.Data.Entity.Design.PluralizationServices.PluralizationService.Pluralize"/>
        /// It's provides the convenience of creating the pluralization service once, on demand, and using that instance of the service
        /// for the life of the app domain.
        /// </remarks>
        /// <param name="value">The word to pluralize.</param>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        /// <returns>
        /// The plural form of the input parameter.
        /// </returns>
        public static string Pluralize(this string value)
        {
            Condition.Requires(value, "value").IsNotNull();
            if (pluralizationService == null)
            {
                lock (PluralizationServiceLock)
                {
                    if (pluralizationService == null)
                    {
                        pluralizationService = PluralizationService.CreateService(CultureInfo.CurrentCulture);
                    }
                }
            }

            return pluralizationService.Pluralize(value);
        }

        /// <summary>
        /// Performs a fast case-insensitive string replacement
        /// </summary>
        /// <remarks>
        /// adapted from <a href="http://www.codeproject.com/KB/string/fastestcscaseinsstringrep.aspx"/>
        /// If newValue is null, all occurrences of oldValue are removed
        /// </remarks>
        /// <param name="value">the string being searched</param>
        /// <param name="oldValue">string to be replaced</param>
        /// <param name="newValue">string to replace all occurrences of oldValue.</param>
        /// <exception cref="ArgumentNullException">value is null</exception>
        /// <exception cref="ArgumentNullException">oldValue is null.</exception>
        /// <exception cref="ArgumentException">oldValue is the empty string ("").</exception>
        /// <returns>
        /// A string where the case-insensitive string replacement has been applied.
        /// </returns>
        public static string ReplaceCaseInsensitive(this string value, string oldValue, string newValue)
        {
            Condition.Requires(value, "value").IsNotNull();
            Condition.Requires(oldValue, "oldValue").IsNotNullOrEmpty();
            if (newValue == null)
            {
                newValue = string.Empty;
            }

            int count = 0, position0 = 0;
            int position1;
            string upperString = value.ToUpper(CultureInfo.CurrentCulture);
            string upperPattern = oldValue.ToUpper(CultureInfo.CurrentCulture);
            int inc = (value.Length / oldValue.Length) * (newValue.Length - oldValue.Length);
            var chars = new char[value.Length + Math.Max(0, inc)];
            while ((position1 = upperString.IndexOf(upperPattern, position0, StringComparison.CurrentCulture)) != -1)
            {
                for (int i = position0; i < position1; ++i)
                {
                    chars[count++] = value[i];
                }

                foreach (char t in newValue)
                {
                    chars[count++] = t;
                }

                position0 = position1 + oldValue.Length;
            }

            if (position0 == 0)
            {
                return value;
            }

            for (int i = position0; i < value.Length; ++i)
            {
                chars[count++] = value[i];
            }

            return new string(chars, 0, count);
        }

        /// <summary>
        /// Converts a string to a byte-array with a given encoding.
        /// </summary>
        /// <param name="value">The string to encode.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <returns>byte array representing the string in a given encoding.</returns>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        public static byte[] ToBytes(this string value, Encoding encoding)
        {
            Condition.Requires(value).IsNotNull();
            Condition.Requires(encoding).IsNotNull();
            return encoding.GetBytes(value);
        }

        /// <summary>
        /// Encodes all characters in a given string to an array of bytes encoded in ASCII.
        /// </summary>
        /// <param name="value">The string to encode.</param>
        /// <returns>byte array representing the string in ASCII.</returns>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        public static byte[] ToAsciiBytes(this string value)
        {
            return value.ToBytes(AsciiEncoding);
        }

        /// <summary>
        /// Encodes all characters in a given string to an array of bytes encoded in unicode.
        /// </summary>
        /// <param name="value">The string to encode.</param>
        /// <returns>byte array representing the string in unicode.</returns>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        public static byte[] ToUnicodeBytes(this string value)
        {
            return value.ToBytes(UnicodeEncoding);
        }

        /// <summary>
        /// Encodes all characters in a given string to an array of bytes encoded in UTF-8.
        /// </summary>
        /// <param name="value">The string to encode.</param>
        /// <returns>byte array representing the string in UTF-8.</returns>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        public static byte[] ToUtf8Bytes(this string value)
        {
            return value.ToBytes(Utf8Encoding);
        }

        /// <summary>
        /// Makes a string safe to insert as a value into a 
        /// comma separated values (CSV) object such as a file.
        /// </summary>
        /// <remarks>
        /// Here are the rules for making a string CSV safe:
        /// http://en.wikipedia.org/wiki/Comma-separated_values
        /// </remarks>
        /// <param name="value">The string to make safe.</param>
        /// <returns>
        /// Returns a string that is safe to insert into a CSV object.
        /// </returns>
        public static string ToCsvSafe(this string value)
        {
            Condition.Requires(value, "value").IsNotNull();
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            bool containsCommas = value.Contains(",");
            bool containsDoubleQuotes = value.Contains("\"");
            bool containsLineBreak = value.Contains(Environment.NewLine);
            containsLineBreak = containsLineBreak || value.Contains("\n");
            bool hasLeadingSpace = value.First() == ' ';
            bool hasTrailingSpace = value.Last() == ' ';

            if (containsDoubleQuotes)
            {
                value = value.Replace("\"", "\"\"");
            }

            if (containsCommas || containsDoubleQuotes || containsLineBreak || hasLeadingSpace || hasTrailingSpace)
            {
                value = "\"" + value + "\"";
            }

            return value;
        }

        /// <summary>
        /// Performs both ToLower() and Trim() on a String
        /// </summary>
        /// <param name="value">The string to operate on.</param>
        /// <returns>Lower-case, trimmed string</returns>
        /// <exception cref="NullReferenceException">Thrown when value is null.</exception>
        public static string ToLowerTrimmed(this string value)
        {
            Condition.Requires(value, "value").IsNotNull();
            return value.ToLower(CultureInfo.CurrentCulture).Trim();
        }

        #endregion

        #region Internal Methods

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion
    }
}
