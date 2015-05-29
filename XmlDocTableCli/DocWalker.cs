using System.Linq;
using System.Collections.Generic;
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
        /// <summary>The callback that gets executed whenever a documentation comment is on a struct.</summary>
        public abstract void OnStruct(StructDeclarationSyntax klass, DocumentationCommentTriviaSyntax doc);
        /// <summary>The callback that gets executed whenever a documentation comment is on a field.</summary>
        public abstract void OnField(FieldDeclarationSyntax field, DocumentationCommentTriviaSyntax doc);
        /// <summary>The callback that gets executed whenever a documentation comment is on a property.</summary>
        public abstract void OnProperty(PropertyDeclarationSyntax property, DocumentationCommentTriviaSyntax doc);
        /// <summary>The callback that gets executed whenever a documentation comment is on a method.</summary>
        public abstract void OnMethod(MethodDeclarationSyntax method, DocumentationCommentTriviaSyntax doc);
        /// <summary>The callback that gets executed whenever a documentation comment is on a constructor.</summary>
        public abstract void OnConstructor(ConstructorDeclarationSyntax method, DocumentationCommentTriviaSyntax doc);

        /// <summary>The visitor that executes the callbacks.</summary>
        public override void VisitTrivia(SyntaxTrivia trivia)
        {
            if (SyntaxFacts.IsDocumentationCommentTrivia(trivia.Kind()))
            {
                var doc = (DocumentationCommentTriviaSyntax) trivia.GetStructure();
                var parent = trivia.Token.Parent;
                var fds = parent.AncestorsAndSelf().OfType<FieldDeclarationSyntax>().ToList();
                var pds = parent.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().ToList();
                var mds = parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().ToList();
                var cnds = parent.AncestorsAndSelf().OfType<ConstructorDeclarationSyntax>().ToList();
                var cds = parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().ToList();
                var sds = parent.AncestorsAndSelf().OfType<StructDeclarationSyntax>().ToList();
                if (fds.Count > 0)
                    OnField(fds.First(), doc);
                else if (pds.Count > 0)
                    OnProperty(pds.First(), doc);
                else if (mds.Count > 0)
                    OnMethod(mds.First(), doc);
                else if (cnds.Count > 0)
                    OnConstructor(cnds.First(), doc);
                else if (sds.Count > 0)
                    OnStruct(sds.First(), doc);
                else if (cds.Count > 0)
                    OnClass(cds.First(), doc);
            }
            base.VisitTrivia(trivia);
        }
    }
}
