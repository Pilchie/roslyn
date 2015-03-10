﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis
{
    public partial struct SyntaxTriviaList
    {
        /// <summary>
        /// reversed enumerable
        /// </summary>
        public struct Reversed : IEnumerable<SyntaxTrivia>, IEquatable<Reversed>
        {
            private SyntaxTriviaList _list;

            public Reversed(SyntaxTriviaList list)
            {
                _list = list;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(ref _list);
            }

            IEnumerator<SyntaxTrivia> IEnumerable<SyntaxTrivia>.GetEnumerator()
            {
                if (_list.Count == 0)
                {
                    return SpecializedCollections.EmptyEnumerator<SyntaxTrivia>();
                }

                return new ReversedEnumeratorImpl(ref _list);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                if (_list.Count == 0)
                {
                    return SpecializedCollections.EmptyEnumerator<SyntaxTrivia>();
                }

                return new ReversedEnumeratorImpl(ref _list);
            }

            public override int GetHashCode()
            {
                return _list.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return (obj is Reversed) && Equals((Reversed)obj);
            }

            public bool Equals(Reversed other)
            {
                return _list.Equals(other._list);
            }

            public struct Enumerator
            {
                private readonly SyntaxToken _token;
                private readonly GreenNode _singleNodeOrList;
                private readonly int _baseIndex;
                private readonly int _count;

                private int _index;
                private GreenNode _current;
                private int _position;

                public Enumerator(ref SyntaxTriviaList list)
                    : this()
                {
                    if (list.Any())
                    {
                        _token = list._token;
                        _singleNodeOrList = list._node;
                        _baseIndex = list._index;
                        _count = list.Count;

                        _index = _count;
                        _current = null;

                        var last = list.Last();
                        _position = last.Position + last.FullWidth;
                    }
                }

                public bool MoveNext()
                {
                    if (_count == 0 || _index <= 0)
                    {
                        _current = null;
                        return false;
                    }

                    _index--;

                    _current = GetGreenNodeAt(_singleNodeOrList, _index);
                    _position -= _current.FullWidth;

                    return true;
                }

                public SyntaxTrivia Current
                {
                    get
                    {
                        if (_current == null)
                        {
                            throw new InvalidOperationException();
                        }

                        return new SyntaxTrivia(_token, _current, _position, _baseIndex + _index);
                    }
                }
            }

            private class ReversedEnumeratorImpl : IEnumerator<SyntaxTrivia>
            {
                private Enumerator _enumerator;

                // SyntaxTriviaList is a relatively big struct so is passed as ref
                internal ReversedEnumeratorImpl(ref SyntaxTriviaList list)
                {
                    _enumerator = new Enumerator(ref list);
                }

                public SyntaxTrivia Current
                {
                    get { return _enumerator.Current; }
                }

                object IEnumerator.Current
                {
                    get { return _enumerator.Current; }
                }

                public bool MoveNext()
                {
                    return _enumerator.MoveNext();
                }

                public void Reset()
                {
                    throw new NotSupportedException();
                }

                public void Dispose()
                {
                }
            }
        }
    }
}
