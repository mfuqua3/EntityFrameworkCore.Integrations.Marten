# Why, oh why, did you make this?

Look, I get it. Your first reaction might be something along the lines of, "Combining Marten and Entity Framework Core? Are you some kind of masochist or just plain bored?" But bear with me a moment, and I promise it'll start to make some semblance of sense. Maybe.

Joking aside, I didn't wake up one morning, brush my teeth, look at myself in the mirror and think, "You know what the world needs? A Frankenstein's monster of database access layers." No, this project was born out of very real and practical considerations.

You see, I was working on a codebase that was in the awkward position of having to support both Marten and Entity Framework Core for interacting with its database. It was... well, let's just say it was "suboptimal". Developers had to know the type of table an entity represents ahead of time so they could decide whether to inject `IDocumentSession` or `MyAppDbContext`. It was like being bilingual in two completely unrelated languages, and constantly having to switch between them in the middle of conversations.

In addition, the two tools had different mechanisms for applying migrations. Imagine being a translator who not only had to switch languages on the fly, but also had to follow different rulebooks for how to do the translating. It's not just confusing – it's a potential source of errors and inefficiencies.

That's why I decided to create this integration. This project aims to simplify the developer's life by creating a unified API for accessing both relational and document-based data. By leveraging Entity Framework Core's DbContext and introducing a new property similar to `DbSet<T>`, this integration lets you declare Marten schema objects in the same place as relational tables.

Now, instead of having to remember which tool to use for which entity type, developers can simply use the same DbContext for all their database needs. Additionally, the integration also includes migration-generation behavior that detects schema changes from the new `DocumentSet<T>` object and leverages Marten to generate the migration and correctly update the EF model snapshot. This way, we can maintain a consistent approach to applying database migrations, regardless of whether we're dealing with relational tables or document-based data.

Is it complex? Absolutely. Ambitious? You bet. But I believe that the benefits of having a unified, consistent API for database access, as well as a streamlined approach to handling database migrations, outweigh the complexities of the initial implementation. I hope you'll agree.

And hey, if you don't, that's okay too. After all, not everyone needs to speak multiple languages – and not every codebase needs to juggle multiple database access tools. But for those that do, I hope this integration will make life just a little bit easier. And isn't that what all good software is supposed to do?

## The Case for Integration

So, you've read the backstory, but you're still wondering, "Why not just stick to one database access tool? Isn't integration just adding another layer of complexity?" Here's why I believe the effort is worth it:

1. **Unified API**: Having a unified API simplifies the codebase and improves developer experience. With the `DbDocument<T>` addition, there's no need to remember which tool to use for each entity type. It's all managed through the same `DbContext`. Say goodbye to unnecessary context switching (pun intended).

2. **Streamlined Migrations**: The integration also helps streamline database migrations. Instead of having to work with different mechanisms for applying migrations, we now have a consistent approach that handles schema changes from both `DbSet<T>` and `DocumentSet<T>`. Marten's `IDocumentStore` is used to generate the SQL for the migrations. The end result? Easier-to-manage migrations and fewer headaches.

3. **More Flexibility**: By supporting both Marten and Entity Framework Core, we cater to a wider range of use cases. Whether your data is better suited to a relational model or a document-based model, you're covered. And you get to use the same consistent API for both. Win-win!

## Risks and Acknowledgements

Of course, I'd be remiss if I didn't acknowledge the potential drawbacks and risks associated with this approach. Ambition often comes with a price tag, after all.

1. **Complexity**: Despite the aim to simplify the developer experience, the integration itself adds a level of complexity. Developers will need to understand how Marten and Entity Framework Core are working together, and there's a learning curve associated with that.

2. **Maintenance Overhead**: The integration needs to keep up with updates to both Marten and Entity Framework Core. This means more potential maintenance work down the line.

3. **Risk of Misuse**: With more flexibility comes the potential for misuse. While the unified API can handle both relational and document-based data, it's still important to choose the right tool for the job based on the nature of the data and the specific use case.

4. **Performance Considerations**: Performance may vary between Marten and Entity Framework Core, and it's something that needs to be carefully monitored. There's also a risk of running into issues if Entity Framework Core's change tracking gets mixed up with Marten's unit of work.

But in spite of these challenges, I believe the benefits outweigh the potential risks. With careful use and an understanding of the underlying mechanisms, this integration can significantly improve the developer experience and bring greater flexibility to your codebase. After all, isn't the goal of any tool to make our lives easier?
