// Copyright 2014 Outercurve Foundation. Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
// https://github.com/xunit/samples.xunit/tree/f4d955a52d3298150ad2bd211c65a3477e184413

using System.Collections.Generic;
using System.Linq;

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Roslyn.Test.Utilities
{
    public class WpfTheoryDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly TheoryDiscoverer _theoryDiscoverer;

        public WpfTheoryDiscoverer(IMessageSink diagnosticMessageSink)
        {
            _theoryDiscoverer = new TheoryDiscoverer(diagnosticMessageSink);
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            return _theoryDiscoverer.Discover(discoveryOptions, testMethod, factAttribute)
                                   .Select(testCase => new WpfTestCase(testCase));
        }
    }
}
