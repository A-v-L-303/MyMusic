namespace MyMusic.Application.Features.Stammdaten.Label.Commands.Update;

public sealed class UpdateLabelCommandHandler(
    IRepository<LabelEntity> repository,
    IRepository<CountryEntity> countryRepository,
    ExceptionManager exceptionManager,
    LabelResponseBuilder responseBuilder)
    : ICommandHandler<UpdateLabelCommand, LabelResponse>
{
    public async Task<LabelResponse> HandleAsync(UpdateLabelCommand command, CancellationToken cancellationToken)
    {
        var existingLabel = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (existingLabel is null || existingLabel.UserId != command.UserId)
            throw exceptionManager.NotFound("Label", command.Id);

        var (_, conflictingCount) = await repository.GetPagedAsync(
            label => label.UserId == command.UserId && label.Name == command.Name && label.Id != command.Id,
            query => query.OrderBy(label => label.Id),
            page: 1,
            pageSize: 1,
            cancellationToken);

        if (conflictingCount > 0)
            throw exceptionManager.Conflict($"Ein Label mit dem Namen '{command.Name}' existiert bereits.");

        var updatedLabel = existingLabel.Update(command.Name, command.CountryId, command.Information);

        repository.Update(updatedLabel);

        await repository.SaveChangesAsync(cancellationToken);

        var country = await countryRepository.GetByIdAsync(updatedLabel.CountryId, cancellationToken);

        return responseBuilder.Build(updatedLabel, country!.Name);
    }
}
