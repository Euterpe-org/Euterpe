using Euterpe.Contracts.Account;
using Euterpe.Contracts.Telemetry;

namespace Euterpe.Tests.Contracts;

[Category("RecordContractsTests")]
public sealed class RecordContractsTest
{
    [Test]
    public async Task MuseDashUidRequest_Record_ExposesUidAndSupportsValueEquality()
    {
        var a = new MuseDashUidRequest("abc");
        var b = new MuseDashUidRequest("abc");
        var c = new MuseDashUidRequest("def");

        using var _ = Assert.Multiple();
        await Assert.That(a.Uid).IsEqualTo("abc");
        await Assert.That(a).IsEqualTo(b);
        await Assert.That(a).IsNotEqualTo(c);
    }

    [Test]
    public async Task SessionEvent_Record_ExposesAllFieldsAndSupportsValueEquality()
    {
        var a = new SessionEvent("US", "Linux", "x64", "2.0.0");
        var b = new SessionEvent("US", "Linux", "x64", "2.0.0");
        var c = a with { Country = "JP" };

        using var _ = Assert.Multiple();
        await Assert.That(a.Country).IsEqualTo("US");
        await Assert.That(a.Platform).IsEqualTo("Linux");
        await Assert.That(a.Arch).IsEqualTo("x64");
        await Assert.That(a.AppVersion).IsEqualTo("2.0.0");
        await Assert.That(a).IsEqualTo(b);
        await Assert.That(a).IsNotEqualTo(c);
        await Assert.That(c.Country).IsEqualTo("JP");
    }
}