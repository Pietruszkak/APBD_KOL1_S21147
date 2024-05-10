using APBD_KOL1_s21147.DTOs;
using APBD_KOL1_s21147.Services;
using APBD_KOL1_s21147.Validators;
using FluentValidation;

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
        ? Results.NotFound($"book with id:{id} doesn't exits")
        : Results.Ok(result);
});
app.MapPost("api/books", async (AddBooksDTO book, IDbService db, IValidator<AddBooksDTO> validator) =>
{
    var validation = validator.Validate(book);
    if (!validation.IsValid) 
        return Results.ValidationProblem(validation.ToDictionary());
    foreach (var genre in book.Genres)
    {
        if (!await db.DoesGenreExist(genre))
            return Results.NotFound($"Genre with id - {genre} doesn't exist");
    }
    await db.AddBooksWithGenres(book);
    return Results.Created("", null);
});
app.Run();
