using System.Globalization;
using System.Text;

namespace CtrlCV.Util
{
    public class Text
    {
        public static string Normalize(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return texto;

            var map = new Dictionary<char, string>
            {
                // a
                ['á'] = "a",
                ['à'] = "a",
                ['ã'] = "a",
                ['â'] = "a",
                ['ä'] = "a",
                ['å'] = "a",
                ['Á'] = "A",
                ['À'] = "A",
                ['Ã'] = "A",
                ['Â'] = "A",
                ['Ä'] = "A",
                ['Å'] = "A",

                // e
                ['é'] = "e",
                ['è'] = "e",
                ['ê'] = "e",
                ['ë'] = "e",
                ['É'] = "E",
                ['È'] = "E",
                ['Ê'] = "E",
                ['Ë'] = "E",

                // i
                ['í'] = "i",
                ['ì'] = "i",
                ['î'] = "i",
                ['ï'] = "i",
                ['Í'] = "I",
                ['Ì'] = "I",
                ['Î'] = "I",
                ['Ï'] = "I",

                // o
                ['ó'] = "o",
                ['ò'] = "o",
                ['õ'] = "o",
                ['ô'] = "o",
                ['ö'] = "o",
                ['ø'] = "o",
                ['Ó'] = "O",
                ['Ò'] = "O",
                ['Õ'] = "O",
                ['Ô'] = "O",
                ['Ö'] = "O",
                ['Ø'] = "O",

                // u
                ['ú'] = "u",
                ['ù'] = "u",
                ['û'] = "u",
                ['ü'] = "u",
                ['Ú'] = "U",
                ['Ù'] = "U",
                ['Û'] = "U",
                ['Ü'] = "U",

                // c
                ['ç'] = "c",
                ['Ç'] = "C",

                // n
                ['ñ'] = "n",
                ['Ñ'] = "N",

                // y
                ['ý'] = "y",
                ['ÿ'] = "y",
                ['Ý'] = "Y",

                // ligaturas
                ['æ'] = "ae",
                ['Æ'] = "AE",
                ['œ'] = "oe",
                ['Œ'] = "OE",

                // alemão
                ['ß'] = "ss",

                // outros comuns
                ['ð'] = "d",
                ['Ð'] = "D",
                ['þ'] = "th",
                ['Þ'] = "Th",
                ['ł'] = "l",
                ['Ł'] = "L"
            };

            var sb = new StringBuilder(texto.Length);

            foreach (var c in texto)
            {
                if (map.TryGetValue(c, out var replacement))
                    sb.Append(replacement);
                else
                    sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
