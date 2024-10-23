namespace MathParser.Core
{
    internal enum TokenBindingPower
    {
        Minimum,
        Identifier,
        Number,        // Stuff like Number, if string literals ever get added could use this same thing
        Additive,
        Multiplicative,
        Exponential,
        Unary,
        Parenthesis,
        Maximum,
    }
}
