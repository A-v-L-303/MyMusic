namespace MyMusic.Application.Tests.Features.Stammdaten.Label.Queries.GetPaged;

public class GetPagedLabelsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_LeitetSeitenparameterWeiterUndRuftLaenderAuflösungAuf()
    {
        // arrange
        var userId = Guid.NewGuid();

        var repository = Substitute.For<IRepository<LabelEntity>>();

        // Leere Items-Liste: CountryEntity.Create liefert stets Id 0 (interner Konstruktor,
        // kein InternalsVisibleTo für Tests), während Label.CountryId zwingend > 0 sein muss -
        // eine nicht-leere Liste würde die Dictionary-Auflösung im Builder mit einer nicht
        // erzeugbaren Test-Konstellation kollidieren lassen. Die eigentliche Zuordnungslogik
        // wird stattdessen in LabelResponseBuilderTests mit einem frei wählbaren Dictionary
        // geprüft (bekannte Einschränkung, analog GetPagedAsync bei Genre/Country).
        repository.GetPagedAsync(
                Arg.Any<Expression<Func<LabelEntity, bool>>>(),
                Arg.Any<Func<IQueryable<LabelEntity>, IOrderedQueryable<LabelEntity>>>(),
                2,
                10,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<LabelEntity>)new List<LabelEntity>(), TotalCount: 12));

        var countryRepository = Substitute.For<IRepository<CountryEntity>>();

        countryRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<CountryEntity>());

        var handler = new GetPagedLabelsQueryHandler(repository, countryRepository, new LabelResponseBuilder());

        var query = new GetPagedLabelsQuery(userId, Page: 2, PageSize: 10, Name: null, CountryId: null);

        // act
        var response = await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.Equal(12, response.TotalCount);
        Assert.Equal(2, response.Page);
        Assert.Equal(10, response.PageSize);

        await countryRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_FilterBeschraenktAufEigeneLabelsNamenUndLand()
    {
        // arrange
        var userId = Guid.NewGuid();

        var repository = Substitute.For<IRepository<LabelEntity>>();

        Expression<Func<LabelEntity, bool>>? capturedFilter = null;

        repository.GetPagedAsync(
                Arg.Do<Expression<Func<LabelEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Func<IQueryable<LabelEntity>, IOrderedQueryable<LabelEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<LabelEntity>)new List<LabelEntity>(), TotalCount: 0));

        var countryRepository = Substitute.For<IRepository<CountryEntity>>();

        countryRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<CountryEntity>());

        var handler = new GetPagedLabelsQueryHandler(repository, countryRepository, new LabelResponseBuilder());

        var query = new GetPagedLabelsQuery(userId, Page: 1, PageSize: 20, Name: "rough", CountryId: 1);

        // act
        await handler.HandleAsync(query, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var eigenesTreffer = LabelEntity.Create("Rough Trade", 1, null, userId);

        var eigenesFalschesLand = LabelEntity.Create("Rough Trade", 2, null, userId);

        var eigenesFalscherName = LabelEntity.Create("Sub Pop", 1, null, userId);

        var fremdesTreffer = LabelEntity.Create("Rough Trade", 1, null, Guid.NewGuid());

        Assert.True(predicate(eigenesTreffer));
        Assert.False(predicate(eigenesFalschesLand));
        Assert.False(predicate(eigenesFalscherName));

        // Mandantentrennung gilt auch für die Namens- und Landfilterung der Liste
        Assert.False(predicate(fremdesTreffer));
    }
}
