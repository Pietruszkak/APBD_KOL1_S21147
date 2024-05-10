using APBD_KOL1_s21147.DTOs;
using APBD_KOL1_s21147.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDbService, DbService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapGet("api/books/{id:int}/genres", async (int id, IDbService db) =>
{
    var result = await db.GetGenresByBookId(id);
    return result is null
        ? Results.NotFound($"book with id:{id} does not exits")
        : Results.Ok(result);
});
app.MapPost("api/books", async (AddBooksDTO book, IDbService db) =>
{
    foreach (var genre in book.Genres)
    {
        if (!await db.DoesGenreExist(genre))
            return Results.NotFound($"Genre with id - {genre} doesn't exist");
    }
    var id=await db.AddBooksWithGenres(book);
    return Results.Created("", db.GetGenresByBookId(id));
});
app.Run();
