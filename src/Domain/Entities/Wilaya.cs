using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Wilaya
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        // Other properties...

        // Navigation properties for one-to-many relationships
        public ICollection<Commune> Communes { get; private set; } = new List<Commune>();
        public ICollection<BloodTransferCenter> BloodTransferCenters { get; private set; } = new List<BloodTransferCenter>();
        
        private Wilaya() { } // For EF Core
        
        public Wilaya(string name)
        {
            Id = Guid.NewGuid();    
            Name = name;
        }
    }
}