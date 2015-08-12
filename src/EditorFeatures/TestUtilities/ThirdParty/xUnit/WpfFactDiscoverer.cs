// Copyright 2014 Outercurve Foundation. Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
// https://github.com/xunit/samples.xunit/tree/f4d955a52d3298150ad2bd211c65a3477e184413

using System.Collections.Generic;
using System.Linq;

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Roslyn.Test.Utilities
{
    public class WpfFactDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly FactDiscoverer _factDiscoverer;

        public WpfFactDiscoverer(IMessageSink diagnosticMessageSink)
        {
            _factDiscoverer = new FactDiscoverer(diagnosticMessageSink);
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            return _factDiscoverer.Discover(discoveryOptions, testMethod, factAttribute)
                                  .Select(testCase => new WpfTestCase(testCase));
        }
    }
}