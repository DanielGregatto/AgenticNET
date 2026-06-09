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