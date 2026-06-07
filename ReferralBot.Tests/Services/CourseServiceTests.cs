using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

using ReferralBot.Core.Models;
using ReferralBot.Services;

namespace ReferralBot.Tests.Services;

public class CourseServiceTests
{
    private const int TeacherId = 596721262;

    private static IConfiguration Config(int teacherId = TeacherId) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["STEPIK_TEACHER_ID"] = teacherId.ToString()
            })
            .Build();

    private static CourseService CreateSut(IStepikApiClient stepik, IConfiguration config) =>
        new(
            stepik,
            Mock.Of<IHttpClientFactory>(),
            new MemoryCache(new MemoryCacheOptions()),
            config,
            NullLogger<CourseService>.Instance);

    [Fact]
    public async Task GetCoursesIdTitleAsync_ReturnsOnlyPublishedActive_OrderedByPosition()
    {
        var stepik = new Mock<IStepikApiClient>();
        stepik.Setup(s => s.GetTeacherCoursesAsync(TeacherId, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StepikCourse>
            {
                new() { Id = 3, Title = "C", Position = 2, IsPublic = true,  IsActive = true,  IsArchived = false },
                new() { Id = 1, Title = "A", Position = 1, IsPublic = true,  IsActive = true,  IsArchived = false },
                new() { Id = 2, Title = "Draft",    IsPublic = false, IsActive = true,  IsArchived = false },
                new() { Id = 4, Title = "Archived", IsPublic = true,  IsActive = true,  IsArchived = true  },
                new() { Id = 5, Title = "Inactive", IsPublic = true,  IsActive = false, IsArchived = false },
            });

        var sut = CreateSut(stepik.Object, Config());

        var result = await sut.GetCoursesIdTitleAsync();

        result.Should().HaveCount(2);
        result.Select(c => c.Id).Should().ContainInOrder(1, 3);       // отсортировано по Position
        result.Select(c => c.Title).Should().ContainInOrder("A", "C");
    }

    [Fact]
    public async Task GetCoursesIdTitleAsync_NoTeacherId_ReturnsEmpty_AndDoesNotCallApi()
    {
        var stepik = new Mock<IStepikApiClient>();
        var sut = CreateSut(stepik.Object, Config(teacherId: 0));

        var result = await sut.GetCoursesIdTitleAsync();

        result.Should().BeEmpty();
        stepik.Verify(
            s => s.GetTeacherCoursesAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCoursesIdTitleAsync_CachesResult_HitsApiOnce()
    {
        var stepik = new Mock<IStepikApiClient>();
        stepik.Setup(s => s.GetTeacherCoursesAsync(TeacherId, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StepikCourse>
            {
                new() { Id = 1, Title = "A", Position = 1, IsPublic = true, IsActive = true, IsArchived = false },
            });

        var sut = CreateSut(stepik.Object, Config());

        await sut.GetCoursesIdTitleAsync();
        await sut.GetCoursesIdTitleAsync();

        stepik.Verify(
            s => s.GetTeacherCoursesAsync(TeacherId, It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCourseInfoAsync_MapsFields_AndParsesPrice()
    {
        var stepik = new Mock<IStepikApiClient>();
        stepik.Setup(s => s.GetCourseByIdAsync(107, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StepikCourse { Id = 107, Title = "PRO C#", Summary = "Описание", Price = "68900.00" });

        var sut = CreateSut(stepik.Object, Config());

        var result = await sut.GetCourseInfoAsync(107);

        result.Should().NotBeNull();
        result!.Id.Should().Be(107);
        result.Title.Should().Be("PRO C#");
        result.Summary.Should().Be("Описание");
        result.Price.Should().Be(68900m);
    }

    [Fact]
    public async Task GetCourseInfoAsync_CourseNotFound_ReturnsNull()
    {
        var stepik = new Mock<IStepikApiClient>();
        stepik.Setup(s => s.GetCourseByIdAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StepikCourse?)null);

        var sut = CreateSut(stepik.Object, Config());

        var result = await sut.GetCourseInfoAsync(999);

        result.Should().BeNull();
    }
}
