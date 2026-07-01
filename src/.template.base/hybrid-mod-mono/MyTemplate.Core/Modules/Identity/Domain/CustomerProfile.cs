namespace MyTemplate.Core.Modules.Identity.Domain;

public sealed record CustomerProfile(
    Guid Id,
    string DisplayName,
    CustomerStatus Status)
{
    public bool CanPlaceOrders => Status == CustomerStatus.Active;
}
