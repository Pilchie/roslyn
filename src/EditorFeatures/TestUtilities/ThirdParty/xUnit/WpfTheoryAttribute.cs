// Copyright 2014 Outercurve Foundation. Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
// https://github.com/xunit/samples.xunit/tree/f4d955a52d3298150ad2bd211c65a3477e184413

using System;

using Xunit;
using Xunit.Sdk;

namespace Roslyn.Test.Utilities
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("Roslyn.Test.Utilities.WpfTheoryDiscoverer", "Roslyn.Services.Test.Utilities")]
    public class WpfTheoryAttribute : TheoryAttribute
    {
    }
}
