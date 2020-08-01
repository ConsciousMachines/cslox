using System;
using System.Collections.Generic;
using System.Text;

namespace Lox
{
    public class RuntimeError : Exception
    {
        public readonly Token token;
        public readonly string message;

        public RuntimeError(Token token, string message) : base(message)
        {
            this.message = message;
            this.token = token;
        }
    }
}
