namespace SnipeIt.UiTests.Fixtures;

/// <summary>
/// Moves a flaky test out of the gating run (filter <c>TestCategory!=Quarantine</c>) into the non-blocking
/// quarantine job, for a limited time. The reason names the tracking issue; <paramref name="until"/> is a
/// yyyy-MM-dd date after which a unit test fails the build. See "Quarantining a flaky test" in CONTRIBUTING.md.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class QuarantineAttribute(string reason, string until) : CategoryAttribute
{
    public string Reason { get; } = reason;

    public string Until { get; } = until;
}
