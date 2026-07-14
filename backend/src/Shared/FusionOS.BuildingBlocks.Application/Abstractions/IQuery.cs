using MediatR;

namespace FusionOS.BuildingBlocks.Application.Abstractions;

public interface IQuery<TResponse> : IRequest<TResponse> { }
