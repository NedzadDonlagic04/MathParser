using System.Text.RegularExpressions;

namespace MathParser.Core
{
    public class MathFunction
    {
        private readonly string[] _argNames;
        
        private readonly string ArgNamePattern = Tokenizer.TokenPatterns[TokenType.Identifier];

        private readonly string _functionBody;

        /// <param name="argNames">An array of argument names</param>
        /// <param name="functionBody">The expression which represents the function body</param>
        /// <exception cref="MathFunctionException">
        ///     Thrown when the argument names are not in the expected format
        ///     or by repeating the same identifier more than once
        /// </exception>
        /// <inheritdoc cref="Regex.Match"/>
        public MathFunction(string[] argNames, string functionBody)
        {
            if (argNames.Distinct().Count() != argNames.Length)
            {
                throw new MathFunctionException("Function arguments have repeating identifiers");
            }

            foreach (var argName in argNames)
            {
                Match argNameMatch = Regex.Match(argName, ArgNamePattern);

                if (argNameMatch.Value.Length != argName.Length)
                {
                    throw new MathFunctionException($"Invalid identifier character {argName[argNameMatch.Value.Length]} encountered");
                }

                Match argNameAppearsInBodyMatch = Regex.Match(functionBody, argName);

                if (!argNameAppearsInBodyMatch.Success)
                {
                    throw new MathFunctionException($"Argument \"{argName}\" never used in function body");
                }
            }

            _argNames = argNames;
            _functionBody = functionBody;
        }

        /// <summary>
        ///     Calls a parser with the given args and returns the value from the parsing
        /// </summary>
        /// <param name="args">An array of argument values</param>
        /// <returns>The result of evaluating the expression that's the body of the function</returns>
        /// <exception cref="MathFunctionException">
        ///     Thrown when the amount of arguments passed does not match the amount that
        ///     was set in the constructor
        /// </exception>
        /// <inheritdoc cref="Parser.Parse"/>
        public double Call(double[] args)
        {
            if (args.Length != _argNames.Length)
            {
                throw new MathFunctionException($"Expected {_argNames.Length} arguments for function but got {args.Length}");
            }

            // I think this won't throw an exception ever because the constructor
            // ensures that the format of identifier is correct and not repeating
            // (Keyword here being "I think")
            Parser parser = new();
            for (int i = 0; i < args.Length; ++i)
            {
                parser.Constants.Add(_argNames[i], args[i]);
            }

            return parser.Parse(_functionBody);
        }
    }
}
