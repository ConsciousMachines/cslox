using System;
using System.Collections.Generic;
using System.Text;

namespace Lox
{
    public class Environment
    {
        public readonly Environment enclosing;
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public Environment() => this.enclosing = null;

        public Environment(Environment enclosing) => this.enclosing = enclosing;

        public object get(Token name)
        {
            if (values.ContainsKey(name.lexeme))
            {
                return values[name.lexeme];
            }

            if (enclosing != null) return enclosing.get(name); // go up chain

            throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
        }

        public object getAt(int distance, string name)
        {
            return ancestor(distance).values[name];
        }

        public Environment ancestor(int distance)
        {
            Environment environment = this;
            for (int i = 0; i < distance; i++)
            {
                environment = environment.enclosing;
            }

            return environment;
        }

        public void assignAt(int distance, Token name, object value)
        {
            ancestor(distance).values[name.lexeme] = value;
        }

        public void define(string name, object value) => values[name] = value;

        public void assign(Token name, object value)
        {
            if (values.ContainsKey(name.lexeme))
            {
                values[name.lexeme] = value;
                return;
            }

            if (enclosing != null)
            {
                enclosing.assign(name, value);
                return;
            }

            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }
    }
}
