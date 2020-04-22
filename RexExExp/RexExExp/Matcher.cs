using System.Text.RegularExpressions;

namespace RexExExp
{
    public class Matcher
    {
        public static bool IsMatch(string input, string pattern)
        {
            return Regex.IsMatch(input, $"^{pattern}$");
        }
    }
}
