using Domain.ValueObjects;

namespace Application.DTOs
{
    public class ServiceDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}