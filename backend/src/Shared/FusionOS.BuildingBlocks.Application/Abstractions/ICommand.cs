using MediatR;

namespace FusionOS.BuildingBlocks.Application.Abstractions;

public interface ICommand : IRequest<Unit> { }

public interface ICommand<TResponse> : IRequest<TResponse> { }
