using System;
using System.Collections.Generic;
using System.Text;

namespace Lox
{
    public class Resolver : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private enum ClassType
        {
            NONE,
            CLASS,
            SUBCLASS
        }


        private enum FunctionType
        {
            NONE,
            FUNCTION,
            METHOD,
            INITIALIZER,
        }

        private ClassType currentClass = ClassType.NONE;
        private FunctionType currentFunction = FunctionType.NONE;
        private readonly Interpreter interpreter;
        private readonly List<Dictionary<string, bool>> scopes = new List<Dictionary<string, bool>>();

        public void resolve(List<Stmt> statements)
        {
            foreach (Stmt statement in statements)
            {
                resolve(statement);
            }
        }

        private void resolve(Stmt stmt)
        {
            stmt.Accept(this); // since Stmt is generic, the derived class will accept this visitor and depending on its type, will summon the proper visit method. 
        }

        private void define(Token name)
        {
            //if (scopes.Count == 0) return;
            //scopes.Peek()[name.lexeme] = true;
            if (scopes.Count < 1)
                return;
            scopes[scopes.Count - 1][name.lexeme] = true;
        }

        private void declare(Token name)
        {
            /*
            if (scopes.Count == 0) return;

            Dictionary<string, bool> scope = scopes.Peek();
            if (scope.ContainsKey(name.lexeme))
            {
                Lox.error(name, "Variable with this name already declared in this scope.");
            }
            scope[name.lexeme] = false;
            */
            if (scopes.Count < 1)
                return;

            Dictionary<string, bool> scope = scopes[scopes.Count - 1];

            if (scope.ContainsKey(name.lexeme))
            {
                Lox.error(name, "Variable with this name already declared in this scope.");
            }

            scope.Add(name.lexeme, false);
        }

        private void resolveLocal(Expr expr, Token name)
        {
            /*
            for (int i = scopes.Count - 1; i >= 0; i--)
            {
                if (scopes.ElementAt(i).ContainsKey(name.lexeme))
                {
                    interpreter.resolve(expr, scopes.Count - 1 - i);
                    return;
                }
            }
            */
            for (int i = scopes.Count - 1; i >= 0; i--)
            {
                if (scopes[i].ContainsKey(name.lexeme)) // Csharp doesn't access Stack<> by index (without resorting to Linq), hence used List<> instead
                {
                    interpreter.resolve(expr, scopes.Count - 1 - i);
                    return;
                }

            }

            // Not found. Assume it is global.
        }

        private void resolveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            beginScope();
            foreach (Token param in function._params)
            {
                declare(param);
                define(param);
            }
            resolve(function.body);
            endScope();
            currentFunction = enclosingFunction;
        }

        private void beginScope() => scopes.Add(new Dictionary<string, bool>());

        private void endScope() => scopes.RemoveAt(scopes.Count - 1);

        private void resolve(Expr expr) => expr.Accept(this);

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            resolve(expr.value);
            resolveLocal(expr, expr.name);
            return null;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            resolve(expr.left);
            resolve(expr.right);
            return null;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            beginScope();
            resolve(stmt.statements);
            endScope();
            return null;
        }


        public object VisitCallExpr(Expr.Call expr)
        {
            resolve(expr.callee);

            foreach (Expr argument in expr.arguments)
            {
                resolve(argument);
            }

            return null;
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            resolve(stmt.expression);
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            declare(stmt.name);
            define(stmt.name);

            resolveFunction(stmt, FunctionType.FUNCTION);
            return null;
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            resolve(expr.expression);
            return null;
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            resolve(stmt.condition);
            resolve(stmt.thenBranch);
            if (stmt.elseBranch != null) resolve(stmt.elseBranch);
            return null;
        }

        public object VisitLiteralExpr(Expr.Literal expr) => null;

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            resolve(expr.left);
            resolve(expr.right);
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            resolve(stmt.expression);
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            if (currentFunction == FunctionType.NONE)
            {
                Lox.error(stmt.keyword, "Cannot return from top-level code.");
            }

            if (stmt.value != null)
            {
                if (currentFunction == FunctionType.INITIALIZER)
                {
                    Lox.error(stmt.keyword,
                        "Cannot return a value from an initializer.");
                }

                resolve(stmt.value);
            }

            return null;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            resolve(expr.right);
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            //if (!(scopes.Count == 0) && (scopes.Peek()[expr.name.lexeme] == false))
            {
                //Lox.error(expr.name, "Cannot read local variable in its own initializer.");
            }
            if (scopes.Count > 0 && scopes[scopes.Count - 1].ContainsKey(expr.name.lexeme))
            {
                if (scopes[scopes.Count - 1][expr.name.lexeme] == false) // declared but not yet defined
                    Lox.error(expr.name, "Cannot read local variable in its own initializer.");
            }

            resolveLocal(expr, expr.name);
            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            declare(stmt.name);
            if (stmt.initializer != null)
            {
                resolve(stmt.initializer);
            }
            define(stmt.name);
            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            resolve(stmt.condition);
            resolve(stmt.body);
            return null;
        }

        public object VisitClassStmt(Stmt.Class stmt)
        {
            ClassType enclosingClass = currentClass;
            currentClass = ClassType.CLASS;

            declare(stmt.name);
            define(stmt.name);

            if (stmt.superclass != null && stmt.name.lexeme == stmt.superclass.name.lexeme)
            {
                Lox.error(stmt.superclass.name,
                    "A class cannot inherit from itself.");
            }

            if (stmt.superclass != null)
            {
                currentClass = ClassType.SUBCLASS;
                resolve(stmt.superclass);

                beginScope();
                scopes[scopes.Count - 1].Add("super", true);
            }

            beginScope();
            scopes[scopes.Count - 1].Add("this", true);

            foreach (Stmt.Function method in stmt.methods)
            {
                FunctionType declaration = FunctionType.METHOD;
                if (method.name.lexeme == "init")
                {
                    declaration = FunctionType.INITIALIZER;
                }
                resolveFunction(method, declaration);
            }

            endScope();

            if (stmt.superclass != null) endScope();

            currentClass = enclosingClass;
            return null;
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            resolve(expr._object);
            return null;
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            resolve(expr.value);
            resolve(expr._object);
            return null;
        }

        public object VisitThisExpr(Expr.This expr)
        {
            if (currentClass == ClassType.NONE)
            {
                Lox.error(expr.keyword,
                    "Cannot use 'this' outside of a class.");
                return null;
            }
            resolveLocal(expr, expr.keyword);
            return null;
        }

        public object VisitSuperExpr(Expr.Super expr)
        {
            if (currentClass == ClassType.NONE)
            {
                Lox.error(expr.keyword,
                    "Cannot use 'super' outside of a class.");
            }
            else if (currentClass != ClassType.SUBCLASS)
            {
                Lox.error(expr.keyword,
                    "Cannot use 'super' in a class with no superclass.");
            }

            resolveLocal(expr, expr.keyword);
            return null;
        }
    }
}
