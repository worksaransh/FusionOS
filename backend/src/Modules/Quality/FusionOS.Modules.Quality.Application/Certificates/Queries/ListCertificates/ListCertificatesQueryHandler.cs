using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Certificates.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Certificates.Queries.ListCertificates;

public sealed class ListCertificatesQueryHandler : IRequestHandler<ListCertificatesQuery, PagedResult<QualityCertificateSummaryDto>>
{
    private readonly IQualityCertificateRepository _repository;

    public ListCertificatesQueryHandler(IQualityCertificateRepository repository) => _repository = repository;

    public async Task<PagedResult<QualityCertificateSummaryDto>> Handle(ListCertificatesQuery request, CancellationToken cancellationToken)
    {
        var certificates = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = certificates.Select(QualityCertificateSummaryMapper.ToSummaryDto).ToList();

        return new PagedResult<QualityCertificateSummaryDto>(dtos, request.Page, request.PageSize, total);
    }
}
