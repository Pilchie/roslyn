using System.Composition;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Completion.Providers;
using Microsoft.CodeAnalysis.CSharp.Completion.Providers;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Completion.CompletionProviders
{
    [ExportLanguageService(typeof(ICompletionProviderDefaultBehaviorLanguageService), LanguageNames.CSharp), Shared]
    internal sealed class CSharpCompletionProviderDefaultBehaviorLanguageService : ICompletionProviderDefaultBehaviorLanguageService
    {
        public bool IsTriggerCharacter(SourceText text, int characterPosition, OptionSet options)
        {
            return CompletionUtilities.IsTriggerCharacter(text, characterPosition, options);
        }

        public bool SendEnterThroughToEditor(CompletionItem completionItem, string textTypedSoFar)
        {
            return CompletionUtilities.SendEnterThroughToEditor(completionItem, textTypedSoFar);
        }
    }
}
