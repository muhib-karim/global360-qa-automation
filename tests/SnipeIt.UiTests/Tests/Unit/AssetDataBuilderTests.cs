using SnipeIt.UiTests.Data;

namespace SnipeIt.UiTests.Tests.Unit;

[TestFixture]
[Category("Unit")]
public sealed class AssetDataBuilderTests
{
    [Test]
    public void Every_build_gets_a_unique_tag()
    {
        var tags = Enumerable.Range(0, 1000).Select(_ => new AssetDataBuilder().Build().Tag).ToList();

        Assert.That(tags, Is.Unique.And.All.Match(@"^QA-[0-9a-f]{12}$"));
    }

    [Test]
    public void All_fields_share_the_id()
    {
        var asset = new AssetDataBuilder().WithId("abc").Build();

        Assert.That(asset, Is.EqualTo(new AssetData("QA-abc", "QA Macbook abc", "QA-SERIAL-abc", "Automated UI assessment asset abc")));
    }
}
