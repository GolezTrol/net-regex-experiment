﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace RexExExp
{

    // A sub-pattern consists of a character to match (or the '.' wildcard), and a flag indicating if it's fixed or variable.
    class SubPattern
    {
        public char mask;
        public int min = 1;
        public int max = 1;

        public SubPattern(char mask)
        {
            this.mask = mask;
        }

        public bool Match(string input, ref int position)
        {
            var iteration = 0;

            // A pattern can match 0 or more characters, so loop while we match.
            while (true)
            {

                // Check if there is something to match, and if so, if it does match.
                bool match = (position < input.Length && (input[position] == mask || mask == '.'));

                if (match)
                {
                    // There is a match for this character, go to the next character.
                    position++;
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

                    // If we didn't reach the minimum yet, there the pattern doesn't match.
                    return false;
                }
            }
            return true;
        }
    }

    class Pattern
    {
        public List<SubPattern> subPatterns;

        public Pattern(string pattern)
        {
            subPatterns = new List<SubPattern>();

            var p = 0;

            while (p < pattern.Length)
                subPatterns.Add(ScanPattern(pattern, ref p));
        }

        private char ScanCharacterMatcher(string pattern, ref int p)
        {
            char c = pattern[p++];

            if (!(char.IsLetterOrDigit(c) || c == '.'))
                throw new ExpectedException("alphanumeric character or wildcard", pattern, --p);

            return c;
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

        private void ScanQuantifier(string pattern, ref int p, ref int min, ref int max)
        {
            if (p < pattern.Length)
            {
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
                        } else
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
                else p--;
            }
        }

        private SubPattern ScanPattern(string pattern, ref int p)
        {
            var subPattern = new SubPattern(ScanCharacterMatcher(pattern, ref p));

            ScanQuantifier(pattern, ref p, ref subPattern.min, ref subPattern.max);

            return subPattern;
        }
    }

    public class Matcher
    {
        public static bool IsMatch(string input, string pattern)
        {
            Pattern pat = new Pattern(pattern);

            var i = 0;
            foreach (SubPattern sub in pat.subPatterns)
            {
                if (!sub.Match(input, ref i))
                {
                    return false;
                }
            }
            
            // Currently we need to match the entire string, so result is only true if there is no more input string left.
            return i == input.Length;
        }
    }
}
