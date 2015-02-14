// *********************************************************
//
// Copyright © Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of
// the License at
//
// http://www.apache.org/licenses/LICENSE-2.0 
//
// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES
// OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED,
// INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES
// OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY OR NON-INFRINGEMENT.
//
// See the Apache 2 License for the specific language
// governing permissions and limitations under the License.
//
// *********************************************************

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Completion.Providers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editor;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace ArgumentExceptionCompletionCS
{
    [ExportCompletionProvider("ArgumentExceptionCompletionCS", LanguageNames.CSharp)]
    internal class CompletionProvider : AbstractCompletionProvider
    {
        public override async Task<IEnumerable<CompletionItem>> GetItemsAsync(Document document, int position, CompletionTriggerInfo triggerInfo, CancellationToken cancellationToken)
        {
            var root = (SyntaxNode)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(position);
            if (token.Kind() != SyntaxKind.StringLiteralToken)
            {
                return null;
            }

            var ancestors = token.Parent.AncestorsAndSelf().ToArray();

            // Look for a constructor call above us.
            if (ancestors.Length < 4 ||
                ancestors[0].Kind() != SyntaxKind.StringLiteralExpression ||
                ancestors[1].Kind() != SyntaxKind.Argument ||
                ancestors[2].Kind() != SyntaxKind.ArgumentList ||
                ancestors[3].Kind() != SyntaxKind.ObjectCreationExpression)
            {
                return null;
            }

            var stringLiteral = ((LiteralExpressionSyntax)ancestors[0]).Token;
            var filterSpan = GetFilterSpan(stringLiteral);
            if (!filterSpan.IntersectsWith(position))
            {
                return null;
            }

            var objectCreation = (ObjectCreationExpressionSyntax)ancestors[3];
            var semanticModel = (SemanticModel)await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var semanticInfo = semanticModel.GetSymbolInfo(objectCreation, cancellationToken);

            var candidates = Enumerable.Empty<IMethodSymbol>();
            if (semanticInfo.Symbol != null && semanticInfo.Symbol.Kind == SymbolKind.Method)
            {
                candidates = candidates.Concat(new[] { (IMethodSymbol)semanticInfo.Symbol });
            }

            candidates = candidates.Concat(semanticInfo.CandidateSymbols.OfType<IMethodSymbol>());

            if (!candidates.Any())
            {
                return null;
            }

            var specialConstructors = FindConstructors(document.Project.GetCompilationAsync().Result);
            var constructors = candidates.Where(c => specialConstructors.Contains(c)).ToArray();
            if (!constructors.Any())
            {
                return null;
            }

            var validPosition = false;
            var argument = (ArgumentSyntax)ancestors[1];
            foreach (var constructor in constructors)
            {
                // Are we specifying the argument with a named argument?
                if (argument.NameColon != null)
                {
                    if (argument.NameColon.Name.ToString() == "paramName")
                    {
                        validPosition = true;
                    }
                }
                else
                {
                    // Otherwise, check to see if the index of the argument we're typing matches.
                    var paramNameParameterInConstructor = constructor.Parameters.Single(p => p.Name == "paramName");
                    var indexOfArgumentBeingTyped = ((ArgumentListSyntax)argument.Parent).Arguments.IndexOf(argument);
                    if (constructor.Parameters.IndexOf(paramNameParameterInConstructor) == indexOfArgumentBeingTyped)
                    {
                        validPosition = true;
                    }
                }
            }

            if (!validPosition)
            {
                return null;
            }

            var names = GetEnclosingParameterNames(argument);
            return names.Select(n => new CompletionItem(this, n, filterSpan)).ToArray();
        }

        public override bool IsCommitCharacter(CompletionItem completionItem, char ch, string textTypedSoFar, Workspace workspace, string languageName)
        {
            return ch == '"';
        }

        private static TextSpan GetFilterSpan(SyntaxToken stringLiteral)
        {
            TextSpan textSpan;
            if (stringLiteral.IsVerbatimStringLiteral())
            {
                if (stringLiteral.Span.Length > 2 && stringLiteral.ToString().EndsWith("\""))
                {
                    textSpan = TextSpan.FromBounds(stringLiteral.Span.Start + 2, stringLiteral.Span.End - 1);
                }
                else
                {
                    textSpan = TextSpan.FromBounds(stringLiteral.Span.Start + 2, stringLiteral.Span.End);
                }
            }
            else
            {
                if (stringLiteral.Span.Length > 1 && stringLiteral.ToString().EndsWith("\""))
                {
                    textSpan = TextSpan.FromBounds(stringLiteral.Span.Start + 1, stringLiteral.Span.End - 1);
                }
                else
                {
                    textSpan = TextSpan.FromBounds(stringLiteral.Span.Start + 1, stringLiteral.Span.End);
                }
            }

            return textSpan;
        }

        private IEnumerable<string> GetEnclosingParameterNames(SyntaxNode node)
        {
            var simpleLambda = node.FirstAncestorOrSelf<SimpleLambdaExpressionSyntax>();
            if (simpleLambda != null)
            {
                return new[] { simpleLambda.Parameter.Identifier.ToString() }.Concat(GetEnclosingParameterNames(simpleLambda.Parent));
            }

            var parenLambda = node.FirstAncestorOrSelf<ParenthesizedLambdaExpressionSyntax>();
            if (parenLambda != null)
            {
                return GetParameterNames(parenLambda.ParameterList).Concat(GetEnclosingParameterNames(parenLambda.Parent));
            }

            var method = node.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>();
            if (method != null)
            {
                return GetParameterNames(method.ParameterList);
            }

            var accessor = node.FirstAncestorOrSelf<AccessorDeclarationSyntax>();
            if (accessor != null)
            {
                IEnumerable<string> parameters = Enumerable.Empty<string>();
                if (accessor.Kind() == SyntaxKind.SetAccessorDeclaration ||
                    accessor.Kind() == SyntaxKind.AddAccessorDeclaration ||
                    accessor.Kind() == SyntaxKind.RemoveAccessorDeclaration)
                {
                    parameters = parameters.Concat(new[] { "value" });
                }

                var indexer = accessor.FirstAncestorOrSelf<IndexerDeclarationSyntax>();
                if (indexer != null)
                {
                    parameters = parameters.Concat(GetParameterNames(indexer.ParameterList));
                }

                return parameters;
            }

            return Enumerable.Empty<string>();
        }

        private IEnumerable<string> GetParameterNames(BaseParameterListSyntax parameterList)
        {
            return parameterList.Parameters.Select(p => p.Identifier.ToString());
        }

        private static IEnumerable<IMethodSymbol> FindConstructors(Compilation compilation)
        {
            var typeNames = new[]
                {
                    "System.ArgumentException",
                    "System.ArgumentNullException",
                    "System.ArgumentOutOfRangeException",
                };

            foreach (var typeName in typeNames)
            {
                var type = compilation.GetTypeByMetadataName(typeName);
                foreach (var constructor in type.InstanceConstructors)
                {
                    if (constructor.Parameters.Any(p => p.Name == "paramName"))
                    {
                        yield return constructor;
                    }
                }
            }
        }
    }
}