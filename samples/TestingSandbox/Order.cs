namespace TestingSandbox;

public class Order
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
}