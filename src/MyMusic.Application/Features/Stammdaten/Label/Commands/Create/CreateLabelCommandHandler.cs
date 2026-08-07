namespace MyMusic.Application.Features.Stammdaten.Label.Commands.Create;

public sealed class CreateLabelCommandHandler(
    IRepository<LabelEntity> repository,
    IRepository<CountryEntity> countryRepository,
    ExceptionManager exceptionManager,
    LabelResponseBuilder responseBuilder)
    : ICommandHandler<CreateLabelCommand, LabelResponse>
{
    public async Task<LabelResponse> HandleAsync(CreateLabelCommand command, CancellationToken cancellationToken)
    {
        var (_, existingCount) = await repository.GetPagedAsync(
            label => label.UserId == command.UserId && label.Name == command.Name,
            query => query.OrderBy(label => label.Id),
            page: 1,
            pageSize: 1,
            cancellationToken);

        if (existingCount > 0)
            throw exceptionManager.Conflict($"Ein Label mit dem Namen '{command.Name}' existiert bereits.");

        var label = LabelEntity.Create(command.Name, command.CountryId, command.Information, command.UserId);

        await repository.AddAsync(label, cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        var country = await countryRepository.GetByIdAsync(label.CountryId, cancellationToken);

        return responseBuilder.Build(label, country!.Name);
    }
}
