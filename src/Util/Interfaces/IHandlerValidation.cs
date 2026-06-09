namespace Util.Interfaces
{
    public interface IHandlerValidation
    {
        /// <summary>
        /// Determines whether the specified credit card number is valid according to standard validation rules.
        /// </summary>
        /// <param name="cardNumber">The credit card number to validate. The value must be a non-null, non-empty string containing only numeric
        /// digits, with or without spaces or dashes.</param>
        /// <returns><see langword="true"/> if <paramref name="cardNumber"/> is a valid credit card number; otherwise, <see
        /// langword="false"/>.</returns>
        bool IsValidCreditCard(string cardNumber);

        /// <summary>
        /// Determines whether the specified email address is in a valid format.
        /// </summary>
        /// <param name="email">The email address to validate. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="email"/> is in a valid email address format; otherwise, <see
        /// langword="false"/>.</returns>
        bool IsValidEmail(string email);

        /// <summary>
        /// Determines whether the specified string is a valid phone number.
        /// </summary>
        /// <remarks>The criteria for a valid phone number may vary by implementation. Callers should
        /// refer to the specific implementation's documentation for details on supported formats and validation
        /// rules.</remarks>
        /// <param name="phoneNumber">The phone number to validate. The format and validation rules may depend on the implementation.</param>
        /// <returns><see langword="true"/> if <paramref name="phoneNumber"/> is recognized as a valid phone number; otherwise,
        /// <see langword="false"/>.</returns>
        bool IsValidPhoneNumber(string phoneNumber);
    }
}