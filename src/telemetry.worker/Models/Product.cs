namespace telemetry.worker.Models;

public class Product
{
    public Product(Guid id, string name, DateTimeOffset createdDate)
    {
        this.Id = id;
        this.Name = name;
        this.CreatedDate = createdDate;
    }

    public Guid Id { get; }

    public string Name { get; }

    public DateTimeOffset CreatedDate { get; }
}