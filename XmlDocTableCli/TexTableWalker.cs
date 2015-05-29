using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XmlDocTableCli
{
    /// <summary>
    /// A documentation generator that makes separate LaTeX tables for each class
    /// and a class description table.
    /// </summary>
    public class TexTableWalker : DocWalker
    {
        public class ClassMembers
        {
            readonly StringBuilder _fields = new StringBuilder();
            readonly StringBuilder _properties = new StringBuilder();
            readonly StringBuilder _methods = new StringBuilder();
            readonly StringBuilder _constructors = new StringBuilder();

            public int FieldsCount;
            public int PropertiesCount;
            public int MethodsCount;
            public int ConstructorsCount;

            public string FieldsTable => _fields.ToString();
            public string PropertiesTable => _properties.ToString();
            public string MethodsTable => _methods.ToString();
            public string ConstructorsTable => _constructors.ToString();

            public void AddField(string s)
            {
                _fields.AppendLine(s);
                FieldsCount++;
            }

            public void AddProperty(string s)
            {
                _properties.AppendLine(s);
                PropertiesCount++;
            }

            public void AddMethod(string s)
            {
                _methods.AppendLine(s);
                MethodsCount++;
            }

            public void AddConstructor(string s)
            {
                _constructors.AppendLine(s);
                ConstructorsCount++;
            }
        }

        readonly StringBuilder _classTable = new StringBuilder();

        /// <summary>The generated member tables in LaTeX syntax.</summary>
        public Dictionary<string, ClassMembers> MemberTables { get; } = new Dictionary<string, ClassMembers>();

        /// <summary>The generated class table in LaTeX syntax.</summary>
        public string ClassTable => _classTable.ToString();

        /// <summary>Adds a class description to the class table.</summary>
        public override void OnClass(ClassDeclarationSyntax klass, DocumentationCommentTriviaSyntax doc)
        {
            _classTable.AppendLine($"{Esc(klass.Identifier.ValueText)} & {Esc(doc.CommentField("summary"))} \\\\");
        }

        /// <summary>Adds a struct description to the class table.</summary>
        public override void OnStruct(StructDeclarationSyntax klass, DocumentationCommentTriviaSyntax doc)
        {
            _classTable.AppendLine($"{Esc(klass.Identifier.ValueText)} & {Esc(doc.CommentField("summary"))} \\\\");
        }

        /// <summary>Adds a field description to the member table of its class.</summary>
        public override void OnField(FieldDeclarationSyntax field, DocumentationCommentTriviaSyntax doc)
        {
            foreach (var variable in field.Declaration.Variables)
                this[field].AddField($"{Esc(variable.Identifier.ValueText)} & {Esc(field.Modifiers.ToString())} & {Esc(field.Declaration.Type.ToString())} & {Multicol(Esc(doc.CommentField("summary")))}" + @" \\ \tabuphantomline");
        }

        /// <summary>Adds a property description to the member table of its class.</summary>
        public override void OnProperty(PropertyDeclarationSyntax property, DocumentationCommentTriviaSyntax doc)
        {
            var al = property.AccessorList?.ToString() ?? "{ get; }"; // C# 6.0 short expression syntax means no AccessorList
            al = Regex.Replace(al, @"([sg]et)\s*\{[^}]*}", @"$1");
            al = Regex.Replace(al, @"\s+", " ");
            this[property].AddProperty($"{Esc(property.Identifier.ValueText)} & {Esc(property.Modifiers.ToString())} & {Esc(property.Type.ToString())} & {Esc(al)} & {Esc(doc.CommentField("summary"))} \\\\");
        }

        /// <summary>Adds a method description to the member table of its class.</summary>
        public override void OnMethod(MethodDeclarationSyntax method, DocumentationCommentTriviaSyntax doc)
        {
            this[method].AddMethod($"{Esc(method.Identifier.ValueText)} & {Esc(method.Modifiers.ToString())} & {Esc(method.ReturnType.ToString())} & {Esc(method.ParameterList.ToString())} & {Esc(doc.CommentField("summary"))} \\\\");
        }

        /// <summary>Adds a constructor description to the member table of its class.</summary>
        public override void OnConstructor(ConstructorDeclarationSyntax constructor, DocumentationCommentTriviaSyntax doc)
        {
            this[constructor].AddConstructor($"{Esc(constructor.Identifier.ValueText)} & {Esc(constructor.Modifiers.ToString())} & {Esc(constructor.ParameterList.ToString())} & {Multicol(Esc(doc.CommentField("summary")))}" + @" \\ \tabuphantomline");
        }

        /// <summary>Escapes a string for safe inclusion in a LaTeX table.</summary>
        private static string Esc(string s)
        {
            s = s.Replace(@"\", @"\textbackslash{}")
                .Replace("^", @"\textasciicircum{}")
                .Replace("~", @"\textasciitilde{}");
            s = Regex.Replace(s, @"([#\$%&_\{}])", @"\$1");
            s = s.Replace("---", "{-}{-}{-}")
                .Replace("--", "{-}{-}")
                .Replace("<", @"{\tt <}\-")
                .Replace(">", @"\-{\tt >}");
            s = Regex.Replace(s, @"([a-z])([A-Z])", @"$1\-$2"); // Hyphenation for CamelCase
            s = s.Replace(@"La\-Te\-X", @"\LaTeX{}");
            return s;
        }

        /// <summary>Generates a tabu multicolumn declaration.</summary>
        public static string Multicol(string s, int cols = 2, int xs = 5) =>
            @"\multicolumn{" + cols + @"}{p{\dimexpr " + xs + @"\tabucolX+" + xs + @"\tabcolsep+\arrayrulewidth\relax}}{" + s + @"}";

        /// <summary>Finds the class or struct name of a given member.</summary>
        private static string ClassKey(SyntaxNode syntax)
        {
            var cds = syntax.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().ToList();
            var sds = syntax.AncestorsAndSelf().OfType<StructDeclarationSyntax>().ToList();
            if (cds.Count > 0)
                return cds.First().Identifier.ValueText;
            if (sds.Count > 0)
                return sds.First().Identifier.ValueText;
            return "<NotFound>";
        }

        private ClassMembers this[SyntaxNode node]
        {
            get
            {
                var key = ClassKey(node);
                if (!MemberTables.ContainsKey(key))
                    MemberTables[key] = new ClassMembers();
                return MemberTables[key];
            }
        }
    }
}
