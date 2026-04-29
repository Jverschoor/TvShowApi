using AutoFixture;
using AutoFixture.AutoNSubstitute;
using TvShows.Application.Entities;

namespace TvShows.Tests._support;

internal static class ShowFixture
{
    public static IFixture Create()
    {
        var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        fixture.Customize<Show>(c => c
            .With(s => s.Premiered, new DateTime(2020, 1, 1))
            .With(s => s.Genres, new List<string> { "Drama" }));
        return fixture;
    }

    public static Show Show(int? tvMazeId = null, DateTime? premiered = null, string name = "Show") => new()
    {
        TvMazeId = tvMazeId,
        Name = name,
        Language = "English",
        Premiered = premiered ?? new DateTime(2020, 1, 1),
        Genres = new List<string> { "Drama" },
        Summary = "Summary"
    };
}
