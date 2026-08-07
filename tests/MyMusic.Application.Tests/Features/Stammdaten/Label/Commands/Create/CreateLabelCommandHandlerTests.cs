namespace MyMusic.Application.Tests.Features.Stammdaten.Label.Commands.Create;

public class CreateLabelCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_NeuerName_LegtLabelAnUndGibtResponseZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var command = new CreateLabelCommand { Name = "Rough Trade", CountryId = 1, UserId = userId };

        var repository = Substitute.For<IRepository<LabelEntity>>();

        StubExistingCount(repository, 0);

        var countryRepository = Substitute.For<IRepository<CountryEntity>>();

        countryRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(CountryEntity.Create("Vereinigtes Königreich", "GB"));

        var handler = new CreateLabelCommandHandler(
            repository, countryRepository, new ExceptionManager(), new LabelResponseBuilder());

        // act
        var response = await handler.HandleAsync(command, CancellationToken.None);

        // assert
        Assert.Equal("Rough Trade", response.Name);
        Assert.Equal(1, response.CountryId);
        Assert.Equal("Vereinigtes Königreich", response.CountryName);

        await repository.Received(1).AddAsync(
            Arg.Is<LabelEntity>(label => label != null && label.Name == "Rough Trade" && label.UserId == userId),
            Arg.Any<CancellationToken>());

        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_NameBereitsVorhandenFuerBenutzer_WirftConflictException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var command = new CreateLabelCommand { Name = "Rough Trade", CountryId = 1, UserId = userId };

        var repository = Substitute.For<IRepository<LabelEntity>>();

        StubExistingCount(repository, 1);

        var countryRepository = Substitute.For<IRepository<CountryEntity>>();

        var handler = new CreateLabelCommandHandler(
            repository, countryRepository, new ExceptionManager(), new LabelResponseBuilder());

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ConflictException>(act);

        await repository.DidNotReceive().AddAsync(Arg.Any<LabelEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_PruefungBeschraenktSichAufEigeneLabelsDesBenutzers()
    {
        // arrange
        var userId = Guid.NewGuid();

        var command = new CreateLabelCommand { Name = "Rough Trade", CountryId = 1, UserId = userId };

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

        countryRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(CountryEntity.Create("Vereinigtes Königreich", "GB"));

        var handler = new CreateLabelCommandHandler(
            repository, countryRepository, new ExceptionManager(), new LabelResponseBuilder());

        // act
        await handler.HandleAsync(command, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        var eigenesLabelGleicherName = LabelEntity.Create("Rough Trade", 1, null, userId);

        var fremdesLabelGleicherName = LabelEntity.Create("Rough Trade", 1, null, Guid.NewGuid());

        // gleicher Name eines anderen Benutzers darf keinen Konflikt auslösen (Mandantentrennung)
        Assert.True(predicate(eigenesLabelGleicherName));
        Assert.False(predicate(fremdesLabelGleicherName));
    }

    private static void StubExistingCount(IRepository<LabelEntity> repository, int totalCount)
    {
        var items = totalCount > 0
            ? new List<LabelEntity> { LabelEntity.Create("Rough Trade", 1, null, Guid.NewGuid()) }
            : new List<LabelEntity>();

        repository.GetPagedAsync(
                Arg.Any<Expression<Func<LabelEntity, bool>>>(),
                Arg.Any<Func<IQueryable<LabelEntity>, IOrderedQueryable<LabelEntity>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((Items: (IReadOnlyList<LabelEntity>)items, TotalCount: totalCount));
    }
}
