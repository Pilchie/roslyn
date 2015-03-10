﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using EmitContext = Microsoft.CodeAnalysis.Emit.EmitContext;

namespace Microsoft.Cci
{
    internal sealed class FullMetadataWriter : MetadataWriter
    {
        private readonly DefinitionIndex<ITypeDefinition> _typeDefs;
        private readonly DefinitionIndex<IEventDefinition> _eventDefs;
        private readonly DefinitionIndex<IFieldDefinition> _fieldDefs;
        private readonly DefinitionIndex<IMethodDefinition> _methodDefs;
        private readonly DefinitionIndex<IPropertyDefinition> _propertyDefs;
        private readonly DefinitionIndex<IParameterDefinition> _parameterDefs;
        private readonly DefinitionIndex<IGenericParameter> _genericParameters;

        private readonly Dictionary<ITypeDefinition, uint> _fieldDefIndex;
        private readonly Dictionary<ITypeDefinition, uint> _methodDefIndex;
        private readonly Dictionary<IMethodDefinition, uint> _parameterListIndex;

        private readonly HeapOrReferenceIndex<IAssemblyReference> _assemblyRefIndex;
        private readonly HeapOrReferenceIndex<string> _moduleRefIndex;
        private readonly InstanceAndStructuralReferenceIndex<ITypeMemberReference> _memberRefIndex;
        private readonly InstanceAndStructuralReferenceIndex<IGenericMethodInstanceReference> _methodSpecIndex;
        private readonly HeapOrReferenceIndex<ITypeReference> _typeRefIndex;
        private readonly InstanceAndStructuralReferenceIndex<ITypeReference> _typeSpecIndex;
        private readonly HeapOrReferenceIndex<uint> _standAloneSignatureIndex;

        public static MetadataWriter Create(
            EmitContext context,
            CommonMessageProvider messageProvider,
            bool allowMissingMethodBodies,
            bool deterministic,
            CancellationToken cancellationToken)
        {
            var heaps = new MetadataHeapsBuilder();
            return new FullMetadataWriter(context, heaps, messageProvider, allowMissingMethodBodies, deterministic, cancellationToken);
        }

        private FullMetadataWriter(
            EmitContext context,
            MetadataHeapsBuilder heaps,
            CommonMessageProvider messageProvider,
            bool allowMissingMethodBodies,
            bool deterministic,
            CancellationToken cancellationToken)
            : base(heaps, context, messageProvider, allowMissingMethodBodies, deterministic, cancellationToken)
        {
            // EDMAURER make some intelligent guesses for the initial sizes of these things.
            int numMethods = this.module.HintNumberOfMethodDefinitions;
            int numTypeDefsGuess = numMethods / 6;
            int numFieldDefsGuess = numTypeDefsGuess * 4;
            int numPropertyDefsGuess = numMethods / 4;

            _typeDefs = new DefinitionIndex<ITypeDefinition>(numTypeDefsGuess);
            _eventDefs = new DefinitionIndex<IEventDefinition>(0);
            _fieldDefs = new DefinitionIndex<IFieldDefinition>(numFieldDefsGuess);
            _methodDefs = new DefinitionIndex<IMethodDefinition>(numMethods);
            _propertyDefs = new DefinitionIndex<IPropertyDefinition>(numPropertyDefsGuess);
            _parameterDefs = new DefinitionIndex<IParameterDefinition>(numMethods);
            _genericParameters = new DefinitionIndex<IGenericParameter>(0);

            _fieldDefIndex = new Dictionary<ITypeDefinition, uint>(numTypeDefsGuess);
            _methodDefIndex = new Dictionary<ITypeDefinition, uint>(numTypeDefsGuess);
            _parameterListIndex = new Dictionary<IMethodDefinition, uint>(numMethods);

            _assemblyRefIndex = new HeapOrReferenceIndex<IAssemblyReference>(this, AssemblyReferenceComparer.Instance);
            _moduleRefIndex = new HeapOrReferenceIndex<string>(this);
            _memberRefIndex = new InstanceAndStructuralReferenceIndex<ITypeMemberReference>(this, new MemberRefComparer(this));
            _methodSpecIndex = new InstanceAndStructuralReferenceIndex<IGenericMethodInstanceReference>(this, new MethodSpecComparer(this));
            _typeRefIndex = new HeapOrReferenceIndex<ITypeReference>(this);
            _typeSpecIndex = new InstanceAndStructuralReferenceIndex<ITypeReference>(this, new TypeSpecComparer(this));
            _standAloneSignatureIndex = new HeapOrReferenceIndex<uint>(this);
        }

        protected override ushort Generation
        {
            get { return 0; }
        }

        protected override Guid EncId
        {
            get { return Guid.Empty; }
        }

        protected override Guid EncBaseId
        {
            get { return Guid.Empty; }
        }

        protected override bool CompressMetadataStream
        {
            get { return true; }
        }

