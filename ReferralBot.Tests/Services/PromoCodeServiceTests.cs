using Microsoft.Extensions.Logging.Abstractions;

using ReferralBot.Core.Services;
using ReferralBot.Db.Entities;
using ReferralBot.Db.Interfaces;

namespace ReferralBot.Tests.Services;

public class PromoCodeServiceTests
{
    private static PromoCodeService CreateSut(IPromoCodesStorage storage) =>
        new(storage, NullLogger<PromoCodeService>.Instance);

    [Fact]
    public async Task GetHexForPaymentAsync_PromoExists_ReturnsHex()
    {
        var accountId = Guid.NewGuid();
        var storage = new Mock<IPromoCodesStorage>();
        storage.Setup(s => s.GetByAccountAndCourseAsync(accountId, 107, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PromoCodeEntity { AccountId = accountId, CourseId = 107, Hex = "abc123" });

        var sut = CreateSut(storage.Object);

        var hex = await sut.GetHexForPaymentAsync(107, accountId);

        hex.Should().Be("abc123");
    }

    [Fact]
    public async Task GetHexForPaymentAsync_NoPromo_ReturnsNull()
    {
        var storage = new Mock<IPromoCodesStorage>();
        storage.Setup(s => s.GetByAccountAndCourseAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PromoCodeEntity?)null);

        var sut = CreateSut(storage.Object);

        var hex = await sut.GetHexForPaymentAsync(107, Guid.NewGuid());

        hex.Should().BeNull();
    }

    [Fact]
    public async Task GetHexForPaymentAsync_EmptyHex_ReturnsNull()
    {
        var storage = new Mock<IPromoCodesStorage>();
        storage.Setup(s => s.GetByAccountAndCourseAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PromoCodeEntity { Hex = string.Empty });

        var sut = CreateSut(storage.Object);

        var hex = await sut.GetHexForPaymentAsync(107, Guid.NewGuid());

        hex.Should().BeNull();
    }
}
