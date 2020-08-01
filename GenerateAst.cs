using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Lox
{
    public class GenerateAst
    {
        static string outputDir = @"C:\Users\pwnag\Desktop";
        public static void main()
        {
            //string outputDir = @"C:\Users\pwnag\Desktop";
            /*
            defineAst(outputDir, "Expr", new List<string>()
            {
                "Binary   : Expr left, Token _operator, Expr right",
      "Grouping : Expr expression",
      "Literal  : object value",
      "Unary    : Token _operator, Expr right",
            });
            */
        }

        public static void defineAst(string _outputDir,
            string baseName, List<string> types)
        {
            string path = System.IO.Path.Join(outputDir, baseName + ".cs");
            StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8);

            writer.WriteLine("using System;");
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine();
            writer.WriteLine();
            writer.WriteLine("namespace Lox");
            writer.WriteLine("{");
            writer.WriteLine($"public abstract class {baseName} {{");

            defineVisitor(writer, baseName, types);

            // The AST classes.
            foreach (string type in types)
            {
                string className = type.Split(":")[0].Trim();
                string fields = type.Split(":")[1].Trim();
                defineType(writer, baseName, className, fields);
            }

            // The base accept() method.
            writer.WriteLine();
            writer.WriteLine("  public abstract T Accept<T>(IVisitor<T> visitor);");

            writer.WriteLine("}");
            writer.WriteLine("}");
            writer.Close();
        }

        private static void defineVisitor(StreamWriter writer,
            string baseName, List<string> types)
        {
            writer.WriteLine("  public interface IVisitor<T> {");

            foreach (string type in types)
            {
                string typeName = type.Split(":")[0].Trim();
                writer.WriteLine($"    T Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
            }

            writer.WriteLine("  }");
        }

        private static void defineType(StreamWriter writer,
            string baseName, string className, string fieldList)
        {
            writer.WriteLine($"  public class {className} : {baseName} {{");

            // Constructor.
            writer.WriteLine($"    public {className}({fieldList}) {{");

            // Store parameters in fields.
            string[] fields = fieldList.Split(", ");
            foreach (string field in fields)
            {
                string name = field.Split(" ")[1];
                writer.WriteLine($"      this.{name} = {name};");
            }

            writer.WriteLine("    }");

            // Visitor pattern.
            writer.WriteLine();
            writer.WriteLine("    public override T Accept<T>(IVisitor<T> visitor)");
            writer.WriteLine("    {");
            writer.WriteLine($"      return visitor.Visit{className}{baseName}(this);");
            writer.WriteLine("    }");

            // Fields.
            writer.WriteLine();
            foreach (string field in fields)
            {
                writer.WriteLine($"    public readonly {field};");
            }

            writer.WriteLine("  }");
        }
    }

}
