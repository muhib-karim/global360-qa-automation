namespace SnipeIt.UiTests.Support;

internal sealed record AssetData(string Tag, string Name, string Serial, string Notes)
{
    public const string Model = "Macbook Pro 13\"";
    public const string Status = "Ready to Deploy";

    public static AssetData Unique()
    {
        var id = Guid.NewGuid().ToString("N")[..12];
        return new AssetData($"QA-{id}", $"QA Macbook {id}", $"QA-SERIAL-{id}",
            $"Automated UI assessment asset {id}");
    }
}

internal sealed record SelectedUser(string Id, string DisplayName, string OptionText);
