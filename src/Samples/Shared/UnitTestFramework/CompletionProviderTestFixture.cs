// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Completion.Providers;
using Microsoft.CodeAnalysis.Text;
using Roslyn.UnitTestFramework;
using Xunit;

namespace Roslyn.UnitTestFramework
{
    public abstract class CompletionProviderTestFixture
    {
        private readonly string language;

        protected CompletionProviderTestFixture(string language)
        {
            this.language = language;
        }

        protected abstract AbstractCompletionProvider CreateProvider();

        protected void VerifyCompletion(string markup)
        {
            var items = GetCompletionItems(markup);
            Assert.True(items.Any());
        }

        protected void VerifyNoCompletion(string markup)
        {
            var items = GetCompletionItems(markup);
            Assert.True(items == null || !items.Any());
        }

        protected void VerifyCompletionContains(string itemDisplayText, string markup)
        {
            var items = GetCompletionItems(markup);
            Assert.True(items.Any(item => item.DisplayText == itemDisplayText));
        }

        protected void VerifyCompletionDoesNotContain(string itemDisplayText, string markup)
        {
            var items = GetCompletionItems(markup);
            Assert.False(items.Any(item => item.DisplayText == itemDisplayText));
        }

        private IEnumerable<CompletionItem> GetCompletionItems(string markup)
        {
            var provider = CreateProvider();
            string code;
            int cursorPosition;
            MarkupTestFile.GetPosition(markup, out code, out cursorPosition);
            var document = CreateDocument(code);
            var triggerInfo = CompletionTriggerInfo.CreateInvokeCompletionTriggerInfo();
            return provider.GetItemsAsync(document, cursorPosition, triggerInfo, CancellationToken.None).Result;
        }

        private Document CreateDocument(string code)
        {
            var docName = "Test." + this.language == LanguageNames.CSharp ? "cs" : "vb";
            var project = new AdhocWorkspace()
                .AddProject("TestProject", this.language)
                .AddMetadataReference(MetadataReference.CreateFromAssembly(typeof(object).Assembly))
                .AddMetadataReference(MetadataReference.CreateFromAssembly(typeof(Enumerable).Assembly));

            return project.AddDocument(docName, code);
        }
    }
}
