// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.LanguageServices.Implementation.CodeModel.InternalElements;
using Microsoft.VisualStudio.LanguageServices.Implementation.CodeModel.Interop;
using Microsoft.VisualStudio.LanguageServices.Implementation.Interop;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.CodeModel.Collections
{
    [ComVisible(true)]
    [ComDefaultInterface(typeof(ICodeElements))]
    public sealed class NamespaceCollection : AbstractCodeElementCollection
    {
        internal static EnvDTE.CodeElements Create(
            CodeModelState state,
            object parent,
            FileCodeModel fileCodeModel,
            SyntaxNodeKey nodeKey)
        {
            var collection = new NamespaceCollection(state, parent, fileCodeModel, nodeKey);
            return (EnvDTE.CodeElements)ComAggregate.CreateAggregatedObject(collection);
        }

        private ComHandle<EnvDTE.FileCodeModel, FileCodeModel> _fileCodeModel;
        private SyntaxNodeKey _nodeKey;

        private NamespaceCollection(
            CodeModelState state,
            object parent,
            FileCodeModel fileCodeModel,
            SyntaxNodeKey nodeKey)
            : base(state, parent)
        {
            Debug.Assert(fileCodeModel != null);

            _fileCodeModel = new ComHandle<EnvDTE.FileCodeModel, FileCodeModel>(fileCodeModel);
            _nodeKey = nodeKey;
        }

        private FileCodeModel FileCodeModel
        {
            get { return _fileCodeModel.Object; }
        }

        private bool IsRootNamespace
        {
            get { return _nodeKey == SyntaxNodeKey.Empty; }
        }

        private SyntaxNode LookupNode()
        {
            if (!IsRootNamespace)
            {
                return FileCodeModel.LookupNode(_nodeKey);
            }
            else
            {
                return FileCodeModel.GetSyntaxRoot();
            }
        }

        private EnvDTE.CodeElement CreateCodeOptionsStatement(SyntaxNode node, SyntaxNode parentNode)
        {
            string name;
            int ordinal;
            CodeModelService.GetOptionNameAndOrdinal(parentNode, node, out name, out ordinal);

            return (EnvDTE.CodeElement)CodeOptionsStatement.Create(this.State, this.FileCodeModel, name, ordinal);
        }

        private EnvDTE.CodeElement CreateCodeImport(SyntaxNode node, AbstractCodeElement parentElement)
        {
            var name = CodeModelService.GetImportNamespaceOrType(node);

            return (EnvDTE.CodeElement)CodeImport.Create(this.State, this.FileCodeModel, parentElement, name);
        }

        private EnvDTE.CodeElement CreateCodeAttribute(SyntaxNode node, SyntaxNode parentNode, AbstractCodeElement parentElement)
        {
            string name;
            int ordinal;
            CodeModelService.GetAttributeNameAndOrdinal(parentNode, node, out name, out ordinal);

            return (EnvDTE.CodeElement)CodeAttribute.Create(this.State, this.FileCodeModel, parentElement, name, ordinal);
        }

        internal override Snapshot CreateSnapshot()
        {
            var node = LookupNode();
            var parentElement = !this.IsRootNamespace
                ? (AbstractCodeElement)this.Parent
                : null;

            var nodesBuilder = ImmutableArray.CreateBuilder<SyntaxNode>();
            nodesBuilder.AddRange(CodeModelService.GetOptionNodes(node));
            nodesBuilder.AddRange(CodeModelService.GetImportNodes(node));
            nodesBuilder.AddRange(CodeModelService.GetAttributeNodes(node));
            nodesBuilder.AddRange(CodeModelService.GetFlattenedMemberNodes(node));

            return new NodeSnapshot(this.State, _fileCodeModel, node, parentElement, nodesBuilder.ToImmutable());
        }

        protected override bool TryGetItemByIndex(int index, out EnvDTE.CodeElement element)
        {
            var node = LookupNode();
            var parentElement = !this.IsRootNamespace
                ? (AbstractCodeElement)this.Parent
                : null;

            int currentIndex = 0;

            // Option statements
            var optionNodes = CodeModelService.GetOptionNodes(node);
            var optionNodeCount = optionNodes.Count();
            if (index < currentIndex + optionNodeCount)
            {
                var child = optionNodes.ElementAt(index - currentIndex);
                element = CreateCodeOptionsStatement(child, node);
                return true;
            }

            currentIndex += optionNodeCount;

            // Imports/using statements
            var importNodes = CodeModelService.GetImportNodes(node);
            var importNodeCount = importNodes.Count();
            if (index < currentIndex + importNodeCount)
            {
                var child = importNodes.ElementAt(index - currentIndex);
                element = CreateCodeImport(child, parentElement);
                return true;
            }

            currentIndex += importNodeCount;

            // Attributes
            var attributeNodes = CodeModelService.GetAttributeNodes(node);
            var attributeNodeCount = attributeNodes.Count();
            if (index < currentIndex + attributeNodeCount)
            {
                var child = attributeNodes.ElementAt(index - currentIndex);
                element = CreateCodeAttribute(child, node, parentElement);
                return true;
            }

            currentIndex += attributeNodeCount;

            // Members
            var memberNodes = CodeModelService.GetFlattenedMemberNodes(node);
            var memberNodeCount = memberNodes.Count();
            if (index < currentIndex + memberNodeCount)
            {
                var child = memberNodes.ElementAt(index - currentIndex);
                element = FileCodeModel.CreateCodeElement<EnvDTE.CodeElement>(child);
                return true;
            }

            element = null;
            return false;
        }

        protected override bool TryGetItemByName(string name, out EnvDTE.CodeElement element)
        {
            var node = LookupNode();
            var parentElement = !IsRootNamespace
                ? (AbstractCodeElement)Parent
                : null;

            // Option statements
            foreach (var child in CodeModelService.GetOptionNodes(node))
            {
                string childName;
                int ordinal;
                CodeModelService.GetOptionNameAndOrdinal(node, child, out childName, out ordinal);
                if (childName == name)
                {
                    element = (EnvDTE.CodeElement)CodeOptionsStatement.Create(State, FileCodeModel, childName, ordinal);
                    return true;
                }
            }

            // Imports/using statements
            foreach (var child in CodeModelService.GetImportNodes(node))
            {
                var childName = CodeModelService.GetImportNamespaceOrType(child);
                if (childName == name)
                {
                    element = (EnvDTE.CodeElement)CodeImport.Create(State, FileCodeModel, parentElement, childName);
                    return true;
                }
            }

            // Attributes
            foreach (var child in CodeModelService.GetAttributeNodes(node))
            {
                string childName;
                int ordinal;
                CodeModelService.GetAttributeNameAndOrdinal(node, child, out childName, out ordinal);
                if (childName == name)
                {
                    element = (EnvDTE.CodeElement)CodeAttribute.Create(State, FileCodeModel, parentElement, childName, ordinal);
                    return true;
                }
            }

            // Members
            foreach (var child in CodeModelService.GetFlattenedMemberNodes(node))
            {
                var childName = CodeModelService.GetName(child);
                if (childName == name)
                {
                    element = FileCodeModel.CreateCodeElement<EnvDTE.CodeElement>(child);
                    return true;
                }
            }

            element = null;
            return false;
        }

        public override int Count
        {
            get
            {
                var node = LookupNode();
                return
                    CodeModelService.GetOptionNodes(node).Count() +
                    CodeModelService.GetImportNodes(node).Count() +
                    CodeModelService.GetAttributeNodes(node).Count() +
                    CodeModelService.GetFlattenedMemberNodes(node).Count();
            }
        }
    }
}
