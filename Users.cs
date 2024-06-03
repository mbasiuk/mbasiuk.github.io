using Microsoft.Data.Sqlite;

public class User : LiteEntity
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Status { get; set; } = null!;

    public static User? FindByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }
        using var connection = new SqliteConnection(ConnectionString);
        using var command = connection.CreateCommand();
        command.CommandTimeout = CommandTimeout;
        command.CommandText = "SELECT full_name, email, status FROM user WHERE email = @email";
        command.Parameters.AddWithValue("email", email);
        connection.Open();
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }
        User user = new User();
        if (!reader.IsDBNull(0))
        {
            user.FullName = reader.GetString(0);
        }
        user.Email = reader.GetString(1);
        if (!reader.IsDBNull(2))
        {
            user.Status = reader.GetString(2);
        }
        return user;
    }
}