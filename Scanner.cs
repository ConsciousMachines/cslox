using System;
using System.Collections.Generic;
using System.Text;
using static Lox.TokenType;

namespace Lox
{
    public class Scanner
    {
        private static readonly Dictionary<string, TokenType> keywords =
            new Dictionary<string, TokenType>()
            {
                {"and", AND },
                {"class", CLASS},
                {"else", ELSE},
                {"false", FALSE},
                {"for", FOR},
                {"fun", FUN},
                {"if", IF},
                {"nil", NIL},
                {"or", OR},
                {"print", PRINT},
                {"return", RETURN},
                {"super", SUPER},
                {"this", THIS},
                {"true", TRUE},
                {"var", VAR},
                {"while", WHILE},
            };


        private readonly string source;
        private readonly List<Token> tokens = new List<Token>();
        private int start = 0;
        private int current = 0;
        private int line = 1;

        public Scanner(string source)
        {
            this.source = source;

        }

        public List<Token> scanTokens()
        {
            while (!isAtEnd())
            {
                // We are at the beginning of the next lexeme.
                start = current;
                scanToken();
            }

            tokens.Add(new Token(EOF, "", null, line));
            return tokens;
        }

        private void scanToken()
        {
            char c = advance();
            switch (c)
            {
                case '(': addToken(LEFT_PAREN); break;
                case ')': addToken(RIGHT_PAREN); break;
                case '{': addToken(LEFT_BRACE); break;
                case '}': addToken(RIGHT_BRACE); break;
                case ',': addToken(COMMA); break;
                case '.': addToken(DOT); break;
                case '-': addToken(MINUS); break;
                case '+': addToken(PLUS); break;
                case ';': addToken(SEMICOLON); break;
                case '*': addToken(STAR); break;
                case '!': addToken(match('=') ? BANG_EQUAL : BANG); break;
                case '=': addToken(match('=') ? EQUAL_EQUAL : EQUAL); break;
                case '<': addToken(match('=') ? LESS_EQUAL : LESS); break;
                case '>': addToken(match('=') ? GREATER_EQUAL : GREATER); break;
                case '/':
                    if (match('/'))
                    {
                        // A comment goes until the end of the line.
                        while (peek() != '\n' && !isAtEnd()) advance();
                    }
                    else
                    {
                        addToken(SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;

                case '\n':
                    line++;
                    break;
                case '"': _string(); break;
                default:
                    if (isDigit(c))
                    {
                        number();
                    }
                    else if (isAlpha(c))
                    {
                        identifier();
                    }
                    else
                    {
                        throw new Exception("soy moment");
                        //Lox.error(line, "Unexpected character.");
                    }
                    break;
            }
        }

        private void identifier()
        {
            while (isAlphaNumeric(peek())) advance();

            // See if the identifier is a reserved word.
            string text = source.Substring(start, current - start); // second argument is length in Csharp
            TokenType type;
            if (!keywords.ContainsKey(text))
                type = TokenType.IDENTIFIER;
            else
                type = keywords[text];
            addToken(type);
        }

        private void number()
        {
            while (isDigit(peek())) advance();

            // Look for a fractional part.
            if (peek() == '.' && isDigit(peekNext()))
            {
                // Consume the "."
                advance();

                while (isDigit(peek())) advance();
            }

            addToken(NUMBER, Double.Parse(source.Substring(start, current - start)));
        }

        private void _string()
        {
            while (peek() != '"' && !isAtEnd())
            {
                if (peek() == '\n') line++;
                advance();
            }

            // Unterminated string.
            if (isAtEnd())
            {
                throw new Exception("soy moment");
                //Lox.error(line, "Unterminated string.");
                return;
            }

            // The closing ".
            advance();

            // Trim the surrounding quotes.
            string value = source.Substring(start + 1, current - start - 2);
            addToken(STRING, value);
        }

        private bool match(char expected) // i think this is same as "peek" or peek ahead
        {
            if (isAtEnd()) return false;
            if (source[current] != expected) return false;

            current++; // only moves ahead if the expectation char is met. 
            return true;
        }

        private char peek() // like advance, but doesnt consume char
        {
            if (isAtEnd()) return '\0';
            return source[current];
        }

        private char peekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }

        private bool isAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c == '_';
        }

        private bool isAlphaNumeric(char c)
        {
            return isAlpha(c) || isDigit(c);
        }

        private bool isDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private bool isAtEnd()
        {
            return current >= source.Length;
        }

        private char advance()
        {
            current++;
            return source[current - 1];
        }

        private void addToken(TokenType type)
        {
            addToken(type, null);
        }

        private void addToken(TokenType type, object literal)
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }
    }
}
