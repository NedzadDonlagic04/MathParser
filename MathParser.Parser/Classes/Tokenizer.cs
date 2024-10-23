using System.Collections.ObjectModel;

using System.Text;
using System.Text.RegularExpressions;

namespace MathParser.Core
{
    internal class Tokenizer
    {
        private StringBuilder _source = null!;

        private readonly Queue<Token> _tokens = new();

        internal static readonly ReadOnlyDictionary<TokenType, string> TokenPatterns = new Dictionary<TokenType, string>() {
            { TokenType.Comma, @"^," },
            
            { TokenType.Number, @"^\d+(\.\d+)?" },
            { TokenType.Identifier, @"^[a-zA-Z_](\w+)?" },
            
            { TokenType.Plus, @"^\+" },
            { TokenType.Minus, @"^-" },
            
            { TokenType.Multiply, @"^\*" },
            { TokenType.Division, @"^\/" },
            { TokenType.Remainder, @"^%" },
            
            { TokenType.Exponent, @"^\^" },
            
            { TokenType.OpenParenthesis, @"^\(" },
            { TokenType.ClosedParenthesis, @"^\)" },
        }.AsReadOnly();

        /// <summary>
        ///     Creates a new StringBuilder instance and clears tokens in the queue
        /// </summary>
        /// <param name="source">The string that will be tokenized</param>
        private void ResetTokenizer(string source) 
        {
            _source = new StringBuilder(source);
            _tokens.Clear();
        }

        /// <summary>
        ///     As the name says it removes leading white spaces if any exist
        /// </summary>
        /// <inheritdoc cref="StringBuilder.Remove"/>
        /// <inheritdoc cref="Regex.Match"/>
        private void RemoveLeadingSpaces()
        {
            string spacesRegex = @"^\s+";

            Match match = Regex.Match(_source.ToString(), spacesRegex);

            _source.Remove(0, match.Value.Length);
        }

        private bool HasTokens() => _source.Length != 0;

        /// <summary>
        ///     Returns the next token using the _source field
        /// </summary>
        /// <returns>The next token</returns>
        /// <exception cref="TokenizerException">Thrown when an unexpected token pattern is encountered</exception>
        /// <inheritdoc cref="RemoveLeadingSpaces"/>
        private Token GetNextToken()
        {
            foreach (var (tokenType, pattern) in TokenPatterns) 
            {
                RemoveLeadingSpaces();

                Match match = Regex.Match(_source.ToString(), pattern);

                if (match.Success)
                {
                    _source.Remove(0, match.Value.Length);
                    return new Token(tokenType, match.Value);
                }
            }

            throw new TokenizerException($"Unknown pattern encountered -> {_source}");
        }

        /// <summary>
        ///     Takes a string and returns a queue of tokens
        /// </summary>
        /// <param name="source">Expression to tokenize</param>
        /// <returns>A queue of tokens after tokenizing source</returns>
        /// <inheritdoc cref="GetNextToken"/>
        public Queue<Token> Tokenize(string source)
        {
            ResetTokenizer(source);

            while (HasTokens())
            {
                _tokens.Enqueue(GetNextToken());
            }
            _tokens.Enqueue(new Token(TokenType.EndOfExpression, ""));

            return _tokens;
        }
    }
}