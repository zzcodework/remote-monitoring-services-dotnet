// Copyright (c) Microsoft. All rights reserved.

using System;
using Xunit.Abstractions;

namespace WebService.Test.helpers
{
    public class CIVariableHelper
    {
        public static bool IsPullRequest(ITestOutputHelper log)
        {
            const string CIVariable = "TRAVIS_PULL_REQUEST";
            try
            {
                var env = Environment.GetEnvironmentVariable("TRAVIS_PULL_REQUEST").ToLowerInvariant();
                log.WriteLine(CIVariable + " = " + env);
                return env != "false";
            }
            catch (Exception)
            {
                // Assume that we are running locally and return false so that we can run the test.
            }

            return false;
        }
    }
}
