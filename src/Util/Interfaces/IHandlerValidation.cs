namespace Util.Interfaces
{
    public interface IHandlerValidation
    {
        /// <summary>
        /// Determines whether the specified CNPJ (Cadastro Nacional da Pessoa Jurídica) number is valid.
        /// </summary>
        /// <remarks>This method checks the structure and check digits of the CNPJ to ensure it conforms
        /// to the official Brazilian standard. It ignores formatting characters and validates only the numeric
        /// content.</remarks>
        /// <param name="cnpj">The CNPJ number to validate. The value must be a string containing 14 digits, with or without formatting
        /// characters such as periods, slashes, or hyphens.</param>
        /// <returns><see langword="true"/> if the specified CNPJ is valid according to official validation rules; otherwise,
        /// <see langword="false"/>.</returns>
        bool IsValidCNPJ(string cnpj);

        /// <summary>
        /// Determines whether the specified string is a valid Brazilian CPF (Cadastro de Pessoas Físicas) number.
        /// </summary>
        /// <remarks>This method checks the format and check digits of the CPF according to official
        /// validation rules. It does not verify whether the CPF is actually assigned to an individual.</remarks>
        /// <param name="cpf">The CPF number to validate. The value can include or omit formatting characters such as periods and hyphens.</param>
        /// <returns><see langword="true"/> if <paramref name="cpf"/> is a valid CPF number; otherwise, <see langword="false"/>.</returns>
        bool IsValidCPF(string cpf);

        /// <summary>
        /// Determines whether the specified string is a valid CPF or CNPJ number.
        /// </summary>
        /// <remarks>This method checks the validity of Brazilian individual (CPF) and company (CNPJ)
        /// identification numbers, including their check digits. The input may be formatted or unformatted.</remarks>
        /// <param name="cpfOrCnpj">The string to validate as a CPF (Cadastro de Pessoas Físicas) or CNPJ (Cadastro Nacional da Pessoa Jurídica)
        /// number. The value may include formatting characters such as periods, hyphens, or slashes.</param>
        /// <returns><see langword="true"/> if <paramref name="cpfOrCnpj"/> is a valid CPF or CNPJ number; otherwise, <see
        /// langword="false"/>.</returns>
        bool IsValidCPFOrCNPJ(string cpfOrCnpj);

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