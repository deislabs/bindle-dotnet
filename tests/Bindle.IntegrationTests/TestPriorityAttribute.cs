using System;

namespace Deislabs.Bindle.IntegrationTests;

[AttributeUsage(AttributeTargets.Method)]
public class TestPriorityAttribute : Attribute
{
    public int Priority { get; private set; }
    public TestPriorityAttribute(int order)
    {
        Priority = order;
    }
}
