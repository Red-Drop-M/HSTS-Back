namespace Domain.ValueObjects
{
    public class ServiceFilter
    {
        public string? Name { get; set; }
        public DateOnly? CreatedAt { get; set; }
        public DateOnly? UpdatedAt { get; set; }
    }
}