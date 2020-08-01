using System;
using System.Collections.Generic;
using System.Text;
using static Lox.TokenType;

namespace Lox
{
    public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        public readonly Environment globals = new Environment();
        private Environment environment;
        private readonly Dictionary<Expr, int> locals = new Dictionary<Expr, int>();

        public Interpreter()
        {
            environment = globals;
            globals.define("clock", new native_clock());
            globals.define("sin", new native_sine());
        }

        public void interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                Lox.runtimeError(error);
            }
        }

        public void resolve(Expr expr, int depth)
        {
            locals[expr] = depth;
        }

        private object lookUpVariable(Token name, Expr expr)
        {
            
            if (locals.ContainsKey(expr))
            {
                int distance = locals[expr];
                return environment.getAt(distance, name.lexeme);
            }
            else
            {
                return globals.get(name);
            }
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = evaluate(expr.right);

            switch (expr._operator.type)
            {
                case MINUS:
                    checkNumberOperand(expr._operator, right);
                    return -(double)right;
                case BANG:
                    return !isTruthy(right);
            }

            // Unreachable.
            return null;
        }

        private void checkNumberOperand(Token _operator, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(_operator, "Operand must be a number.");
        }

        private void checkNumberOperands(Token _operator,
                                   object left, object right)
        {
            if (left is double && right is double) return;

            throw new RuntimeError(_operator, "Operands must be numbers.");
        }

        private bool isTruthy(object _object)
        {
            if (_object == null) return false;
            if (_object is bool) return (bool)_object;
            return true;
        }

        private bool isEqual(object a, object b)
        {
            // nil is only equal to nil.
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b);
        }

        private string stringify(object _object)
        {
            if (_object == null) return "nil";

            // Hack. Work around Java adding ".0" to integer-valued doubles.
            if (_object is double)
            {
                string text = _object.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }

            return _object.ToString();
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return evaluate(expr.expression);
        }

        private object evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        private void execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            evaluate(stmt.expression);
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            object value = evaluate(stmt.expression);
            Console.WriteLine(stringify(value));
            return null;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = evaluate(expr.left);
            object right = evaluate(expr.right);

            switch (expr._operator.type)
            {
                case BANG_EQUAL: return !isEqual(left, right);
                case EQUAL_EQUAL: return isEqual(left, right);
                case GREATER:
                    checkNumberOperands(expr._operator, left, right);
                    return (double)left > (double)right;
                case GREATER_EQUAL:
                    checkNumberOperands(expr._operator, left, right);
                    return (double)left >= (double)right;
                case LESS:
                    checkNumberOperands(expr._operator, left, right);
                    return (double)left < (double)right;
                case LESS_EQUAL:
                    checkNumberOperands(expr._operator, left, right);
                    return (double)left <= (double)right;
                case MINUS:
                    checkNumberOperands(expr._operator, left, right);
                    return (double)left - (double)right;
                case SLASH:
                    checkNumberOperands(expr._operator, left, right);
                    return (double)left / (double)right;
                case STAR:
                    checkNumberOperands(expr._operator, left, right);
                    return (double)left * (double)right;
                case PLUS:
                    if (left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }
                    if (left is string && right is string)
                    {
                        return (string)left + (string)right;
                    }
                    throw new RuntimeError(expr._operator, "Operands must be two numbers or two strings.");
            }

            // Unreachable.
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            //return environment.get(expr.name);
            return lookUpVariable(expr.name, expr);
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if (stmt.initializer != null)
            {
                value = evaluate(stmt.initializer);
            }

            environment.define(stmt.name.lexeme, value);
            return null;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = evaluate(expr.value);

            //environment.assign(expr.name, value);

            
            if (locals.ContainsKey(expr))
            {
                int distance = locals[expr];
                environment.assignAt(distance, expr.name, value);
            }
            else
            {
                globals.assign(expr.name, value);
            }
            return value;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            executeBlock(stmt.statements, new Environment(environment));
            return null;
        }

        public void executeBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = this.environment; // save to restore later
            try
            {
                this.environment = environment;

                foreach (Stmt statement in statements)
                {
                    execute(statement);
                }
            }
            finally
            {
                this.environment = previous;
            }
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            if (isTruthy(evaluate(stmt.condition)))
            {
                execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null)
            {
                execute(stmt.elseBranch);
            }
            return null;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            object left = evaluate(expr.left);

            if (expr._operator.type == TokenType.OR)
            {
                if (isTruthy(left)) return left;
            }
            else // and clause 
            {
                if (!isTruthy(left)) return left;
            }

            return evaluate(expr.right);
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            while (isTruthy(evaluate(stmt.condition)))
            {
                execute(stmt.body);
            }
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            object callee = evaluate(expr.callee);

            List<object> arguments = new List<object>();
            foreach (Expr argument in expr.arguments)
            {
                arguments.Add(evaluate(argument));
            }

            if (!(callee is LoxCallable))
            {
                throw new RuntimeError(expr.paren, "Can only call functions and classes.");
            }

            LoxCallable function = (LoxCallable)callee;
            if (arguments.Count != function.arity())
            {
                throw new RuntimeError(expr.paren, "Expected " +
                    function.arity() + " arguments but got " +
                    arguments.Count + ".");
            }
            return function.call(this, arguments);
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            LoxFunction function = new LoxFunction(stmt, environment, false);
            environment.define(stmt.name.lexeme, function); // this is really just globals (same ref?)
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            object value = null;
            if (stmt.value != null) value = evaluate(stmt.value);

            throw new Return(value);
        }

        public object VisitClassStmt(Stmt.Class stmt)
        {
            object superclass = null;
            if (stmt.superclass != null)
            {
                superclass = evaluate(stmt.superclass);
                if (!(superclass is LoxClass)) {
                    throw new RuntimeError(stmt.superclass.name,
                        "Superclass must be a class.");
                }
            }

            environment.define(stmt.name.lexeme, null);

            if (stmt.superclass != null)
            {
                environment = new Environment(environment);
                environment.define("super", superclass);
            }

            Dictionary<string, LoxFunction> methods = new Dictionary<string, LoxFunction>();
            foreach (Stmt.Function method in stmt.methods)
            {
                LoxFunction function = new LoxFunction(method, environment,method.name.lexeme == "init");
                methods[method.name.lexeme] = function;
            }

            LoxClass klass = new LoxClass(stmt.name.lexeme, (LoxClass)superclass, methods);

            if (superclass != null)
            {
                environment = environment.enclosing;
            }

            environment.assign(stmt.name, klass);
            return null;
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            object _object = evaluate(expr._object);
            if (_object is LoxInstance) {
                return ((LoxInstance)_object).get(expr.name);
            }

            throw new RuntimeError(expr.name, "Only instances have properties.");
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            object _object = evaluate(expr._object);

            if (!(_object is LoxInstance)) {
                throw new RuntimeError(expr.name, "Only instances have fields.");
            }

            Object value = evaluate(expr.value);
            ((LoxInstance)_object).set(expr.name, value);
            return value;
        }

        public object VisitThisExpr(Expr.This expr)
        {
            return lookUpVariable(expr.keyword, expr);
        }

        public object VisitSuperExpr(Expr.Super expr)
        {
            int distance = locals[expr];
            LoxClass superclass = (LoxClass)environment.getAt(
                distance, "super");

            // "this" is always one level nearer than "super"'s environment.
            LoxInstance _object = (LoxInstance)environment.getAt(
                distance - 1, "this");

            LoxFunction method = superclass.findMethod(expr.method.lexeme);
            if (method == null)
            {
                throw new RuntimeError(expr.method,
                    "Undefined property '" + expr.method.lexeme + "'.");
            }
            return method.bind(_object); // binds the instance, aka 'this'
        }
    }
}
