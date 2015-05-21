using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XmlDocTableCli
{
    public static class Ext
    {
        public static string CommentField(this SyntaxNode doc, string field)
        {
            var s = string.Join("",
                doc.DescendantNodes()
                    .OfType<XmlElementSyntax>()
                    .Where(x => x.StartTag.Name.LocalName.ValueText.Contains(field))
                    .Select(x => x.Content.ToString()));
            s = Regex.Replace(s, @"<[^>]+>", "").Replace("///", "").Trim();
            return s;
        }
    }
}
