using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Completion.Providers
{
    internal abstract class AbstractSuggestionModeCompletionProvider : AbstractCompletionProvider
    {
        protected abstract override Task<CompletionItem> GetBuilderAsync(Document document, int position, CompletionTriggerInfo triggerInfo, CancellationToken cancellationToken);

        public override bool IsCommitCharacter(CompletionItem completionItem, char ch, string textTypedSoFar, Workspace workspace, string languageName)
        {
            return false;
        }

        public override bool SendEnterThroughToEditor(CompletionItem completionItem, string textTypedSoFar, Workspace workspace, string languageName)
        {
            return false;
        }

        public override bool IsTriggerCharacter(SourceText text, int characterPosition, OptionSet options, Workspace workspace, string languageName)
        {
            return false;
        }

        public override Task<IEnumerable<CompletionItem>> GetItemsAsync(Document document, int position, CompletionTriggerInfo triggerInfo, CancellationToken cancellationToken)
        {
            return SpecializedTasks.Default<IEnumerable<CompletionItem>>();
        }
    }
}
