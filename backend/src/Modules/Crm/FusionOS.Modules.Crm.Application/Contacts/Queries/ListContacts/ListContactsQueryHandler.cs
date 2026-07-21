using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Contacts.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Contacts.Queries.ListContacts;

public sealed class ListContactsQueryHandler : IRequestHandler<ListContactsQuery, PagedResult<ContactDto>>
{
    private readonly IContactRepository _repository;

    public ListContactsQueryHandler(IContactRepository repository) => _repository = repository;

    public async Task<PagedResult<ContactDto>> Handle(ListContactsQuery request, CancellationToken cancellationToken)
    {
        var contacts = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = contacts.Select(ContactMapper.ToDto).ToList();

        return new PagedResult<ContactDto>(dtos, request.Page, request.PageSize, total);
    }
}
