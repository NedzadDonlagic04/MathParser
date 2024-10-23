namespace MathParser.Core
{
    internal enum TokenType
    {
        EndOfExpression,

        Comma,
        
        Number,
        Identifier,
        
        Plus,
        Minus,
        
        Multiply,
        Division,
        Remainder,
        
        Exponent,
        
        OpenParenthesis,
        ClosedParenthesis,
    }
}
