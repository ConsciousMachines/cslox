using System;
using System.Collections.Generic;
using System.Text;

namespace Lox
{
    interface LoxCallable
    {
        int arity();
        object call(Interpreter interpreter, List<object> arguments);
        string ToString() { return "<native fn>"; }
    }

    public class native_clock : LoxCallable
    {
        public int arity() => 0;
        public object call(Interpreter interpreter, List<object> arguments) =>
            DateTimeOffset.Now.ToUnixTimeSeconds();
    }

    public class native_sine : LoxCallable
    {
        public int arity() => 1;
        public object call(Interpreter interpreter, List<object> arguments) =>
            Math.Sin((double)arguments[0]);
    }
}
