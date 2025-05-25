using Domain.Events;
namespace Domain.Events
{
    public sealed record CtsData(
        Guid HospitalId,
        string HospitalName,
        string HospitalAddress,
        string HospitalPhoneNumber,
        string HospitalEmail,
        string Wilaya,
        string Commune,
        List<GlobalStockData> GlobalStockData);
}