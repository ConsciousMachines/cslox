using System;
using System.Collections.Generic;
using System.Text;

namespace Lox
{
    public class LoxClass : LoxCallable 
    {
        public readonly string name;
        private readonly Dictionary<string, LoxFunction> methods;
        public readonly LoxClass superclass;

        public LoxClass(String name, LoxClass superclass,
           Dictionary<string, LoxFunction> methods)
        {
            this.superclass = superclass;
            this.name = name;
            this.methods = methods;
        }

        public override string ToString()
        {
            return name;
        }

        public int arity()
        {
            LoxFunction initializer = findMethod("init");
            if (initializer == null) return 0;
            return initializer.arity();
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            LoxInstance instance = new LoxInstance(this);
            LoxFunction initializer = findMethod("init");
            if (initializer != null)
            {
                initializer.bind(instance).call(interpreter, arguments);
            }
            return instance;
        }

        public LoxFunction findMethod(string name)
        {
            if (methods.ContainsKey(name))
            {
                return methods[name];
            }

            if (superclass != null)
            {
                return superclass.findMethod(name);
            }

            return null;
        }
    }
}
