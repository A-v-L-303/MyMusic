namespace MyMusic.Application.Tests.Features.Stammdaten.Label.Commands.Update;

public class UpdateLabelCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_EigenesLabel_AktualisiertWerteUndGibtResponseZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var existingLabel = LabelEntity.Create("Rough Trade", 1, null, userId);

        var command = new UpdateLabelCommand
        {
            Id = existingLabel.Id,
            Name = "Sub Pop",
            CountryId = 2,
            UserId = userId
        };

        var repository = Substitute.For<IRepository<LabelEntity>>();

        repository.GetByIdAsync(existingLabel.Id, Arg.Any<CancellationToken>()).Returns(existingLabel);

        StubConflictingCount(repository, 0);

        var countryRepository = Substitute.For<IRepository<CountryEntity>>();

        countryRepository.GetByIdAsync(2, Arg.Any<CancellationToken>())
            .Returns(CountryEntity.Create("USA", "US"));

        var handler = new UpdateLabelCommandHandler(
            repository, countryRepository, new ExceptionManager(), new LabelResponseBuilder());

        // act
        var response = await handler.HandleAsync(command, CancellationToken.None);

        // assert
        Assert.Equal("Sub Pop", response.Name);
        Assert.Equal(2, response.CountryId);
        Assert.Equal("USA", response.CountryName);

        repository.Received(1).Update(Arg.Is<LabelEntity>(label => label != null && label.Name == "Sub Pop"));

        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UnbekanntesLabel_WirftNotFoundException()
    {
        // arrange
        var command = new UpdateLabelCommand { Id = 1, Name = "Sub Pop", CountryId = 1, UserId = Guid.NewGuid() };

        var repository = Substitute.For<IRepository<LabelEntity>>();

        repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((LabelEntity?)null);

        var countryRepository = Substitute.For<IRepository<CountryEntity>>();

        var handler = new UpdateLabelCommandHandler(
            repository, countryRepository, new ExceptionManager(), new LabelResponseBuilder());

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_FremdesLabel_WirftNotFoundExceptionStattForbidden()
    {
        // arrange
        var fremdesLabel = LabelEntity.Create("Rough Trade", 1, null, Guid.NewGuid());

        var command = new UpdateLabelCommand
        {
            Id = fremdesLabel.Id,
            Name = "Sub Pop",
            CountryId = 1,
            UserId = Guid.NewGuid()
        };

        var repository = Substitute.For<IRepository<LabelEntity>>();

        repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(fremdesLabel);

        var countryRepository = Substitute.For<IRepository<CountryEntity>>();

        var handler = new UpdateLabelCommandHandler(
            repository, countryRepository, new ExceptionManager(), new LabelResponseBuilder());

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert: 404 statt 403 - Existenz einer fremden Ressource wird nicht bestätigt
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_NameBereitsBeiAnderemEigenenLabelVorhanden_WirftConflictException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var existingLabel = LabelEntity.Create("Rough Trade", 1, null, userId);

        var command = new UpdateLabelCommand
        {
            Id = existingLabel.Id,
            Name = "Sub Pop",
            CountryId = 1,
            UserId = userId
        };

        var repository = Substitute.For<IRepository<LabelEntity>>();

        repository.GetByIdAsync(existingLabel.Id, Arg.Any<CancellationToken>()).Returns(existingLabel);

        StubConflictingCount(repository, 1);

        var countryRepository = Substitute.For<IRepository<CountryEntity>>();

        var handler = new UpdateLabelCommandHandler(
            repository, countryRepository, new ExceptionManager(), new LabelResponseBuilder());

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ConflictException>(act);
    }

    [Fact]
    public async Task HandleAsync_PruefungSchliesstDenEigenenDatensatzAus()
    {
        // arrange
        var userId = Guid.NewGuid();

        var existingLabel = LabelEntity.Create("Rough Trade", 1, null, userId);

        var command = new UpdateLabelCommand
        {
            Id = existingLabel.Id,
            Name = "Rough Trade",
            CountryId = 1,
            UserId = userId
        };

        var repository = Substitute.For<IRepository<LabelEntity>>();

        repository.GetByIdAsync(existingLabel.Id, Arg.Any<CancellationToken>()).Returns(existingLabel);

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

        var handler = new UpdateLabelCommandHandler(
            repository, countryRepository, new ExceptionManager(), new LabelResponseBuilder());

        // act
        await handler.HandleAsync(command, CancellationToken.None);

        // assert
        Assert.NotNull(capturedFilter);

        var predicate = capturedFilter!.Compile();

        // der bearbeitete Datensatz selbst darf keinen Konflikt mit sich selbst auslösen
        Assert.False(predicate(existingLabel));
    }

    private static void StubConflictingCount(IRepository<LabelEntity> repository, int totalCount)
    {
        var items = totalCount > 0
            ? new List<LabelEntity> { LabelEntity.Create("Sub Pop", 1, null, Guid.NewGuid()) }
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
