﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace RexExExp
{
    // Exception to throw in case of unexpected characters in a pattern, especially when * is at the start of the string, or the ** after each other.
    class PatternException : System.ArgumentException
    {
        public PatternException(string pattern, int position) :
            base($"Invalid pattern, {pattern}, {pattern[position]} was unexpected at position {position}.")
        {
            Pattern = pattern;
            Position = position;
        }

        public string Pattern { get; private set; }
        public int Position { get; private set; }
    }

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
                throw new PatternException(pattern, --p);

            return c;
        }

        private int ScanInt(string pattern, ref int p)
        {
            int i = 0;
            char c;
            while (p < pattern.Length)
            {
                c = pattern[p];
                if (char.IsDigit(c))
                {
                    p++;
                    i = i * 10 + (c - '0');
                } else
                {
                    break;
                }
            }

            if (i <= 0)
                throw new PatternException(pattern, p);

            return i;
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
                    min = ScanInt(pattern, ref p);
                    max = min;
                    if (pattern[p++] != '}')
                        throw new PatternException(pattern, --p);
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
                var iteration = 0;

                // A pattern can match 0 or more characters, so loop while we match.
                while (true) {

                    // Check if there is something to match, and if so, if it does match.
                    bool match = (i < input.Length && (input[i] == sub.mask || sub.mask == '.'));

                    if (match)
                    {
                        // There is a match for this character, go to the next character.
                        i++;
                        // If we reached the max, also go to the next pattern.
                        if (++iteration >= sub.max)
                            break;
                    }
                    else
                    {
                        // No match for this character. 
                        // If we reached the minimum for this subpattern, check the next.
                        if (iteration >= sub.min) 
                            break;
                        
                        // If we didn't reach the minimum yet, there the pattern doesn't match.
                        return false;
                    }
                }
            }
            
            // Currently we need to match the entire string, so result is only true if there is no more input string left.
            return i == input.Length;
        }
    }
}
