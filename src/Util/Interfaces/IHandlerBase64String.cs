namespace Util.Interfaces
{
    public interface IHandlerBase64String
    {
        /// <summary>
        /// Encodes the specified string into its Base64 representation using ASCII encoding.
        /// </summary>
        /// <remarks>The input string is first converted to a byte array using ASCII encoding. Characters
        /// outside the ASCII range may be replaced with a question mark ('?').</remarks>
        /// <param name="texto">The string to encode. Cannot be <see langword="null"/>.</param>
        /// <returns>A Base64-encoded string representing the ASCII bytes of <paramref name="texto"/>.</returns>
        string EncodeToBase64(string texto);

        /// <summary>
        /// Decodes a Base64-encoded string into its original ASCII representation.
        /// </summary>
        /// <remarks>The input string is expected to be encoded using Base64 and represent ASCII-encoded
        /// data. If the input contains non-ASCII characters or is not properly Base64-encoded, an exception is
        /// thrown.</remarks>
        /// <param name="dados">The Base64-encoded string to decode. Must not be <see langword="null"/> or empty.</param>
        /// <returns>The decoded ASCII string represented by the input.</returns>
        string DecodeFrom64(string dados);

    }
}