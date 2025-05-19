using Domain.ValueObjects;

namespace Application.DTOs
{
    public class GlobalStockDTO
    {
        public string BloodType { get; set; } = string.Empty;
        public string BloodBagType { get; set; } = string.Empty;
        public int CountExpired { get; set; }
        public int CountExpiring { get; set; }
        public int ReadyCount { get; set; }
        public int MinStock { get; set; }
        public int CriticalStock { get; set; }
    }
}