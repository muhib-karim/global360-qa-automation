namespace SnipeIt.UiTests.Data;

internal sealed record AssetData(string Tag, string Name, string Serial, string Notes)
{
    public const string Model = "Macbook Pro 13\"";
    public const string Status = "Ready to Deploy";
}

internal sealed record SelectedUser(string Id, string DisplayName, string OptionText);

/// <summary>Builds asset data that is unique per run, so tests never depend on or collide with shared demo data.</summary>
internal sealed class AssetDataBuilder
{
    private string _id = Guid.NewGuid().ToString("N")[..12];

    public AssetDataBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public AssetData Build() =>
        new($"QA-{_id}", $"QA Macbook {_id}", $"QA-SERIAL-{_id}", $"Automated UI assessment asset {_id}");
}
