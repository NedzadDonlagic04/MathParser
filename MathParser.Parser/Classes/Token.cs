namespace MathParser.Core
{
    internal struct Token
    {
        internal TokenType Type { get; private set; }

        internal string Value { get; private set; }

        internal Token(TokenType type, string value) => (Type, Value) = (type, value);

        public override string ToString() => $"{Type} Token -> {Value}";
    }
}
