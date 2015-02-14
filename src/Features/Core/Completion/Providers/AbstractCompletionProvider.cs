// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Completion.Providers
{
    public abstract partial class AbstractCompletionProvider : ICompletionProvider
    {
        private static readonly char[] DefaultCommitCharacters = new[]
        {
            ' ', '{', '}', '[', ']', '(', ')', '.', ',', ':',
            ';', '+', '-', '*', '/', '%', '&', '|', '^', '!',
            '~', '=', '<', '>', '?', '@', '#', '\'', '\"', '\\'
        };

        /// <summary>
        /// Get the items that should be shown in completion.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> that completion applies to.</param>
        /// <param name="position">The character position in <paramref name="document"/> where completion was triggered.</param>
        /// <param name="triggerInfo">Information about what triggered completion.</param>
        /// <param name="cancellationToken">Used to cancel processing.</param>
        /// <returns>The items to display, or a null <see cref="Task{TResult}"/> if there is nothing to show.</returns>
        public abstract Task<IEnumerable<CompletionItem>> GetItemsAsync(Document document, int position, CompletionTriggerInfo triggerInfo, CancellationToken cancellationToken);
 
        /// <summary>
        /// If a completion "builder" should be shown, returns an item with <see cref="CompletionItem.IsBuilder"/>
        /// equal to true, and with a description that explains why the builder was shown.  If no completion builder
        /// should be shown, return a null <see cref="Task{TResult}.Result"/>.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> that completion applies to.</param>
        /// <param name="position">The character position in <paramref name="document"/> where completion will be shown.</param>
        /// <param name="triggerInfo">Information about what triggered completion.</param>
        /// <param name="cancellationToken">Used to cancel processing.</param>
        protected virtual Task<CompletionItem> GetBuilderAsync(Document document, int position, CompletionTriggerInfo triggerInfo, CancellationToken cancellationToken)
        {
            return SpecializedTasks.Default<CompletionItem>();
        }

        /// <summary>
        /// Determine what <see cref="TextChange"/> to apply to the <see cref="Document"/> for the given <see cref="CompletionItem"/>.
        /// </summary>
        /// <param name="selectedItem">The item to be committed.</param>
        /// <param name="ch">The character <paramref name="selectedItem"/> was committed with (if any).</param>
        /// <param name="textTypedSoFar">The text that has already been typed by the user.</param>
        public virtual TextChange GetTextChange(CompletionItem selectedItem, char? ch = null, string textTypedSoFar = null)
        {
            return new TextChange(selectedItem.FilterSpan, selectedItem.DisplayText);
        }

        /// <summary>
        /// Determines whether <paramref name="ch"/> should be considered a commit character for the 
        /// <see cref="CompletionItem"/> with the given <paramref name="textTypedSoFar"/>.
        /// </summary>
        public virtual bool IsCommitCharacter(CompletionItem completionItem, char ch, string textTypedSoFar, Workspace workspace, string languageName)
        {
            return DefaultCommitCharacters.Contains(ch);
        }

        /// <summary>
        /// Determines whether an enter key press used to commit the item will be passed
        /// through to the editor.
        /// </summary>
        public virtual bool SendEnterThroughToEditor(CompletionItem completionItem, string textTypedSoFar, Workspace workspace, string languageName)
        {
            if (workspace != null && workspace.Services.IsSupported(languageName))
            {
                var defaultBehavior = workspace.Services.GetLanguageServices(languageName).GetService<ICompletionProviderDefaultBehaviorLanguageService>();
                if (defaultBehavior != null)
                {
                    return defaultBehavior.SendEnterThroughToEditor(completionItem, textTypedSoFar);
                }
            }

            return false;
        }

        /// <summary>
        /// True if the character at the given <paramref name="characterPosition"/> could possibly 
        /// trigger IntelliSense.  Used for performance optimizations to avoid calling
        /// <see cref="GetItemsAsync(Document, int, CompletionTriggerInfo, CancellationToken)"/>
        /// if there is no way completion could be triggered here.
        /// </summary>
        public virtual bool IsTriggerCharacter(SourceText text, int characterPosition, OptionSet options, Workspace workspace, string languageName)
        {
            if (workspace != null && workspace.Services.IsSupported(languageName))
            {
                var defaultBehavior = workspace.Services.GetLanguageServices(languageName).GetService<ICompletionProviderDefaultBehaviorLanguageService>();
                if (defaultBehavior != null)
                {
                    return defaultBehavior.IsTriggerCharacter(text, characterPosition, options);
                }
            }

            return true;
        }

        /// <summary>
        /// Returns true if only items from this <see cref="AbstractCompletionProvider" />s should be shown at this
        /// <paramref name="position"/> in this <see cref="Document"/>, for the given
        /// <see cref="CompletionTriggerInfo"/>.
        /// </summary>
        protected virtual Task<bool> IsExclusiveAsync(Document document, int position, CompletionTriggerInfo triggerInfo, CancellationToken cancellationToken)
        {
            return SpecializedTasks.False;
        }

        /// <summary>
        /// Returns true if the non-identifier character typed (<paramref name="ch"/>) should be used to filter the
        /// specified <see cref="CompletionItem"/>.  A character will be checked to see if it should filter an item.
        /// If not, it will be checked to see if it should commit that item.  If it does neither, then completion
        /// will be dismissed.
        /// </summary>
        public virtual bool IsFilterCharacter(CompletionItem completionItem, char ch, string textTypedSoFar)
        {
            return false;
        }

        async Task<CompletionItemGroup> ICompletionProvider.GetGroupAsync(Document document, int position, CompletionTriggerInfo triggerInfo, CancellationToken cancellationToken)
        {
            var items = await this.GetItemsAsync(document, position, triggerInfo, cancellationToken).ConfigureAwait(false);
            var builder = await this.GetBuilderAsync(document, position, triggerInfo, cancellationToken).ConfigureAwait(false);

            if (items == null && builder == null)
            {
                return null;
            }

            return new CompletionItemGroup(
                items ?? SpecializedCollections.EmptyEnumerable<CompletionItem>(),
                builder,
                await this.IsExclusiveAsync(document, position, triggerInfo, cancellationToken).ConfigureAwait(false));
        }
    }
}
