using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Deislabs.Bindle.IntegrationTests;

public class TestPriorityOrderer : ITestCaseOrderer
{
    public IEnumerable<T> OrderTestCases<T>(IEnumerable<T> testCases) where T : ITestCase
    {
        var testcasesByPriority = new SortedDictionary<int, List<T>>();
        foreach (var testCase in testCases)
        {
            int priority;
            var attr = testCase.TestMethod.Method.GetCustomAttributes(typeof(TestPriorityAttribute)).FirstOrDefault();
            priority = attr is null ? 0 : attr.GetNamedArgument<int>("Priority");
            if (!testcasesByPriority.TryGetValue(priority, out var list))
            {
                list = new List<T>();
                testcasesByPriority.Add(priority, list);
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
