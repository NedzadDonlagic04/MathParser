namespace MathParser.Core
{
    public class Parser
    {
        private readonly Tokenizer _tokenizer = new();

        private Queue<Token> _tokens = null!;
        
        public readonly Dictionary<string, double> Constants = new();

        public readonly Dictionary<string, MathFunction> Functions = new();

        /// <summary>
        ///     Removes token from the front of queue and returns it
        /// </summary>
        /// <returns>Token at front of queue</returns>
        /// <exception cref="ParserException">
        ///     Thrown when attempint to access the front of the queue when it's empty
        /// </exception>
        private Token EatToken()
        {
            if (_tokens.Count == 0)
            {
                throw new ParserException("Expected token to eat but all tokens have been used up");
            }

            return _tokens.Dequeue();
        }

        /// <summary>
        ///     Returns the token at the front of the queue
        /// </summary>
        /// <returns>Token at front of queue</returns>
        /// <exception cref="ParserException">
        ///     Thrown when attempint to access the front of the queue when it's empty
        /// </exception>
        private Token PeekToken()
        {
            if (_tokens.Count == 0)
            {
                throw new ParserException("Expected token to peek but all tokens have been used up");
            }

            return _tokens.Peek();
        }

        /// <summary>
        ///     Eats a token and compares it to expectedType, if they do not match an exception is thrown
        /// </summary>
        /// <param name="expectedType">The expected token type</param>
        /// <exception cref="ParserException">
        ///     Thrown when the expected token is not at the front of the queue
        /// </exception>
        /// <inheritdoc cref="EatToken"/>
        private void ExpectToken(TokenType expectedType)
        {
            TokenType receivedType = EatToken().Type;

            if (receivedType != expectedType)
            {
                throw new ParserException($"Expected token type \"{expectedType}\" but received \"{receivedType}\"");
            }
        }

        /// <summary>
        ///     Returns the precedence of each token type which helps when parsing
        ///     to make sure the order is correct
        /// </summary>
        /// <param name="token">The token for which the binding power is being searched</param>
        /// <exception cref="NotImplementedException">
        ///     Thrown when the given token type doesn't have a binding power defined for it
        /// </exception>
        private static TokenBindingPower GetTokenBindingPower(Token token) => token.Type switch
        {
            TokenType.EndOfExpression => TokenBindingPower.Minimum,
            TokenType.Comma => TokenBindingPower.Minimum,
            
            TokenType.Identifier => TokenBindingPower.Identifier,
            TokenType.Number => TokenBindingPower.Number,

            TokenType.Plus => TokenBindingPower.Additive,
            TokenType.Minus => TokenBindingPower.Additive,
            
            TokenType.Multiply => TokenBindingPower.Multiplicative,
            TokenType.Division => TokenBindingPower.Multiplicative,
            TokenType.Remainder => TokenBindingPower.Multiplicative,
            
            TokenType.Exponent => TokenBindingPower.Exponential,
            
            TokenType.OpenParenthesis => TokenBindingPower.Parenthesis,
            TokenType.ClosedParenthesis => TokenBindingPower.Minimum,
            
            _ => throw new NotImplementedException($"Token binding power does not exist for token \"{token}\"")
        };

        /// <summary>
        ///     Tries to parse the token into a double
        /// </summary>
        /// <param name="token">Token to parse</param>
        /// <returns>The parsed token as a number</returns>
        /// <inheritdoc cref="double.Parse"/>
        private static double ParseNumber(Token token) => double.Parse(token.Value);

        /// <summary>
        ///     Takes the unary operator token and attempts to parse
        ///     the following expression with it
        /// </summary>
        /// <param name="prefixToken">The given unary operator token</param>
        /// <returns>The parsed unary expression</returns>
        /// <exception cref="NotImplementedException">
        ///     Thrown when the given token is not a valid unary operator
        /// </exception>
        /// <inheritdoc cref="ParseExpression"/>
        private double ParseUnaryExpression(Token prefixToken) => prefixToken.Type switch
        {
            TokenType.Plus => ParseExpression(TokenBindingPower.Unary),
            TokenType.Minus => -ParseExpression(TokenBindingPower.Unary),
            _ => throw new NotImplementedException($"\"{prefixToken}\" is not a defined unary operator")
        };

        /// <summary>
        ///     Attempts to parses the expression inside the parenthesis
        /// </summary>
        /// <param name="_">Ignored</param>
        /// <returns>Parse parenthesis xxpression</returns>
        /// <inheritdoc cref="ParseExpression"/>
        /// <inheritdoc cref="ExpectToken"/>
        private double ParseParenthesisExpression(Token _)
        {
            double value = ParseExpression();
            ExpectToken(TokenType.ClosedParenthesis);

            return value;
        }

        /// <summary>
        ///     Attempts to parse a given token into a constant based on the 
        ///     Constants dictionary inside the ParserContext instance
        /// </summary>
        /// <param name="token">The given constant identifier token</param>
        /// <returns>The parsed context</returns>
        /// <exception cref="ParserException">
        ///     Thrown when there isn't a constant for the given identifier
        /// </exception>
        private double ParseConstant(Token token)
        {
            string constantName = token.Value;

            if (!Constants.ContainsKey(constantName))
            {
                throw new ParserException($"Constant \"{constantName}\" does not exist");
            }

            return Constants[constantName];
        }

        /// <summary>
        ///     Takes the token at the front of the queue and attempts to convert it to a 
        ///     double and add it to the args stack
        /// </summary>
        /// <param name="args">Stack of arguments</param>
        /// <inheritdoc cref="EatToken"/>
        /// <inheritdoc cref="ParseNumber"/>
        /// <inheritdoc cref="ParseIdentifier"/>
        private void AddArg(Stack<double> args)
        {
            Token token = EatToken();   // Eat a token that (hopefully) can be parsed into a number
            args.Push((token.Type == TokenType.Number) ? ParseNumber(token) : ParseIdentifier(token));
        }

        /// <summary>
        ///     Takes the next tokens (if any exists) and tries to convert them to an array
        ///     of args
        /// </summary>
        /// <returns>An array of arguments to be used by a MathFunction instance</returns>
        /// <inheritdoc cref="PeekToken"/>
        /// <inheritdoc cref="AddArg"/>
        /// <inheritdoc cref="EatToken"/>
        private double[] ParseFunctionArgs()
        {
            Stack<double> args = new();

            if (PeekToken().Type == TokenType.Number || PeekToken().Type == TokenType.Identifier)
            {
                AddArg(args);
                while (PeekToken().Type == TokenType.Comma)
                {
                    EatToken();                 // Eat the comma
                    AddArg(args);
                }
            }

            return args.ToArray();
        }

        /// <summary>
        ///     Takes a function identifier and tries to find it's definition in the
        ///     Functions dictionary, afterwhich it will try to take the args for the
        ///     function and call it
        /// </summary>
        /// <param name="token">Token that (should) be a valid function identifier</param>
        /// <returns>The result of evaluating the called function</returns>
        /// <exception cref="ParserException"></exception>
        /// <inheritdoc cref="ExpectToken"/>
        /// <inheritdoc cref="ParseFunctionArgs"/>
        /// <inheritdoc cref="MathFunction.Call"/>
        private double ParseFunction(Token token)
        {
            ExpectToken(TokenType.OpenParenthesis);

            string functionName = token.Value;

            if (!Functions.ContainsKey(functionName))
            {
                throw new ParserException($"Function \"{functionName}\" does not exist");
            }

            double[] args = ParseFunctionArgs(); 

            ExpectToken(TokenType.ClosedParenthesis);
            return Functions[functionName].Call(args);
        }

        /// <summary>
        ///     Tries to parse the given token identifier as a function or constant
        /// </summary>
        /// <param name="token">Token that should be a valid identifier</param>
        /// <returns>Parsed value either from a constant or function evaluation</returns>
        private double ParseIdentifier(Token token)
            => (PeekToken().Type == TokenType.OpenParenthesis) ? ParseFunction(token) : ParseConstant(token);

        /// <summary>
        ///     NUD -> NUll Denotation
        ///     If it returns a function, it should be able to parse a token and expect nothing
        ///     to the left of it
        ///     Number expressions would all have a NUD (in my case it just returns that number)
        ///     but it can also be used with unary expressions, do note even then it doesn't expect
        ///     anything to the left of it, take the expression -5, the - expects nothing to the left
        ///     of it. At least in this context.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///     Thrown when given a token type that doesn't have a defined value for it's NUD handler
        /// </exception>
        /// <inheritdoc cref="ParseNumber"/>
        /// <inheritdoc cref="ParseIdentifier"/>
        /// <inheritdoc cref="ParseUnaryExpression"/>
        /// <inheritdoc cref="ParseParenthesisExpression"/>
        private Func<Token, double>? GetNUDHandler(Token token) => token.Type switch
        {
            TokenType.EndOfExpression => null,
            TokenType.Comma => null,

            TokenType.Number => ParseNumber,
            TokenType.Identifier => ParseIdentifier,
            
            TokenType.Plus => ParseUnaryExpression,
            TokenType.Minus => ParseUnaryExpression,
            
            TokenType.Multiply => null,
            TokenType.Division => null,
            TokenType.Remainder => null,
            
            TokenType.Exponent => null,
            
            TokenType.OpenParenthesis => ParseParenthesisExpression,
            TokenType.ClosedParenthesis => null,
            
            _ => throw new NotImplementedException($"\"{token}\" does not have NUD handler")
        };

        /// <summary>
        ///     Takes the left hand side value of the operator and tries to 
        ///     evaluate it using the right hand side through recursively
        ///     passing the expression
        /// </summary>
        /// <param name="lhs">Value on the left hand side of the binary operator</param>
        /// <returns>The parsed binary expression</returns>
        /// <exception cref="NotImplementedException">
        ///     Thrown when the given token is not a valid binary operator
        /// </exception>
        /// <inheritdoc cref="ParseExpression"/>
        /// <inheritdoc cref="GetTokenBindingPower"/>
        /// <inheritdoc cref="EatToken"/>
        private double ParseBinaryExpression(double lhs)
        {
            Token operatorToken = EatToken();

            return operatorToken.Type switch
            {
                TokenType.Plus => lhs + ParseExpression(GetTokenBindingPower(operatorToken)),
                TokenType.Minus => lhs - ParseExpression(GetTokenBindingPower(operatorToken)),
                TokenType.Multiply => lhs * ParseExpression(GetTokenBindingPower(operatorToken)),
                TokenType.Division => lhs / ParseExpression(GetTokenBindingPower(operatorToken)),
                TokenType.Remainder => lhs % ParseExpression(GetTokenBindingPower(operatorToken)),
                _ => throw new NotImplementedException($"\"{operatorToken}\" is not a defined binary operator")
            };
        }

        /// <summary>
        ///     Exponents given base (lhs) with exponent which will be evaluated
        ///     through parsing the expression recursively
        /// </summary>
        /// <param name="lhs">Base used for exponenting</param>
        /// <returns>An evaluated exponent expression</returns>
        /// <inheritdoc cref="ExpectToken"/>
        /// <inheritdoc cref="ParseExpression"/>
        private double ParseExponentExpression(double lhs)
        {
            ExpectToken(TokenType.Exponent);

            return Math.Pow(lhs, ParseExpression(TokenBindingPower.Exponential));
        }

        /// <summary>
        ///     LED -> LEft Denotation
        ///     Generally used for binary operations like multiplication (*) and division (/).
        ///     Where if you have an expression * 8 it doesn't work because * expects something
        ///     to the left of it.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///     Thrown when given a token type that doesn't have a defined value for it's LED handler
        /// </exception>
        /// <inheritdoc cref="ParseBinaryExpression"/>
        /// <inheritdoc cref="ParseExponentExpression"/>
        private Func<double, double>? GetLEDHandler(Token token) => token.Type switch
        {
            TokenType.EndOfExpression => null,
            TokenType.Comma => null,

            TokenType.Number => null,
            TokenType.Identifier => null,
            
            TokenType.Plus => ParseBinaryExpression,
            TokenType.Minus => ParseBinaryExpression,
            
            TokenType.Multiply => ParseBinaryExpression,
            TokenType.Division => ParseBinaryExpression,
            TokenType.Remainder => ParseBinaryExpression,
            
            TokenType.Exponent => ParseExponentExpression,
            
            TokenType.OpenParenthesis => null,
            TokenType.ClosedParenthesis => null,
            
            _ => throw new NotImplementedException($"\"{token}\" does not have LED handler")
        };

        /// <summary>
        ///     Method intended to be used with ParseExpression's loop
        ///     which allows us to parse the order of operations correctly
        /// </summary>
        /// <param name="minBindingPower">Minimun binding power</param>
        /// <returns>True if tokens queue isn't empty and the next token is bigger than the minimum</returns>
        /// <inheritdoc cref="GetTokenBindingPower"/>
        /// <inheritdoc cref="PeekToken"/>
        private bool CurrentBindingPowerBiggerThanMin(TokenBindingPower minBindingPower)
            => _tokens.Count != 0 && GetTokenBindingPower(PeekToken()) > minBindingPower;

        /// <summary>
        ///     Method which will through loops and recursion parse
        ///     the expression
        /// </summary>
        /// <param name="minBindingPower">Minimum binding power</param>
        /// <returns>Result of parsing the expression</returns>
        /// <exception cref="ParserException">
        ///     Thrown if there is no NUD or LED handler for a token
        /// </exception>
        /// <inheritdoc cref="EatToken"/>
        /// <inheritdoc cref="GetNUDHandler"/>
        /// <inheritdoc cref="GetLEDHandler"/>
        /// <inheritdoc cref="CurrentBindingPowerBiggerThanMin"/>
        private double ParseExpression(TokenBindingPower minBindingPower = TokenBindingPower.Minimum)
        {
            Token token = EatToken();

            var nudHandler = GetNUDHandler(token);

            if (nudHandler == null)
            {
                throw new ParserException($"Expected number or unary operator but received \"{token}\"");
            }

            double lhs = nudHandler(token);

            while (CurrentBindingPowerBiggerThanMin(minBindingPower))
            {
                var ledHandler = GetLEDHandler(PeekToken());

                if (ledHandler == null)
                {
                    throw new ParserException($"\"{PeekToken().Value}\" is not a valid binary or exponent operator");
                }

                lhs = ledHandler(lhs);
            }

            return lhs;
        }

        /// <summary>
        ///     Parses the math expression and return it's value after parsing
        /// </summary>
        /// <param name="expression">The math expression to parse</param>
        /// <returns>Result of parsing the entire expression</returns>
        /// <exception cref="ParserException">
        ///     Thrown when the expression is an empty string
        /// </exception>
        /// <inheritdoc cref="Tokenizer.Tokenize"/>
        /// <inheritdoc cref="ParseExpression"/>
        /// <inheritdoc cref="ExpectToken"/>
        public double Parse(string expression)
        {
            if (expression.Length == 0)
            {
                throw new ParserException("Given expression is empty");
            }

            _tokens = _tokenizer.Tokenize(expression);

            double result = ParseExpression();

            ExpectToken(TokenType.EndOfExpression);

            return result;
        }
    }
}