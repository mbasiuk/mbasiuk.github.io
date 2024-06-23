using Microsoft.Data.Sqlite;

public class CompanySearch
{
    protected const int commandTimeout = 2;
    protected const string connectionString = "Data Source=db1.db";

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset Created { get; set; }

    public int Save()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"INSERT INTO CompanySearch(Name, Description) VALUES(@name, @desc)";
        cmd.Parameters.AddWithValue("desc", Description);
        cmd.Parameters.AddWithValue("name", Name);
        cmd.CommandTimeout = commandTimeout;
        return cmd.ExecuteNonQuery();
    }

    public static List<CompanySearch> Recent()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        var recentCmd = connection.CreateCommand();
        recentCmd.CommandText = @"SELECT Id, Name, Description, Created FROM CompanySearch ORDER BY Id DESC NULLS LAST LIMIT 3";
        recentCmd.CommandTimeout = commandTimeout;
        using var reader = recentCmd.ExecuteReader();
        var result = new List<CompanySearch>(3);
        while (reader.Read())
        {
            var search = new CompanySearch();
            result.Add(search);
            search.Id = reader.GetInt32(0);
            search.Name = reader.GetString(1);
            search.Description = reader.GetString(2);
            search.Created = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt32(3));
        }
        return result;
    }

    public static CompanySearch? Find(int Id)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"SELECT Id, Name, Description FROM CompanySearch WHERE Id = @Id;";
        cmd.Parameters.Add(new SqliteParameter("Id", Id));
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var result = new CompanySearch
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Created = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt32(3))
            };
            return result;
        }
        return null;
    }
}