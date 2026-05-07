using System.Collections.Generic;

namespace Util.Interfaces
{
    public interface IHandlerText
    {
        /// <summary>
        /// Converts a string encoded in ISO-8859-1 to its UTF-8 representation.
        /// </summary>
        /// <remarks>Use this method to convert text from legacy ISO-8859-1 encoding to UTF-8 for
        /// compatibility with modern systems.</remarks>
        /// <param name="txt">The input string encoded in ISO-8859-1 to convert.</param>
        /// <returns>A string containing the UTF-8 encoded representation of the input.</returns>
        string ConvertISOtoUTF(string txt);

        /// <summary>
        /// Converts a string from UTF-8 encoding to ISO-8859-1 encoding.
        /// </summary>
        /// <remarks>Use this method when interoperability with systems that require ISO-8859-1 encoding
        /// is necessary. Characters in the input string that cannot be represented in ISO-8859-1 will be replaced
        /// according to the encoding's fallback mechanism.</remarks>
        /// <param name="txt">The input string encoded in UTF-8 to convert to ISO-8859-1.</param>
        /// <returns>A string representing the input text re-encoded as ISO-8859-1. Characters not representable in ISO-8859-1
        /// are replaced with a fallback character.</returns>
        string ConvertUTFtoISO(string txt);

        /// <summary>
        /// Returns a string containing only the numeric characters from the specified input string.
        /// </summary>
        /// <param name="val">The input string from which to extract numeric characters. Can be <see langword="null"/>.</param>
        /// <returns>A string consisting of only the digit characters from <paramref name="val"/>, in their original order; or
        /// <see langword="null"/> if <paramref name="val"/> is <see langword="null"/> or empty.</returns>
        string KeepOnlyNumbers(string val);

        /// <summary>
        /// Removes all diacritical marks (accents) from the specified string, returning the unaccented equivalent.
        /// </summary>
        /// <remarks>This method preserves the original characters except for diacritical marks, which are
        /// removed. The returned string may be used for accent-insensitive comparisons or searches.</remarks>
        /// <param name="text">The input string from which to remove accents. Cannot be <see langword="null"/>.</param>
        /// <returns>A new string with all accent marks removed from the original text. If <paramref name="text"/> contains no
        /// accents, the original string is returned unchanged.</returns>
        string RemoveAccents(string text);

        /// <summary>
        /// Converts a list of strings to a list of integers by parsing each string element.
        /// </summary>
        /// <param name="list">The list of strings to parse into integers. Elements that cannot be parsed are ignored.</param>
        /// <returns>A list of integers containing the successfully parsed values from <paramref name="list"/>. The order of
        /// elements is preserved, and unparseable strings are omitted.</returns>
        List<int> SplitToInt(List<string> list);

        /// <summary>
        /// Converts the specified string to a URL-friendly format by removing accents, converting to lowercase, and
        /// replacing spaces with hyphens.
        /// </summary>
        /// <param name="val">The input string to convert to a URL-friendly format. Cannot be <see langword="null"/>.</param>
        /// <returns>A URL-friendly version of the input string, with accents removed, non-alphanumeric characters excluded, and
        /// spaces replaced by hyphens.</returns>
        string ToFriendlyUrl(string val);

        /// <summary>
        /// Returns a substring of the specified text containing the first occurrence of the given keyword, including a
        /// specified number of words before and after the keyword.
        /// </summary>
        /// <remarks>Words are determined by splitting the text on spaces. The method does not preserve
        /// original punctuation or formatting.</remarks>
        /// <param name="text">The input text to search and truncate. Cannot be <see langword="null"/> or whitespace.</param>
        /// <param name="keyword">The keyword to locate within the text. The search is case-insensitive. Cannot be <see langword="null"/> or
        /// whitespace.</param>
        /// <param name="before">The maximum number of words to include before the first occurrence of the keyword. Must be zero or greater.
        /// The default is 250.</param>
        /// <param name="after">The maximum number of words to include after the first occurrence of the keyword. Must be zero or greater.
        /// The default is 250.</param>
        /// <returns>A substring containing the keyword and up to the specified number of words before and after it.  If the
        /// keyword is not found, returns the first <paramref name="before"/> plus <paramref name="after"/> words of the
        /// text.  If <paramref name="text"/> or <paramref name="keyword"/> is <see langword="null"/> or whitespace,
        /// returns <paramref name="text"/> unchanged.</returns>
        string TruncateAroundKeyword(string text, string keyword, int before = 250, int after = 250);
    }
}