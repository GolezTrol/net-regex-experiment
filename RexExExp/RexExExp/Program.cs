using System;
using System.Security;
using System.Text.RegularExpressions;

namespace RexExExp
{
    class Program
    {
        static int Verify(string input, string pattern, bool expected, string reason)
        {
            var m = new Matcher(pattern);
            bool ok = m.IsMatch(input) == expected;
            Console.WriteLine($"{input}\n{pattern}\n{reason}\n" + (ok ? "ok" : "FAILED") + "\n");

            return ok ? 0 : 1; // Return 1 in case of error
        }

        static int VerifyException(string input, string pattern, Type expected, string reason)
        {
            bool ok = false;
            try
            {
                Matcher.IsMatch(input, pattern);
            }
            catch(Exception ex) {
                ok = ex.GetType() == expected;
                Console.WriteLine($"Caught: {ex.Message}");
                Console.WriteLine($"{input}\n{pattern}\n{reason}\n" + (ok ? "ok" : "FAILED") + "\n");
            }
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
                Verify("abbbbcd", "ab{3}cd", false, "{x} for 'exactly x times' quantifier") +
                Verify("abbbbcd", "ab{4}cd", true, "{x} for 'exactly x times' quantifier") +
                Verify("abbbbcd", "ab{5}cd", false, "{x} for 'exactly x times' quantifier") +
                Verify("abbbbcd", "ab{1,3}cd", false, "{x,y} for 'x to y times' quantifier") +
                Verify("abbbbcd", "ab{3,7}cd", true, "{x,y} for 'x to y times' quantifier") +
                Verify("abbbbcd", "ab{3,}cd", true, "{x,} for 'at least x times' quantifier") +
                Verify("abbbbcd", "ab{5,}cd", false, "{x,} for 'at least x times' quantifier") +
                VerifyException("abbbbcd", "ab{5,cd", typeof(ExpectedException), "quantifier should end with }") +
                VerifyException("a", "{5}", typeof(ExpectedException), "quantifier should be preceeded by a character") +
                Verify("aaaa", "a*aa", true, "Backtracking for *") +
                Verify("abbbbbbbbcd", "ab{3,}bbbcd", true, "Backtracking for 'at least' {x,} quantifier") +
                Verify("abbbbbbbbcccdddd", "ab{3,}bbbc*cd+", true, "Backtracking for a combination of unfortunate quantifiers") +
                Verify("abbbbcd", "ab{1,1}bbbbcd", false, "Backtracking ranged {1,1} quantifier, should fail, because fixed pattern after takes all") +
                Verify("abac", "a*b*a*c*", true, "Backtracking repeating groups of a*.") +
                Verify("aaaaac", "a*b*a*c*", true, "Backtracking repeating groups of a*.") +
                Verify("aac", "a*b*a*c*", true, "Backtracking repeating groups of a*.") +
                Verify("c", "a*b*a*c*", true, "Backtracking repeating groups of a*.") +
                0;

            Console.WriteLine($"\n---------------\n{errors} errors");

            return errors;
        }
    }
}
