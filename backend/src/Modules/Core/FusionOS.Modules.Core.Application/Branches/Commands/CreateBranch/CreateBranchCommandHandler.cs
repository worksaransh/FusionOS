using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Core.Application.Branches.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Domain.Organizations;
using MediatR;

namespace FusionOS.Modules.Core.Application.Branches.Commands.CreateBranch;

public sealed class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, BranchDto>
{
    private readonly IBranchRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBranchCommandHandler(IBranchRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BranchDto> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.CodeExistsAsync(request.CompanyId, request.Code, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Code), $"Branch code '{request.Code}' already exists for this company."),
            });
        }

        var branch = Branch.Create(request.CompanyId, request.Name, request.Code, request.IsHeadOffice);

        await _repository.AddAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(branch);
    }

    internal static BranchDto MapToDto(Branch branch) => new(
        branch.Id, branch.CompanyId, branch.Name, branch.Code, branch.IsHeadOffice, branch.IsActive, branch.CreatedAt);
}
