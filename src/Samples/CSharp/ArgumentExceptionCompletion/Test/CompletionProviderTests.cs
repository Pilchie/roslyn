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

#if false
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion.Providers;
using Microsoft.CodeAnalysis.Text;
using Roslyn.UnitTestFramework;
using Xunit;

namespace ArgumentExceptionCompletionCS.UnitTests
{
    public class CompletionProviderTests : CompletionProviderTestFixture
    {
        public CompletionProviderTests()
            : base(LanguageNames.CSharp)
        {
        }

        [Fact]
        public void DoesNotWorkWhenNotInStringLiteral()
        {
            VerifyNoCompletion(@"
class C
{
    static void M(string a)
    {
        throw new System.ArgumentNullException($$);
    }
}");
        }

        [Fact]
        public void WorksInNewArgument()
        {
            VerifyCompletion(@"
class C
{
    static void M(string a)
    {
        throw new System.ArgumentNullException(""$$"");
    }
}");
        }

        [Fact]
        public void WorksInNewArgumentVerbatim()
        {
            VerifyCompletion(@"
class C
{
    static void M(string a)
    {
        throw new System.ArgumentNullException(@""$$"");
    }
}");
        }

        [Fact]
        public void WorksWithIncompleteEnd()
        {
            VerifyCompletion(@"
class C
{
    static void M(string a)
    {
        throw new System.ArgumentNullException(""$$
    }
}");
        }

        [Fact]
        public void WorksWithIncompleteEndVerbatim()
        {
            VerifyCompletion(@"
class C
{
    static void M(string a)
    {
        throw new System.ArgumentNullException(@""$$
    }
}");
        }

        [Fact]
        public void NotInUnboundConstructor()
        {
            VerifyNoCompletion(@"
class C
{
    static void M(string a)
    {
        throw new ArgumentNullException(""$$"");
    }
}");
        }

        [Fact]
        public void NotInNonSpecialConstructor()
        {
            VerifyNoCompletion(@"
class C
{
    static void M(string a)
    {
        throw new System.String(""$$"");
    }
}");
        }

        [Fact]
        public void NotIfNoParameters()
        {
            VerifyNoCompletion(@"
class C
{
    static void M()
    {
        throw new System.ArgumentNullException(""$$"");
    }
}");
        }

        [Fact]
        public void OnlyParameterPresent()
        {
            VerifyCompletionContains("a", @"
class C
{
    static void M(string a)
    {
        throw new System.ArgumentNullException(""$$"");
    }
}");
        }

        [Fact]
        public void FirstParameterPresent()
        {
            VerifyCompletionContains("a", @"
class C
{
    static void M(string a, string b)
    {
        throw new System.ArgumentNullException(""$$"");
    }
}");
        }

        [Fact]
        public void SecondParameterPresent()
        {
            VerifyCompletionContains("b", @"
class C
{
    static void M(string a, string b)
    {
        throw new System.ArgumentNullException(""$$"");
    }
}");
        }

        [Fact]
        public void InSimpleLambda()
        {
            VerifyCompletionContains("p", @"
class C
{
    static void M(string a, string b)
    {
        System.Action<string> action = p => 
        {
            throw new System.ArgumentNullException(""$$"");
        };
    }
}");
        }

        [Fact]
        public void InParenthesizedLambda()
        {
            VerifyCompletionContains("p1", @"
class C
{
    static void M(string a, string b)
    {
        System.Action<string, string> action = (p1, p2) =>
        {
            throw new System.ArgumentNullException(""$$"");
        };
    }
}");
        }

        [Fact]
        public void NotInInvalidPosition()
        {
            VerifyNoCompletion(@"
class C
{
    static void M(string a, string b)
    {
        throw new System.ArgumentNullException(""a"", ""$$"");
    }
}");
        }

        [Fact]
        public void NotWithInvalidName()
        {
            VerifyNoCompletion(@"
class C
{
    static void M(string a, string b)
    {
        throw new System.ArgumentNullException(message: ""$$"", paramName: ""a"");
    }
}");
        }

        [Fact]
        public void WithValidNameInDifferentPosition()
        {
            VerifyCompletion(@"
class C
{
    static void M(string a, string b)
    {
        throw new System.ArgumentNullException(message: ""m"", paramName: ""$$"");
    }
}");
        }

        [Fact]
        public void ValueInPropertySetter()
        {
            VerifyCompletionContains("value", @"
class C
{
    public string Property
    {
        set
        {
            throw new System.ArgumentNullException(""$$"");
        }
    }
}");
        }

        [Fact]
        public void NoValueInPropertyGetter()
        {
            VerifyCompletionDoesNotContain("value", @"
class C
{
    public string Property
    {
        get
        {
            throw new System.ArgumentNullException(""$$"");
        }
    }
}");
        }

        [Fact]
        public void InEventAddAccessor()
        {
            VerifyCompletionContains("value", @"
class C
{
    public event EventyHandler Event
    {
        add
        {
            throw new System.ArgumentNullException(""$$"");
        }
    }
}");
        }

        [Fact]
        public void InEventRemoveAccessor()
        {
            VerifyCompletionContains("value", @"
class C
{
    public event EventyHandler Event
    {
        remove
        {
            throw new System.ArgumentNullException(""$$"");
        }
    }
}");
        }

        [Fact]
        public void ValueInIndexerSetter()
        {
            VerifyCompletionContains("value", @"
class C
{
    public string this[string p]
    {
        set
        {
            throw new System.ArgumentNullException(""$$"");
        }
    }
}");
        }

        [Fact]
        public void ParameterInIndexerSetter()
        {
            VerifyCompletionContains("p", @"
class C
{
    public string this[string p]
    {
        set
        {
            throw new System.ArgumentNullException(""$$"");
        }
    }
}");
        }

        [Fact]
        public void ParameterInIndexerGetter()
        {
            VerifyCompletionContains("p", @"
class C
{
    public string this[string p]
    {
        get
        {
            throw new System.ArgumentNullException(""$$"");
        }
    }
}");
        }

        [Fact]
        public void NoValueInIndexerGetter()
        {
            VerifyCompletionDoesNotContain("value", @"
class C
{
    public string this[string p]
    {
        get
        {
            throw new System.ArgumentNullException(""$$"");
        }
    }
}");
        }

        protected override ICompletionProvider CreateProvider()
        {
            return new CompletionProvider();
        }
    }
}
#endif