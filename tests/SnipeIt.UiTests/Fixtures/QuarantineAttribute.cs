namespace SnipeIt.UiTests.Fixtures;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class QuarantineAttribute(string reason, string until) : NUnitAttribute
{
    public string Reason { get; } = reason;

    public string Until { get; } = until;
}
