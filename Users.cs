using Microsoft.Data.Sqlite;

public class User : LiteEntity
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string Ip { get; set; } = null!;

    public int Port { get; set; }

    public string? Session { get; set; }

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

    public static void SignUp(SignUpRecord sign, HttpContext context)
    {
        if (!sign.IsValid())
        {
            return;
        }

        var newUser = new User();
        if (sign.IsEmail())
        {
            newUser.Email = sign.User;
        }
        else
        {
            newUser.Phone = sign.User;
        }

        newUser.Session = context.Request.Cookies["t"];
        newUser.Password = sign.Password;
        newUser.Ip = context.Connection.RemoteIpAddress!.ToString();
        newUser.Port = context.Connection.RemotePort;
        using var connection = new SqliteConnection(ConnectionString);
        using var command = connection.CreateCommand();
        command.CommandTimeout = CommandTimeout;
        command.CommandText = "INSERT INTO User(email, phone, password, ip, port, session) VALUES(@email, @phone, @password, @ip, @port, @session)";

        if (newUser.Email == null)
        {
            command.Parameters.AddWithValue("email", DBNull.Value);
        }
        else
        {
            command.Parameters.AddWithValue("email", newUser.Email);
        }
        if (newUser.Phone == null)
        {
            command.Parameters.AddWithValue("phone", DBNull.Value);
        }
        else
        {
            command.Parameters.AddWithValue("phone", newUser.Phone);
        }
        command.Parameters.AddWithValue("password", newUser.Password);


        if (newUser.Ip == null)
        {
            command.Parameters.AddWithValue("ip", DBNull.Value);
        }
        else
        {
            command.Parameters.AddWithValue("ip", newUser.Ip);
        }

        if (newUser.Port == 0)
        {
            command.Parameters.AddWithValue("port", DBNull.Value);
        }
        else
        {
            command.Parameters.AddWithValue("port", newUser.Port);
        }

        if (newUser.Session == null)
        {
            command.Parameters.AddWithValue("session", DBNull.Value);
        }
        else
        {
            command.Parameters.AddWithValue("session", newUser.Session);
        }
        connection.Open();
        var result = command.ExecuteNonQuery();
    }
}