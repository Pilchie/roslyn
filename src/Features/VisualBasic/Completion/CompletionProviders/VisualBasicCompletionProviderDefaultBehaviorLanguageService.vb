Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Completion
Imports Microsoft.CodeAnalysis.Completion.Providers
Imports Microsoft.CodeAnalysis.Host.Mef
Imports Microsoft.CodeAnalysis.Options
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic.Completion.Providers

<ExportLanguageService(GetType(ICompletionProviderDefaultBehaviorLanguageService), LanguageNames.VisualBasic), [Shared]>
Friend NotInheritable Class VisualBasicCompletionProviderDefaultBehaviorLanguageService
    Implements ICompletionProviderDefaultBehaviorLanguageService

    Public Function IsTriggerCharacter(text As SourceText, characterPosition As Integer, options As OptionSet) As Boolean Implements ICompletionProviderDefaultBehaviorLanguageService.IsTriggerCharacter
        Return CompletionUtilities.IsDefaultTriggerCharacter(text, characterPosition, options)
    End Function

    public Function SendEnterThroughToEditor(completionItem As CompletionItem, textTypedSoFar As String) As Boolean Implements ICompletionProviderDefaultBehaviorLanguageService.SendEnterThroughToEditor
        Return CompletionUtilities.SendEnterThroughToEditor(completionItem, textTypedSoFar)
    End Function
End Class
