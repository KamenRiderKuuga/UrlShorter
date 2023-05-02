using System.Text;

namespace UrlShorter.Utils
{
    public static class UrlUtil
    {
        public static string Base62Encode(long value)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            const int radix = 62;

            if (value == 0)
            {
                return chars[0].ToString();
            }

            var result = new StringBuilder();
            while (value > 0)
            {
                int remainder = (int)(value % radix);
                result.Insert(0, chars[remainder]);
                value /= radix;
            }

            return result.ToString();
        }

        public static long Base62Decode(string value)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            const int radix = 62;

            long result = 0;
            long multiplier = 1;
            for (int i = value.Length - 1; i >= 0; i--)
            {
                int digit = chars.IndexOf(value[i]);
                result += digit * multiplier;
                multiplier *= radix;
            }

            return result;
        }
    }
}
