using System;
using System.Collections.Generic;
using System.Text;

namespace Lox
{
    public class LoxFunction : LoxCallable
    {
        private readonly Environment closure; // a closure is like a snapshot in time of all the variables that existed in the scope when the fn was declared. 
        private readonly Stmt.Function declaration;
        private readonly bool isInitializer;

        public LoxFunction(Stmt.Function declaration, Environment closure,
                    bool isInitializer)
        {
            this.isInitializer = isInitializer;
            this.closure = closure;
            this.declaration = declaration;
        }
        public object call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(closure);
            for (int i = 0; i < declaration._params.Count; i++)
            {
                environment.define(declaration._params[i].lexeme,
                    arguments[i]);
            }

            try
            {
                interpreter.executeBlock(declaration.body, environment);
            }
            catch (Return returnValue)
            {
                if (isInitializer) return closure.getAt(0, "this");

                return returnValue.value;
            }

            if (isInitializer) return closure.getAt(0, "this");
            return null;
        }

        public LoxFunction bind(LoxInstance instance)
        {
            Environment environment = new Environment(closure);
            environment.define("this", instance);
            return new LoxFunction(declaration, environment, isInitializer);
        }

        public int arity() => declaration._params.Count;
        public override string ToString() => $"<fn {declaration.name.lexeme}>";
    }
}
