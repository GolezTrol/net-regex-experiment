namespace RexExExp
{
    // Exception to throw in case of unexpected characters in a pattern, especially when * is at the start of the string, or the ** after each other.
    public class PatternException : System.ArgumentException
    {
        public PatternException(string message, string pattern, int position) :
            base(message)
        {
            Pattern = pattern;
            Position = position;
        }

        public string Pattern { get; private set; }
        public int Position { get; private set; }
    }

    public class UnexpectedCharacterException : PatternException {
        public UnexpectedCharacterException(string pattern, int position): base($"{pattern[position]} was unexpected.", pattern, position)
        {

        }
    }

    public class ExpectedException: PatternException
    {
        public ExpectedException(string expected, string pattern, int position): base(GetMessage(expected, pattern, position), pattern, position)
        {
        }

        private static string GetMessage(string expected, string pattern, int position)
        {
            string found = (position < pattern.Length ? pattern[position].ToString() : "EOF");

            return $"Expected to find {expected}, but found {found}.";
        }
    }

    public class ValueException : PatternException
    {
        public ValueException(string message, string pattern, int position) : base(message, pattern, position)
        {

        }
    }
}
