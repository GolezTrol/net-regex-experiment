using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace RexExExp
{
    public class Match
    {
        public string value;
        public int startpos;
        public int length;
    }

    // A sub-pattern consists of a character to match (or the '.' wildcard), and a flag indicating if it's fixed or variable.
    public class SubPattern
    {
        public string mask;
        public int min = 1;
        public int max = 1;

        public SubPattern(string mask)
        {
            this.mask = mask;
        }

        public bool Match(string input, ref int position, ref Match match)
        {
            if (match == null)
            {
                match = new Match() { startpos = position, length = max };
            }
            else if (match.length == min)
            {
                // Already done this pattern, and backtracked it completely. It's not matching, go back further.
                return false;
            } 
            else
            {
                match.length--;
            }

            var iteration = 0;

            // A pattern can match 0 or more strings, so loop while we match.
            while (true)
            {
                // Check if there is something to match, and if so, if it does match.

                bool matched = position + mask.Length <= input.Length;

                if (matched && mask != ".")
                {
                    string sub = input.Substring(position, mask.Length);

                    matched = mask == sub;
                }

                if (matched)
                {
                    // There is a match for this character, go to the next character.
                    position += mask.Length;
                    // If we reached the max, also go to the next pattern.
                    if (++iteration >= max)
                        break;
                }
                else
                {
                    // No match for this character. 
                    // If we reached the minimum for this subpattern, check the next.
                    if (iteration >= min)
                        break;

                    // If we didn't reach the minimum yet, then the pattern doesn't match.
                    return false;
                }
            }
            match.length = iteration;
            match.value = input.Substring(match.startpos, match.length);

            return true;
        }
    }

    public class Pattern
    {
        public List<SubPattern> subPatterns;

        public Pattern(string pattern)
        {
            subPatterns = new List<SubPattern>();

            var p = 0;

            while (p < pattern.Length)
                subPatterns.Add(ScanPattern(pattern, ref p));
        }

        private bool ScanInt(string pattern, ref int p, ref int result)
        {
            // Scan for a sequence of digits. Return false if 0 digits were found.
            bool found = false;
            char c;
            result = 0;

            while (p < pattern.Length)
            {
                c = pattern[p];
                if (char.IsDigit(c))
                {
                    p++;
                    result = result * 10 + (c - '0');
                    found = true;
                } else
                {
                    break;
                }
            }

            return found;
        }

        private bool ScanQuantifier(string pattern, ref int p, out int min, out int max)
        {
            (min, max) = (1, 1);
            if (p >= pattern.Length) return false;

            char c = pattern[p++];
            (min, max) = (0, int.MaxValue);
            if (c == '*') (min, max) = (0, int.MaxValue);
            else if (c == '+') (min, max) = (1, int.MaxValue);
            else if (c == '?') (min, max) = (0, 1);
            else if (c == '{')
            {
                // At least one non-negative number expected
                if (!ScanInt(pattern, ref p, ref min))
                    throw new ExpectedException("number", pattern, p); // Expected a non-negative int.

                if (pattern[p] == ',')
                {
                    if (min < 0)
                        throw new ValueException("Lower bound of quantifier cannot be lower than 0", pattern, p);

                    // If followed by a comma, it's a range. The end of the range is optional.
                    p++;
                    if (!ScanInt(pattern, ref p, ref max))
                    {
                        max = int.MaxValue;
                    }
                    else
                    {
                        if (max < min)
                        {
                            // To do. Start throwing more specific exceptions
                            // In this case, tell user that max cannot be < min
                            throw new ValueException("Upper bound of range cannot be lower than lower bound.", pattern, p);
                        }
                    }
                }
                else
                {
                    // No comma, so a fixed length. Max = min, but it should be non-zero positive. 
                    max = min;
                    if (min <= 0)
                        throw new ValueException("Quantifier must be more than 0.", pattern, p);
                }

                if (pattern[p++] != '}')
                    throw new ExpectedException("closing }", pattern, --p);
            }
            else
            {
                // No explicit quantifier. Implies exactly 1.
                (min, max) = (1, 1);
                p--;
                return false;
            }

            return true;
        }

        private bool IsLiteral(char c)
        {
            return char.IsLetterOrDigit(c);
        }

        private bool PeekQuantifier(char c)
        {
            return "*+?{".Contains(c);
        }

        private SubPattern ScanPattern(string pattern, ref int p)
        {
            int min;
            int max;

            char c = pattern[p];

            if (c == '.') {
                p++;
                ScanQuantifier(pattern, ref p, out min, out max);
                return new SubPattern(".") { min = min, max = max };
            }
            else if (IsLiteral(c))
            {
                int e = p;
                for (; e < pattern.Length; e++) {
                    if (!IsLiteral(pattern[e]))
                    {
                        if (PeekQuantifier(pattern[e]) && e-p > 1)
                        {
                            // If the just scanned group of literals consists of more than one, and it
                            // is followed by a quantifier, don't take the last character. 
                            // Ex: abcd*e would result in subpatterns abc, d* and e
                            e--;
                        }
                        break;
                    }
                }

                string sequence = pattern.Substring(p, e-p);
                p = e;
                ScanQuantifier(pattern, ref p, out min, out max);
                return new SubPattern(sequence) { min = min, max = max };
            } 
            else
            {
                throw new ExpectedException("alphanumeric character or wildcard", pattern, p);
            }
        }
    }

    public class Matcher
    {
        public Pattern pattern;

        public Matcher(string pattern)
        {
            this.pattern = new Pattern(pattern);
        }

        public List<Match> Match(string input)
        {
            var i = 0;
            var p = 0;

            var result = new List<Match>();

            for (p = 0; p < pattern.subPatterns.Count; p++)
            {
                SubPattern sub = pattern.subPatterns[p];
                Match match = null;
                if (!sub.Match(input, ref i, ref match))
                {
                    return null;
                } else
                {
                    result.Add(match);
                }
            }

            // Currently we need to match the entire string, so result is only true if there is no more input string left.
            if (i < input.Length)
                return null;
            return result;
        }

        public bool IsMatch(string input)
        {
            var matches = Match(input);
            return !(matches == null || matches.Count == 0);
        }

        public static bool IsMatch(string input, string pattern)
        {
            return (new Matcher(pattern)).IsMatch(input);
        }
    }
}
