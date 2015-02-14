' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Completion
Imports Microsoft.CodeAnalysis.Completion.Providers
Imports Microsoft.CodeAnalysis.Options
Imports Microsoft.CodeAnalysis.Text
Imports Roslyn.Utilities

Namespace Microsoft.VisualStudio.LanguageServices.UnitTests.Completion
    Friend Class MockCompletionProvider
        Inherits AbstractCompletionProvider

        Private ReadOnly _span As TextSpan
        Private ReadOnly _itemsTask As Task(Of IEnumerable(Of CompletionItem))

        Public Sub New(span As TextSpan)
            _span = span
            _itemsTask = Task.FromResult(SpecializedCollections.SingletonEnumerable(New CompletionItem(Me, "DisplayText", span)))
        End Sub

        Public Overrides Function GetItemsAsync(document As Document, position As Integer, triggerInfo As CompletionTriggerInfo, cancellationToken As CancellationToken) As Task(Of IEnumerable(Of CompletionItem))
            Return _itemsTask
        End Function

        Public Overrides Function IsCommitCharacter(completionItem As CompletionItem, ch As Char, textTypedSoFar As String, workspace As Workspace, languageName As String) As Boolean
            Return False
        End Function

        Public Overrides Function IsTriggerCharacter(text As SourceText, characterPosition As Integer, options As OptionSet, workspace As Workspace, languageName As String) As Boolean
            Return True
        End Function

        Public Overrides Function SendEnterThroughToEditor(completionItem As CompletionItem, textTypedSoFar As String, workspace As Workspace, languageName As String) As Boolean
            Return False
        End Function

        Public Overrides Function GetTextChange(selectedItem As CompletionItem, Optional ch As Char? = Nothing, Optional textTypedSoFar As String = Nothing) As TextChange
            Return New TextChange(selectedItem.FilterSpan, "InsertionText")
        End Function
    End Class
End Namespace
