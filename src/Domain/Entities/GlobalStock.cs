using Domain.ValueObjects;

namespace Domain.Entities
{
    public class GlobalStock
    {
        // Composite Primary Key: BloodType + BloodBagType
        public BloodType BloodType { get; private set; } = default!;
        public BloodBagType BloodBagType { get; private set; } = default!;

        // Stock tracking properties
        public int CountExpired { get; private set; } = 0;
        public int CountExpiring { get; private set; } = 0;
        public int ReadyCount { get; private set; } = 0;
        public int MinStock { get; private set; } = 0;
        public int CriticalStock { get; private set; } = 0;

        // EF Core requires a parameterless constructor
        private GlobalStock() { }

        public GlobalStock(
            BloodType bloodType,
            BloodBagType bloodBagType,
            int countExpired,
            int countExpiring,
            int readyCount,
            int minStock,
            int criticalStock)
        {
            BloodType = bloodType;
            BloodBagType = bloodBagType;
            CountExpired = countExpired;
            CountExpiring = countExpiring;
            ReadyCount = readyCount;
            MinStock = minStock;
            CriticalStock = criticalStock;
        }

        // Optional: methods to update stock counts
        public void UpdateCounts(int expired, int expiring, int ready)
        {
            CountExpired = expired;
            CountExpiring = expiring;
            ReadyCount = ready;
        }

        public void UpdateThresholds(int min, int critical)
        {
            MinStock = min;
            CriticalStock = critical;
        }
    }
}
