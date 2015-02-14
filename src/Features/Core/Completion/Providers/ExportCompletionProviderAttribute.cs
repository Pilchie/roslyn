using System;
using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis.Completion.Providers;

namespace Microsoft.CodeAnalysis.Completion.Providers
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class ExportCompletionProviderAttribute : ExportAttribute
    {
        public string Name { get; }
        public string Language { get; }

        public ExportCompletionProviderAttribute(string name, string language)
            : base(typeof(ICompletionProvider))
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (language == null)
            {
                throw new ArgumentNullException("language");
            }

            this.Name = name;
            this.Language = language;
        }
    }
}
