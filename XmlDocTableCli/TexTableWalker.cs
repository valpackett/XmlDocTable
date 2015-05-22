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

            public int FieldsCount;
            public int PropertiesCount;
            public int MethodsCount;

            public string FieldsTable => _fields.ToString();
            public string PropertiesTable => _properties.ToString();
            public string MethodsTable => _methods.ToString();

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

        /// <summary>Adds a field description to the member table of its class.</summary>
        public override void OnField(FieldDeclarationSyntax field, DocumentationCommentTriviaSyntax doc)
        {
            foreach (var variable in field.Declaration.Variables)
                this[field].AddField($"{Esc(variable.Identifier.ValueText)} & {Esc(field.Modifiers.ToString())} & {Esc(field.Declaration.Type.ToString())} & {Esc(doc.CommentField("summary"))} \\\\");
        }

        /// <summary>Adds a property description to the member table of its class.</summary>
        public override void OnProperty(PropertyDeclarationSyntax property, DocumentationCommentTriviaSyntax doc)
        {
            var al = property.AccessorList?.ToString() ?? "{ get; }"; // C# 6.0 short expression syntax means no AccessorList
            al = Regex.Replace(al, @"(get|set)\s*\{.*\}", @"$1");
            al = Regex.Replace(al, @"\s+", " ");
            this[property].AddProperty($"{Esc(property.Identifier.ValueText)} & {Esc(property.Modifiers.ToString())} & {Esc(property.Type.ToString())} & {Esc(al)} & {Esc(doc.CommentField("summary"))} \\\\");
        }

        /// <summary>Adds a method description to the member table of its class.</summary>
        public override void OnMethod(MethodDeclarationSyntax method, DocumentationCommentTriviaSyntax doc)
        {
            this[method].AddMethod($"{Esc(method.Identifier.ValueText)} & {Esc(method.Modifiers.ToString())} & {Esc(method.ReturnType.ToString())} & {Esc(method.ParameterList.ToString())} & {Esc(doc.CommentField("summary"))} \\\\");
        }

        /// <summary>Escapes a string for safe inclusion in a LaTeX table.</summary>
        private static string Esc(string s) => s.Replace(@"\", @"\\").Replace(@"&", @"\&");

        /// <summary>Finds the class name of a given member.</summary>
        private static string ClassKey(SyntaxNode syntax) => syntax.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First().Identifier.ValueText;

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
