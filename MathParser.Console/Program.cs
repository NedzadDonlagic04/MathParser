using MathParser.Core;

namespace MathParser.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Parser parser = new();
            parser.Constants.Add("pi", Math.PI);
            parser.Functions.Add("square", new MathFunction(new string[] { "x" }, "x*x"));
            parser.Functions.Add("add", new MathFunction(new string[] { "x", "y" }, "x + y"));

            while (true)
            {
                Console.Write("Enter an expression: ");
                string? userExpression = Console.ReadLine();

                if (userExpression == null)
                {
                    Console.WriteLine("Nothing to parse :(");
                    return;
                }

                try 
                {
                    double result = parser.Parse(userExpression);
                    Console.WriteLine($"{userExpression} = {result}");
                } 
                catch(Exception ex)
                {
                    Console.WriteLine("An error occured");
                    Console.WriteLine(ex.Message);
                }

                Console.WriteLine();
            }
        }
    }
}