        protected override bool TryGetTypeDefIndex(ITypeDefinition def, out uint index)
        {
            return _typeDefs.TryGetValue(def, out index);
        }

        protected override uint GetTypeDefIndex(ITypeDefinition def)
        {
            return _typeDefs[def];
        }

        protected override ITypeDefinition GetTypeDef(int index)
        {
            return _typeDefs[index];
        }

        protected override IReadOnlyList<ITypeDefinition> GetTypeDefs()
        {
            return _typeDefs.Rows;
        }

        protected override uint GetEventDefIndex(IEventDefinition def)
        {
            return _eventDefs[def];
        }

        protected override IReadOnlyList<IEventDefinition> GetEventDefs()
        {
            return _eventDefs.Rows;
        }

        protected override uint GetFieldDefIndex(IFieldDefinition def)
        {
            return _fieldDefs[def];
        }

        protected override IReadOnlyList<IFieldDefinition> GetFieldDefs()
        {
            return _fieldDefs.Rows;
        }

        protected override bool TryGetMethodDefIndex(IMethodDefinition def, out uint index)
        {
            return _methodDefs.TryGetValue(def, out index);
        }

        protected override uint GetMethodDefIndex(IMethodDefinition def)
        {
            return _methodDefs[def];
        }

        protected override IMethodDefinition GetMethodDef(int index)
        {
            return _methodDefs[index];
        }

        protected override IReadOnlyList<IMethodDefinition> GetMethodDefs()
        {
            return _methodDefs.Rows;
        }

        protected override uint GetPropertyDefIndex(IPropertyDefinition def)
        {
            return _propertyDefs[def];
        }

        protected override IReadOnlyList<IPropertyDefinition> GetPropertyDefs()
        {
            return _propertyDefs.Rows;
        }

        protected override uint GetParameterDefIndex(IParameterDefinition def)
        {
            return _parameterDefs[def];
        }

        protected override IReadOnlyList<IParameterDefinition> GetParameterDefs()
        {
            return _parameterDefs.Rows;
        }

        protected override IReadOnlyList<IGenericParameter> GetGenericParameters()
        {
            return _genericParameters.Rows;
        }

        protected override uint GetFieldDefIndex(INamedTypeDefinition typeDef)
        {
            return _fieldDefIndex[typeDef];
        }

        protected override uint GetMethodDefIndex(INamedTypeDefinition typeDef)
        {
            return _methodDefIndex[typeDef];
        }

        protected override uint GetParameterDefIndex(IMethodDefinition methodDef)
        {
            return _parameterListIndex[methodDef];
        }

        protected override uint GetOrAddAssemblyRefIndex(IAssemblyReference reference)
        {
            return _assemblyRefIndex.GetOrAdd(reference);
        }

        protected override IReadOnlyList<IAssemblyReference> GetAssemblyRefs()
        {
            return _assemblyRefIndex.Rows;
        }

        protected override uint GetOrAddModuleRefIndex(string reference)
        {
            return _moduleRefIndex.GetOrAdd(reference);
        }

        protected override IReadOnlyList<string> GetModuleRefs()
        {
            return _moduleRefIndex.Rows;
        }

        protected override uint GetOrAddMemberRefIndex(ITypeMemberReference reference)
        {
            return _memberRefIndex.GetOrAdd(reference);
        }

        protected override IReadOnlyList<ITypeMemberReference> GetMemberRefs()
        {
            return _memberRefIndex.Rows;
        }

        protected override uint GetOrAddMethodSpecIndex(IGenericMethodInstanceReference reference)
        {
            return _methodSpecIndex.GetOrAdd(reference);
        }

        protected override IReadOnlyList<IGenericMethodInstanceReference> GetMethodSpecs()
        {
            return _methodSpecIndex.Rows;
        }

        protected override bool TryGetTypeRefIndex(ITypeReference reference, out uint index)
        {
            return _typeRefIndex.TryGetValue(reference, out index);
        }

        protected override uint GetOrAddTypeRefIndex(ITypeReference reference)
        {
            return _typeRefIndex.GetOrAdd(reference);
        }

        protected override IReadOnlyList<ITypeReference> GetTypeRefs()
        {
            return _typeRefIndex.Rows;
        }

        protected override uint GetOrAddTypeSpecIndex(ITypeReference reference)
        {
            return _typeSpecIndex.GetOrAdd(reference);
        }

        protected override IReadOnlyList<ITypeReference> GetTypeSpecs()
        {
            return _typeSpecIndex.Rows;
        }

        protected override uint GetOrAddStandAloneSignatureIndex(uint blobIndex)
        {
            return _standAloneSignatureIndex.GetOrAdd(blobIndex);
        }

        protected override IReadOnlyList<uint> GetStandAloneSignatures()
        {
            return _standAloneSignatureIndex.Rows;
        }

