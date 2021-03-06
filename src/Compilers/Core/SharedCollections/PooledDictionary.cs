﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Collections
{
    // Dictionary that can be recycled via an object pool
    // NOTE: these dictionaries always have the default comparer.
    internal class PooledDictionary<K, V> : Dictionary<K, V>
    {
        private readonly ObjectPool<PooledDictionary<K, V>> pool;

        private PooledDictionary(ObjectPool<PooledDictionary<K, V>> pool)
            : base()
        {
            this.pool = pool;
        }

        public void Free()
        {
            this.Clear();
            if (pool != null)
            {
                pool.Free(this);
            }
        }

        // global pool
        private static readonly ObjectPool<PooledDictionary<K, V>> PoolInstance = CreatePool();

        // if someone needs to create a pool;
        public static ObjectPool<PooledDictionary<K, V>> CreatePool()
        {
            ObjectPool<PooledDictionary<K, V>> pool = null;
            pool = new ObjectPool<PooledDictionary<K, V>>(() => new PooledDictionary<K, V>(pool), 128);
            return pool;
        }

        public static PooledDictionary<K, V> GetInstance()
        {
            var instance = PoolInstance.Allocate();
            Debug.Assert(instance.Count == 0);
            return instance;
        }
    }
}
