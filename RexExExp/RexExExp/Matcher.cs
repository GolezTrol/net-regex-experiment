using System.Collections.Generic;
using System.ComponentModel;
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
        public bool variable;

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
            SubPattern subPattern = null; ;
            for (int p = 0; p < pattern.Length; ++p)
            {
                if (pattern[p] == '*')
                {
                    if (subPattern == null)
                    {
                        throw new PatternException(pattern, p);
                    }
                    subPattern.variable = true;
                    subPattern = null;
                }
                else
                {
                    subPattern = new SubPattern(pattern[p]);
                    subPatterns.Add(subPattern);
                }
            }
        }
    }

    public class Matcher
    {
        public static bool IsMatch(string input, string pattern)
        {
            Pattern pat = new Pattern(pattern);

            var p = 0;
            
            foreach (char c in input)
            {
                // 0 or more patterns can match a character, so loop through the patterns while they match.
                while (true)
                {
                    // If we run out of sub-patterns, there is no full match. There is unmatched input at the end.
                    if (p >= pat.subPatterns.Count) 
                        return false;

                    var sub = pat.subPatterns[p];

                    if (sub.mask != c && sub.mask != '.')
                    {
                        // If the input character doesn't match the pattern, _and_ the pattern is not a variable 
                        // pattern, there is no match.
                        if (!sub.variable) 
                            return false;
                        // If it is a variable subpattern, it's fine. This subpattern has ended, on to the next.
                        // Don't break the loop here. We still need to match the input character c!
                        p++;
                    }
                    else
                    {
                        // There is a match! If this is not a variable subpattern, advance to the next subpatternn.
                        if (!sub.variable)
                            p++;
                        // End the inner loop here. The input character c was matched.
                        break;
                    }
                }
            }

            // Out of input string. Skip any remaining subpatterns if they are of variable length.
            if (p < pat.subPatterns.Count && pat.subPatterns[p].variable)
                p++;
              
            // No subpatterns left? Then we had a full match.
            return p == pat.subPatterns.Count;
        }
    }
}
