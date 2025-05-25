namespace Domain.Entities
{
    public class Commune
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public Guid WilayaId { get; private set; } // Foreign key
        
        // Navigation property
        public Wilaya Wilaya { get; private set; } = null!;
        
        private Commune() { } // For EF Core
        
        public Commune(string name, Guid wilayaId)
        {
            Id = Guid.NewGuid();
            Name = name;
            WilayaId = wilayaId;
        }
    }
}