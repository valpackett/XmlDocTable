using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XmlDocTableCli
{
    /// <summary>A base class for all documentation generators.</summary>
    public abstract class DocWalker : CSharpSyntaxWalker
    {
        protected DocWalker() : base(SyntaxWalkerDepth.StructuredTrivia) { }

        /// <summary>The callback that gets executed whenever a documentation comment is on a class.</summary>
        public abstract void OnClass(ClassDeclarationSyntax klass, DocumentationCommentTriviaSyntax doc);
        /// <summary>The callback that gets executed whenever a documentation comment is on a field.</summary>
        public abstract void OnField(FieldDeclarationSyntax field, DocumentationCommentTriviaSyntax doc);
        /// <summary>The callback that gets executed whenever a documentation comment is on a property.</summary>
        public abstract void OnProperty(PropertyDeclarationSyntax property, DocumentationCommentTriviaSyntax doc);
        /// <summary>The callback that gets executed whenever a documentation comment is on a method.</summary>
        public abstract void OnMethod(MethodDeclarationSyntax method, DocumentationCommentTriviaSyntax doc);

        /// <summary>The visitor that executes the callbacks.</summary>
        public override void VisitTrivia(SyntaxTrivia trivia)
        {
            if (SyntaxFacts.IsDocumentationCommentTrivia(trivia.Kind()))
            {
                var doc = (DocumentationCommentTriviaSyntax) trivia.GetStructure();
                var parent = trivia.Token.Parent;
                var type = parent.GetType();
                if (type == typeof(ClassDeclarationSyntax))
                    OnClass((ClassDeclarationSyntax) parent, doc);
                else if (type == typeof(FieldDeclarationSyntax))
                    OnField((FieldDeclarationSyntax) parent, doc);
                else if (type == typeof(PropertyDeclarationSyntax))
                    OnProperty((PropertyDeclarationSyntax) parent, doc);
                else if (type == typeof(MethodDeclarationSyntax))
                    OnMethod((MethodDeclarationSyntax) parent, doc);
            }
            base.VisitTrivia(trivia);
        }
    }
}
