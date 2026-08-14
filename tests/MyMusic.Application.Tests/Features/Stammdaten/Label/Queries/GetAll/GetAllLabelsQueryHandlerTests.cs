namespace MyMusic.Application.Tests.Features.Stammdaten.Label.Queries.GetAll;

public class GetAllLabelsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_RuftGetPagedAsyncMitSeite1UndMaximalerSeitengroesseAufUndLaenderAuflösungAuf()
    {
        // arrange
        var userId = Guid.NewGuid();

        var repository = Substitute.For<IRepository<LabelEntity>>();

        // Leere Items-Liste: CountryEntity.Create liefert stets Id 0 (interner Konstruktor, kein
        // InternalsVisibleTo für Tests), während Label.CountryId zwingend > 0 sein muss - eine
        // nicht-leere Liste würde die Dictionary-Auflösung mit einer nicht erzeugbaren
        // Test-Konstellation kollidieren lassen (bekannte Einschränkung, siehe
        // GetPagedLabelsQueryHandlerTests). Die eigentliche Mapping-Logik wird in
        // LabelResponseBuilderTests mit einem frei wählbaren Dictionary geprüft.
        repository.GetPagedAsync(
                Arg.Any<Expression<Func<LabelEntity, bool>>>(),
                Arg.Any<Func<IQueryable<LabelEntity>, IOrderedQueryable<LabelEntity>>>(),
                1,
                int.MaxValue,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<LabelEntity>)new List<LabelEntity>(), TotalCount: 0));

        var countryRepository = Substitute.For<IRepository<CountryEntity>>();

        countryRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<CountryEntity>());

        var handler = new GetAllLabelsQueryHandler(repository, countryRepository, new LabelResponseBuilder());

        // act
        var response = await handler.HandleAsync(new GetAllLabelsQuery(userId), CancellationToken.None);

        // assert
        Assert.Empty(response);

        await countryRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_FilterBeschraenktAufEigeneLabels()
    {
        // arrange
        var userId = Guid.NewGuid();

        var repository = Substitute.For<IRepository<LabelEntity>>();

        Expression<Func<LabelEntity, bool>>? capturedFilter = null;

        repository.GetPagedAsync(
                Arg.Do<Expression<Func<LabelEntity, bool>>>(filter => capturedFilter = filter),
                Arg.Any<Func<IQueryable<LabelEntity>, IOrderedQueryable<LabelEntity>>>(),
                1,
                int.MaxValue,
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<LabelEntity>)new List<LabelEntity>(), TotalCount: 0));

        var countryRepository = Substitute.For<IRepository<CountryEntity>>();

        countryRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<CountryEntity>());

        var handler = new GetAllLabelsQueryHandler(repository, countryRepository, new LabelResponseBuilder());

        // act
        await handler.HandleAsync(new GetAllLabelsQuery(userId), CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var eigenesLabel = LabelEntity.Create("Rough Trade", 1, null, userId);

        var fremdesLabel = LabelEntity.Create("Rough Trade", 1, null, Guid.NewGuid());

        Assert.True(predicate(eigenesLabel));

        // Mandantentrennung: fremde Labels dürfen nicht mitgeliefert werden
        Assert.False(predicate(fremdesLabel));
    }
}
