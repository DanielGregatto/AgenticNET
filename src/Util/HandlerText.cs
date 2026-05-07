using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Util.Interfaces;

namespace Util
{
    public class HandlerText : IHandlerText
    {
        public string ConvertISOtoUTF(string txt)
        {
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes(txt);
            byte[] isoBytes = Encoding.Convert(iso, utf8, utfBytes);
            string msg = iso.GetString(isoBytes);
            return msg;
        }
        public string ConvertUTFtoISO(string txt)
        {
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes(txt);
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            string msg = iso.GetString(isoBytes);
            return msg;
        }
        public string KeepOnlyNumbers(string val)
        {
            if (!string.IsNullOrEmpty(val))
                return String.Join("", Regex.Split(val, @"[^\d]"));

            return null;
        }
        public string RemoveAccents(string text)
        {
            StringBuilder sbReturn = new StringBuilder();
            var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
            foreach (char letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }
            return sbReturn.ToString();
        }
        public List<int> SplitToInt(List<string> list)
        {
            var intList = new List<int>();

            foreach (var item in list)
            {
                var convert = int.TryParse(item, out int value);
                if (convert)
                    intList.Add(value);
            }

            return intList;
        }
        public string ToFriendlyUrl(string val)
        {
            string result = Regex.Replace(this.RemoveAccents(val).ToLower(), "[^0-9a-zA-Z ]+", "").Trim().Replace(' ', '-');
            return result;
        }
        public string TruncateAroundKeyword(string text, string keyword, int before = 250, int after = 250)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(keyword))
                return text;

            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var keywordIndex = Array.FindIndex(words, w => w.Contains(keyword, StringComparison.OrdinalIgnoreCase));

            if (keywordIndex == -1)
                return string.Join(' ', words.Take(before + after)); // fallback if keyword not found

            var start = Math.Max(0, keywordIndex - before);
            var end = Math.Min(words.Length - 1, keywordIndex + after);

            var selectedWords = words[start..(end + 1)];

            return string.Join(' ', selectedWords);
        }
    }
}
