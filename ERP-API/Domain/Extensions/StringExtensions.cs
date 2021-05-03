namespace ERP_API.Domain.Extensions
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            return string.IsNullOrEmpty(str) || str.Length < 2 ? str : char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}