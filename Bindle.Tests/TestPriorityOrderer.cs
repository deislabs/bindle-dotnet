using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Deislabs.Bindle.Tests
{
    public class TestPriorityOrderer : ITestCaseOrderer
    {
        public IEnumerable<T> OrderTestCases<T>(IEnumerable<T> testCases) where T : ITestCase
        {
            var testcasesByPriority = new SortedDictionary<int, List<T>>();
            foreach (var testCase in testCases)
            {
                int priorty;
                var attr = testCase.TestMethod.Method.GetCustomAttributes(typeof(TestPriorityAttribute)).FirstOrDefault();
                priorty = attr == null ? 0 : attr.GetNamedArgument<int>("Priority");
                if (!testcasesByPriority.TryGetValue(priorty, out var list))
                {
                    list = new List<T>();
                    testcasesByPriority.Add(priorty, list);
                }
                list.Add(testCase);
            }
            Console.WriteLine($"Test Case Order:");
            foreach (var testCaseGroup in testcasesByPriority)
            {
                 Console.WriteLine($"Priority:{testCaseGroup.Key}");
                 foreach (var testCase in testCaseGroup.Value)
                 {
                     Console.WriteLine($"\t\t{testCase.TestMethod.Method.Name}");
                      yield return testCase;
                 }
            }
        }
    }
}