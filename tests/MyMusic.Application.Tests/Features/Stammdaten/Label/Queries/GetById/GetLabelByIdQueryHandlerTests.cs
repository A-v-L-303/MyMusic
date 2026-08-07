namespace MyMusic.Application.Tests.Features.Stammdaten.Label.Queries.GetById;

public class GetLabelByIdQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_EigenesLabel_GibtResponseMitLandnamenZurueck()
    {
        // arrange
        var userId = Guid.NewGuid();

        var label = LabelEntity.Create("Rough Trade", 1, null, userId);

        var repository = Substitute.For<IRepository<LabelEntity>>();

        repository.GetByIdAsync(label.Id, Arg.Any<CancellationToken>()).Returns(label);

        var countryRepository = Substitute.For<IRepository<CountryEntity>>();

        countryRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(CountryEntity.Create("Vereinigtes Königreich", "GB"));

        var handler = new GetLabelByIdQueryHandler(
            repository, countryRepository, new ExceptionManager(), new LabelResponseBuilder());

        // act
        var response = await handler.HandleAsync(new GetLabelByIdQuery(label.Id, userId), CancellationToken.None);

        // assert
        Assert.Equal("Rough Trade", response.Name);
        Assert.Equal("Vereinigtes Königreich", response.CountryName);
    }

    [Fact]
    public async Task HandleAsync_UnbekanntesLabel_WirftNotFoundException()
    {
        // arrange
        var repository = Substitute.For<IRepository<LabelEntity>>();

        repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((LabelEntity?)null);

        var countryRepository = Substitute.For<IRepository<CountryEntity>>();

        var handler = new GetLabelByIdQueryHandler(
            repository, countryRepository, new ExceptionManager(), new LabelResponseBuilder());

        // act
        var act = () => handler.HandleAsync(new GetLabelByIdQuery(1, Guid.NewGuid()), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_FremdesLabel_WirftNotFoundExceptionStattForbidden()
    {
        // arrange
        var fremdesLabel = LabelEntity.Create("Rough Trade", 1, null, Guid.NewGuid());

        var repository = Substitute.For<IRepository<LabelEntity>>();

        repository.GetByIdAsync(fremdesLabel.Id, Arg.Any<CancellationToken>()).Returns(fremdesLabel);

        var countryRepository = Substitute.For<IRepository<CountryEntity>>();

        var handler = new GetLabelByIdQueryHandler(
            repository, countryRepository, new ExceptionManager(), new LabelResponseBuilder());

        var query = new GetLabelByIdQuery(fremdesLabel.Id, Guid.NewGuid());

        // act
        var act = () => handler.HandleAsync(query, CancellationToken.None);

        // assert: 404 statt 403 - Existenz einer fremden Ressource wird nicht bestätigt
        await Assert.ThrowsAsync<NotFoundException>(act);
    }
}
