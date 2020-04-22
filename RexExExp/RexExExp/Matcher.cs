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

            var i = 0;

            foreach (SubPattern sub in pat.subPatterns)
            {
                // A pattern can match 0 or more characters, so loop while we match.
                while (true) {

                    // Check if there is something to match, and if so, if it does match.
                    bool match = (i < input.Length && (input[i] == sub.mask || sub.mask == '.'));

                    // If there was a mismatch on a fixed length mask, there is no match at all.
                    if (!match && !sub.variable) return false;
                    
                    // If there was a match of any kind, advance to the next input character.
                    if (match) i++;

                    // If there was either a fixed length match, or a variable length mismatch, proceed to the next pattern. 
                    if (match ^ sub.variable) break;
                }
            }
            
            // Currently we need to match the entire string, so result is only true if there is no more input string left.
            return i == input.Length;
        }
    }
}
