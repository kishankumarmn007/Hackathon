using System;

public class Reservation
{
    public string Id { get; set; } = "";
    public string Sku { get; set; } = "";
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
}