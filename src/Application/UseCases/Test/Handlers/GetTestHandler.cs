using MediatR;

public class GetTestHandler : IRequestHandler<GetTestQuery, string>
{
    public async Task<string> Handle(GetTestQuery request, CancellationToken cancellationToken)
    {
        // Validate the ID
        if (request.Id <= 0)
        {
            throw new ArgumentException("Invalid ID. ID must be greater than 0.");
        }

        // Simulate fetching data (replace with actual logic)
        return await Task.FromResult($"Test result for ID {request.Id}");
    }
}
