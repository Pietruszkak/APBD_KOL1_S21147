using System.Data.SqlClient;
using System.Data;
using APBD_KOL1_s21147.DTOs;

namespace APBD_KOL1_s21147.Services;

public interface IDbService
{
    Task<GetBookWithGenresDTO?> GetGenresByBookId(int id);
    Task<int> AddBooksWithGenres(AddBooksDTO book);
    Task<bool> DoesGenreExist(int id);
}

public class DbService : IDbService
{
    private IConfiguration _configuration;

    public DbService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private async Task<SqlConnection> GetConnection()
    {
        var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        return connection;
    }
    public async Task<bool> DoesGenreExist(int id)
    {
        var query = "SELECT 1 FROM genre WHERE pk = @id";

        await using var connection = await GetConnection();
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<GetBookWithGenresDTO?> GetGenresByBookId(int id)
    {
        await using var connection = await GetConnection();
        var command = new SqlCommand(
            @"SELECT g.pk, g.nam, b.title
                              FROM genres g left join books_genres bg
                              on g.pk = bg.fk_genre
                              left join books b
                              on b.pk = bg.fk_book
                              WHERE g.pk = @id",
            connection
        );
        command.Parameters.AddWithValue("@id", id);
        var reader = await command.ExecuteReaderAsync();
        if (!reader.HasRows)
        {
            return null;
        }

        var result = new GetBookWithGenresDTO(
            reader.GetInt32(0),
            reader.GetString(1),
            new List<string>()
        );
        while (await reader.ReadAsync())
        {
            result.Genres.Add(reader.GetString(2));
        }

        return result;
    }
    
    public async Task<int> AddBooksWithGenres(AddBooksDTO book)
    {
        await using var connection = await GetConnection();
        var command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = """
                                   INSERT INTO books VALUES(@title)
                                   """;
        command.Parameters.AddWithValue("@title", book.Title);
        
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        try
        {
            var id = await command.ExecuteScalarAsync();
            foreach (var genre in book.Genres)
            {
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO books_genres VALUES(@fk_book, @fk_genre)";
                command.Parameters.AddWithValue("@fk_book", id);
                command.Parameters.AddWithValue("@fk_genre", genre);

                await command.ExecuteNonQueryAsync();
            }
            return (int)id;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
        
    }

}