        protected override ReferenceIndexer CreateReferenceVisitor()
        {
            return new FullReferenceIndexer(this);
        }

        protected override void ReportReferencesToAddedSymbols()
        {
            // noop
        }

        private sealed class FullReferenceIndexer : ReferenceIndexer
        {
            internal FullReferenceIndexer(MetadataWriter metadataWriter)
                : base(metadataWriter)
            {
            }
        }

        protected override void PopulateEncLogTableRows(List<EncLogRow> table, ImmutableArray<int> rowCounts)
        {
        }

        protected override void PopulateEncMapTableRows(List<EncMapRow> table, ImmutableArray<int> rowCounts)
        {
        }

        protected override void PopulateEventMapTableRows(List<EventMapRow> table)
        {
            ITypeDefinition lastParent = null;
            foreach (IEventDefinition eventDef in this.GetEventDefs())
            {
                if (eventDef.ContainingTypeDefinition == lastParent)
                {
                    continue;
                }

                lastParent = eventDef.ContainingTypeDefinition;
                uint eventIndex = this.GetEventDefIndex(eventDef);
                EventMapRow r = new EventMapRow();
                r.Parent = this.GetTypeDefIndex(lastParent);
                r.EventList = eventIndex;
                table.Add(r);
            }
        }

        protected override void PopulatePropertyMapTableRows(List<PropertyMapRow> table)
        {
            ITypeDefinition lastParent = null;
            foreach (IPropertyDefinition propertyDef in this.GetPropertyDefs())
            {
                if (propertyDef.ContainingTypeDefinition == lastParent)
                {
                    continue;
                }

                lastParent = propertyDef.ContainingTypeDefinition;
                uint propertyIndex = this.GetPropertyDefIndex(propertyDef);
                PropertyMapRow r = new PropertyMapRow();
                r.Parent = this.GetTypeDefIndex(lastParent);
                r.PropertyList = propertyIndex;
                table.Add(r);
            }
        }

        protected override IEnumerable<INamespaceTypeDefinition> GetTopLevelTypes(IModule module)
        {
            return module.GetTopLevelTypes(this.Context);
        }

        protected override void CreateIndicesForNonTypeMembers(ITypeDefinition typeDef)
        {
            _typeDefs.Add(typeDef);

            IEnumerable<IGenericTypeParameter> typeParameters = this.GetConsolidatedTypeParameters(typeDef);
            if (typeParameters != null)
            {
                foreach (IGenericTypeParameter genericParameter in typeParameters)
                {
                    _genericParameters.Add(genericParameter);
                }
            }

            foreach (MethodImplementation methodImplementation in typeDef.GetExplicitImplementationOverrides(Context))
            {
                this.methodImplList.Add(methodImplementation);
            }

            foreach (IEventDefinition eventDef in typeDef.Events)
            {
                _eventDefs.Add(eventDef);
            }

            _fieldDefIndex.Add(typeDef, _fieldDefs.NextRowId);
            foreach (IFieldDefinition fieldDef in typeDef.GetFields(Context))
            {
                _fieldDefs.Add(fieldDef);
            }

            _methodDefIndex.Add(typeDef, _methodDefs.NextRowId);
            foreach (IMethodDefinition methodDef in typeDef.GetMethods(Context))
            {
                this.CreateIndicesFor(methodDef);
                _methodDefs.Add(methodDef);
            }

            foreach (IPropertyDefinition propertyDef in typeDef.GetProperties(Context))
            {
                _propertyDefs.Add(propertyDef);
            }
        }

        private void CreateIndicesFor(IMethodDefinition methodDef)
        {
            _parameterListIndex.Add(methodDef, _parameterDefs.NextRowId);

            foreach (var paramDef in this.GetParametersToEmit(methodDef))
            {
                _parameterDefs.Add(paramDef);
            }

            if (methodDef.GenericParameterCount > 0)
            {
                foreach (IGenericMethodParameter genericParameter in methodDef.GenericParameters)
                {
                    _genericParameters.Add(genericParameter);
                }
            }
        }

        private struct DefinitionIndex<T> where T : IReference
        {
            private readonly Dictionary<T, uint> _index;
            private readonly List<T> _rows;

            public DefinitionIndex(int capacity)
            {
                _index = new Dictionary<T, uint>(capacity);
                _rows = new List<T>(capacity);
            }

            public bool TryGetValue(T item, out uint index)
            {
                return _index.TryGetValue(item, out index);
            }

            public uint this[T item]
            {
                get { return _index[item]; }
            }

            public T this[int index]
            {
                get { return _rows[index]; }
            }

            public IReadOnlyList<T> Rows
            {
                get { return _rows; }
            }

            public uint NextRowId
            {
                get { return (uint)_rows.Count + 1; }
            }

            public void Add(T item)
            {
                _index.Add(item, NextRowId);
                _rows.Add(item);
            }
        }
    }
}
