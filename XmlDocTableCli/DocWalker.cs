using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XmlDocTableCli
{
    public abstract class DocWalker : CSharpSyntaxWalker
    {
        protected DocWalker() : base(SyntaxWalkerDepth.StructuredTrivia) { }

        public abstract void OnClass(ClassDeclarationSyntax klass, DocumentationCommentTriviaSyntax doc);
        public abstract void OnField(FieldDeclarationSyntax field, DocumentationCommentTriviaSyntax doc);
        public abstract void OnProperty(PropertyDeclarationSyntax property, DocumentationCommentTriviaSyntax doc);
        public abstract void OnMethod(MethodDeclarationSyntax method, DocumentationCommentTriviaSyntax doc);

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