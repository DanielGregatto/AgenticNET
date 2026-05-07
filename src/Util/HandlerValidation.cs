using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Util.Interfaces;

namespace Util
{
    public class HandlerValidation : IHandlerValidation
    {
        private readonly IHandlerText _handlerText;
        public HandlerValidation(IHandlerText handlerText)
        {
            _handlerText = handlerText;
        }

        public bool IsValidCPF(string cpf)
        {
            if (string.IsNullOrEmpty(cpf)) return false;

            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;
            cpf = cpf.Trim();
            cpf = cpf.Replace(".", "").Replace("-", "");
            if (cpf.Length != 11)
                return false;
            tempCpf = cpf.Substring(0, 9);
            soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = digito + resto.ToString();
            return cpf.EndsWith(digito);
        }

        public bool IsValidCNPJ(string cnpj)
        {
            if (string.IsNullOrEmpty(cnpj)) return false;

            int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma;
            int resto;
            string digito;
            string tempCnpj;
            cnpj = cnpj.Trim();
            cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");
            if (cnpj.Length != 14)
                return false;
            tempCnpj = cnpj.Substring(0, 12);
            soma = 0;
            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];
            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCnpj = tempCnpj + digito;
            soma = 0;
            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];
            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = digito + resto.ToString();
            return cnpj.EndsWith(digito);
        }

        public bool IsValidCPFOrCNPJ(string cpfOrCnpj)
        {
            if (string.IsNullOrEmpty(cpfOrCnpj))
                return false;

            var isvalid = IsValidCPF(cpfOrCnpj);
            if (isvalid)
                return true;

            isvalid = IsValidCNPJ(cpfOrCnpj);

            return isvalid;
        }

        public bool IsValidEmail(string email)
        {
            Regex rg = new Regex(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$");
            if (rg.IsMatch(email))
                return true;
            else
                return false;
        }

        public bool IsValidCreditCard(string cardNumber)
        {
            if (!string.IsNullOrEmpty(cardNumber))
                cardNumber = _handlerText.KeepOnlyNumbers(cardNumber);

            List<string> allowedTest = new List<string>()
            {
                "4000000000000010", "4000000000000028", "4000000000000036", "4000000000000044", "4000000000000077", "4000000000000093", "4000000000000051", "4000000000000069"
            };

            if (allowedTest.Contains(cardNumber))
                return true;

            if (string.IsNullOrWhiteSpace(cardNumber) || !cardNumber.All(char.IsDigit))
                return false;

            int sum = 0;
            bool alternate = false;
            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int digit = cardNumber[i] - '0';

                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }

                sum += digit;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }

        public bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            string cleanedPhone = phoneNumber.Trim()
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "");

            if (!Regex.IsMatch(cleanedPhone, @"^\d+$"))
                return false;

            if (cleanedPhone.Length < 10 || cleanedPhone.Length > 11)
                return false;

            return true;
        }
    }
}