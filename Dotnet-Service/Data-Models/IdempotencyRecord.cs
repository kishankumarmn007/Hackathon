public class IdempotencyRecord
{
    public string Key { get; set; } = "";
    public ReservationStatus Status { get; set; }
    public string? ReservationId { get; set; }
}