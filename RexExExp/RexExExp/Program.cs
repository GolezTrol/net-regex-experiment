using System;
using System.Security;
using System.Text.RegularExpressions;

namespace RexExExp
{
    class Program
    {
        static int Verify(string input, string pattern, bool expected, string reason)
        {
            bool ok = Matcher.IsMatch(input, pattern) == expected;
            Console.WriteLine($"{input}\n{pattern}\n{reason}\n" + (ok ? "ok" : "FAILED"));

            return ok ? 0 : 1; // Return 1 in case of error
        }

        static int Main(string[] args)
        {
            var errors =
                Verify("aa", "a", false, "'a' does not match the entire string 'aa'.") +
                Verify("aa", "a*", true, "'*' means zero or more of the preceding element, 'a'.Therefore, by repeating 'a' once, it becomes 'aa'.") +
                Verify("ab", ".*", true, "'.' means 'zero or more () of any character (.)'.") +
                Verify("aab", "c*a*b", true, "c can be repeated 0 times, a can be repeated 2 times. Therefore, it matches 'aab'.") +
                Verify("mississippi", "mis*is*p*.", false, "Everything matches, except the 'i' beween 'ss' and 'pp'.") +
                Verify("ac", "a?b?c?", true, "? as 0 or 1 quantifier") +
                Verify("acc", "a?b?c?", false, "? as 0 or 1 quantifier can't match 2") +
                Verify("abbbbcd", "ab+cd", true, "+ for 1 or more quantifier") +
                Verify("acd", "ab+cd", false, "+ can't match 0") +
                0;

            Console.WriteLine($"\n---------------\n{errors} errors");

            return errors;
        }
    }
}
