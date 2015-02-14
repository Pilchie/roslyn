using System.Collections.Generic;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Completion.Providers
{
    internal class CompletionItemUnionComparer : IEqualityComparer<CompletionItem>
    {
        public static CompletionItemUnionComparer Instance = new CompletionItemUnionComparer();

        public bool Equals(CompletionItem x, CompletionItem y)
        {
            return x.DisplayText == y.DisplayText && x.Glyph == y.Glyph;
        }

        public int GetHashCode(CompletionItem obj)
        {
            return Hash.Combine(obj.DisplayText.GetHashCode(), obj.Glyph.GetHashCode());
        }
    }
}
