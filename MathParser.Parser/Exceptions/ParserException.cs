﻿namespace MathParser.Core
{
    public class ParserException : Exception
    {
        public ParserException() { }

        public ParserException(string message) : base(message) { }

        public ParserException(string message, Exception inner) : base(message, inner) { }
    }
}
