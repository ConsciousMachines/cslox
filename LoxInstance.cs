using System;
using System.Collections.Generic;
using System.Text;

namespace Lox
{
    public class LoxInstance
    {
        private LoxClass klass;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        public LoxInstance(LoxClass klass)
        {
            this.klass = klass;
        }

        public override string ToString()
        {
            return klass.name + " instance";
        }

        public object get(Token name)
        {
            if (fields.ContainsKey(name.lexeme))
            {
                return fields[name.lexeme];
            }

            LoxFunction method = klass.findMethod(name.lexeme);
            if (method != null) return method.bind(this);

            throw new RuntimeError(name, "Undefined property '" + name.lexeme + "'.");
        }

        

        public void set(Token name, Object value)
        {
            fields[name.lexeme] = value;
        }
    }
}
