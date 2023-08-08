# EntityFrameworkCore.Integrations.Marten

This library is designed to seamlessly integrate [Marten](https://martendb.io/), a .NET transactional document DB and event store on PostgreSQL, with [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/), the popular .NET ORM. The goal is to reduce the cognitive load for developers who work with both relational and document databases, providing a unified approach to data access.

## Ongoing Development

This project is a work-in-progress. Star the repository to follow development. 

## Design Goals

1. Unify EF Core and Marten interactions under a single DbContext.
2. Leverage EF Core migrations to handle Marten document schema changes.
3. Streamline data access to both relational and document-based data via EF Core's familiar APIs.
4. Improve developer productivity by removing the need to decide which data access technology to use.

## Intended Usage

Assuming we have an entity `Person` that should be a Marten-backed document, you can interact with it as follows:

```csharp
public class Person
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
```

```csharp
public class MyDbContext : MartenIntegratedDbContext
{
    public DbDocument<Person> People { get; set; }
}
```

The DocumentSet<T> is a new property, similar to DbSet<T>, which implicitly declares Marten-backed document collections rather than relational tables.
```csharp
// Adding a new document
using (var context = new MyDbContext())
{
    var person = new Person { Name = "John Doe" };
    context.People.Add(person);
    await context.SaveChangesAsync();
}
```
```csharp
// Querying documents
using (var context = new MyDbContext())
{
    var person = await context.People.Where(p => p.Name == "John Doe").FirstOrDefaultAsync();
}
```
Migrations are leveraged to detect changes to the Marten documents schemas. When adding or removing a DocumentSet<T> from the context, a migration is generated via EF Core's mechanisms. The migration then delegates to Marten's schema handling capabilities to correctly apply the changes and keep the EF Core model snapshot updated.

# To add a migration
```bash
dotnet ef migrations add AddPersonDocument
```
# To update the database
```bash
dotnet ef database update
```
This README assumes certain functionality that matches your library's intended design. Make sure to update the code examples to reflect the actual API as your implementation progresses.
