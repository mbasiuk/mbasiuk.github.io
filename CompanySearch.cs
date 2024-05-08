using Microsoft.Data.Sqlite;

public class CompanySearch
{
    protected const int commandTimeout = 2;
    protected const string connectionString = "Data Source=db1.db";
    
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public int Save()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"INSERT INTO CompanySearch(Description) VALUES(@desc)";
        var desc = new SqliteParameter("desc", Description);
        cmd.Parameters.Add(desc);
        cmd.CommandTimeout = commandTimeout;
        return cmd.ExecuteNonQuery();
    }

    public static int Create()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        var createTableCommand = connection.CreateCommand();
        createTableCommand.CommandText = @"
            CREATE TABLE IF NOT EXISTS CompanySearch(Id INTEGER PRIMARY KEY,
            Description TEXT UNIQUE NOT NULL)
        ";
        int result = createTableCommand.ExecuteNonQuery();
        Console.WriteLine("Create CompanySearch Table: {0}", result);
        return result;
    }

    public static List<CompanySearch> Recent()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        var createTableCommand = connection.CreateCommand();
        createTableCommand.CommandText = @"
            SELECT Id, Description FROM CompanySearch ORDER BY Id DESC NULLS LAST LIMIT 3
        ";
        using var reader = createTableCommand.ExecuteReader();
        var result = new List<CompanySearch>(3);
        while(reader.Read())
        {
            var search = new CompanySearch();
            result.Add(search);
            search.Id = reader.GetInt32(0);
            search.Description = reader.GetString(1);
        }
        return result;
    }
}