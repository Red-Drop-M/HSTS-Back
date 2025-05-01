using Domain.Entities;
using Domain.ValueObjects;
namespace Domain.Entities
{
public class Service
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ICollection<Request> Requests { get; private set; } = new List<Request>();

    // Constructor without Id (the database will generate it)
    public Service(string name)
    {
        Name = name;
    }

    public void Update(string name)
    {
        Name = name;
    }

    // EF Core requires a parameterless constructor for querying
    protected Service() { } 
}

}