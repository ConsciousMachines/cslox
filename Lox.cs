using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualBasic.CompilerServices;

namespace Lox
{
    public class Lox
    {
        static bool hadError = false;
        static bool hadRuntimeError = false;
        private static readonly Interpreter interpreter = new Interpreter();

        public static void Main(string[] args)
        {
            /* E X P R 
            GenerateAst.defineAst("", "Expr", new List<string>()
            {
                "Assign   : Token name, Expr value",
                "Binary   : Expr left, Token _operator, Expr right",
                "Call     : Expr callee, Token paren, List<Expr> arguments",
                "Get      : Expr _object, Token name",
                "Grouping : Expr expression",
                "Literal  : Object value",
                "Logical  : Expr left, Token _operator, Expr right",
                "Set      : Expr _object, Token name, Expr value",
                "Super    : Token keyword, Token method",
                "This     : Token keyword",
                "Unary    : Token _operator, Expr right",
                "Variable : Token name"
            });
            */

            /* S T M T 
            GenerateAst.defineAst("", "Stmt", new List<string>()
            {
                "Block      : List<Stmt> statements",
                "Class      : Token name, Expr.Variable superclass, List<Stmt.Function> methods",
                "Expression : Expr expression",
                "Function   : Token name, List<Token> _params, List<Stmt> body",
                "If         : Expr condition, Stmt thenBranch, Stmt elseBranch",
                "Print      : Expr expression",
                "Return     : Token keyword, Expr value",
                "Var        : Token name, Expr initializer",
                "While      : Expr condition, Stmt body"
            });
            */

            Console.WriteLine("Please set the directory of the script file in the main Lox.cs file in function 'Main'!");
            

            runFile(@"C:\Users\pwnag\source\repos\Lox\test.lox");

            if (args.Length > 1)
            {
                Console.WriteLine("Usage: jlox [script]");
                System.Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                runFile(args[0]);
            }
            else
            {
                runPrompt();
            }
        }

        private static void runFile(string path)
        {
            string bytes = File.ReadAllText(path);
            run(bytes);

            // Indicate an error in the exit code.
            if (hadError) System.Environment.Exit(65);
            if (hadRuntimeError) System.Environment.Exit(70);
        }

        private static void runPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                if (line == null) break; // this doesnt work. Ctrl-C exits tho
                run(line);
                hadError = false;
            }
        }

        private static void run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.scanTokens();
            Parser parser = new Parser(tokens);
            List<Stmt> statements = parser.parse();

            // Stop if there was a syntax error.
            if (hadError) return;

            Resolver resolver = new Resolver(interpreter);
            resolver.resolve(statements);

            // Stop if there was a resolution error.
            if (hadError) return;

            //Console.WriteLine(new AstPrinter().print(expression));
            interpreter.interpret(statements);
        }

        public static void error(Token token, String message)
        {
            if (token.type == TokenType.EOF)
            {
                report(token.line, " at end", message);
            }
            else
            {
                report(token.line, " at '" + token.lexeme + "'", message);
            }
        }

        public static void runtimeError(RuntimeError error)
        {
            Console.WriteLine($"{error.message}\n[line {error.token.line}]");
            hadRuntimeError = true;
        }

        private static void report(int line, string where, string message)
        {
            Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
            hadError = true;
        }
    }
}
