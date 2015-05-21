using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XmlDocTableCli
{
    public static class Ext
    {
        public static string CommentField(this SyntaxNode doc, string field)
        {
            return string.Join("",
                doc.DescendantNodes()
                    .OfType<XmlElementSyntax>()
                    .Where(x => x.StartTag.Name.LocalName.ValueText.Contains(field))
                    .Select(x => x.Content.ToString())).Replace("///", "").Trim();
        }
    }
}