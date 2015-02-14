using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Completion.Providers
{
    internal interface ICompletionProviderDefaultBehaviorLanguageService : ILanguageService
    {
        bool SendEnterThroughToEditor(CompletionItem completionItem, string textTypedSoFar);
        bool IsTriggerCharacter(SourceText text, int characterPosition, OptionSet options);
    }
}
