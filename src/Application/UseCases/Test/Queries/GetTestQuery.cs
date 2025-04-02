using MediatR;
public record GetTestQuery(int Id):IRequest<string>;