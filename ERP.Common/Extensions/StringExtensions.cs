using System.Linq;
using System.Text;

namespace ERP.Common.Extensions
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            return string.IsNullOrEmpty(str) || str.Length < 2 ? str : char.ToLowerInvariant(str[0]) + str[1..];
        }

        public static string RemoveSpecialCharacter(this string str)
        {
            var sb = new StringBuilder();
            foreach (
                var c in str.Where(c => c is >= '0' and <= '9' or >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '.' or '_'))
            {
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}