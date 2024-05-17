using Microsoft.Data.Sqlite;

public class CompanySearch
{
    protected const int commandTimeout = 2;
    protected const string connectionString = "Data Source=db1.db";

    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset Created { get; set; }

    public int Save()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"INSERT INTO CompanySearch(Description) VALUES(@desc)";
        var desc = new SqliteParameter("desc", Description);
        cmd.Parameters.Add(desc);
        var created = new SqliteParameter("@created", Created.ToUnixTimeSeconds());
        cmd.CommandTimeout = commandTimeout;
        cmd.Parameters.Add(created);
        return cmd.ExecuteNonQuery();
    }

    public static List<CompanySearch> Recent()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        var recentCmd = connection.CreateCommand();
        recentCmd.CommandText = @"SELECT Id, Description, Created FROM CompanySearch ORDER BY Id DESC NULLS LAST LIMIT 3";
        recentCmd.CommandTimeout = commandTimeout;
        using var reader = recentCmd.ExecuteReader();
        var result = new List<CompanySearch>(3);
        while (reader.Read())
        {
            var search = new CompanySearch();
            result.Add(search);
            search.Id = reader.GetInt32(0);
            search.Description = reader.GetString(1);
            search.Created = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt32(2));
        }
        return result;
    }

    public static CompanySearch? Find(int Id)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"SELECT Id, Description FROM CompanySearch WHERE Id = @Id;";
        cmd.Parameters.Add(new SqliteParameter("Id", Id));
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var result = new CompanySearch
            {
                Id = reader.GetInt32(0),
                Description = reader.GetString(1),
                Created = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt32(2))
            };
            return result;
        }
        return null;
    }
}