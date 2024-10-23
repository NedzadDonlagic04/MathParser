namespace MathParser.Core
{
    public class MathFunctionException : Exception
    {
        public MathFunctionException() { }

        public MathFunctionException(string message) : base(message) { }

        public MathFunctionException(string message, Exception inner) : base(message, inner) { }
    }
}
