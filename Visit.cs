using Microsoft.Data.Sqlite;

public class Visit : LiteEntity
{
    private readonly DateTimeOffset Date;
    private readonly string Page;
    private readonly string SessionId;
    private readonly string? Auth;
    protected string ConnectionId;
    protected string? Ip;

    public Visit(HttpContext context)
    {
        Date = DateTimeOffset.UtcNow;
        Page = context.Request.Path;
        SessionId = (string)context.Items[nameof(SessionId)]!;
        Auth = context.Request.Cookies["auth"]?.ToString() ?? "unset";
        ConnectionId = context.Connection.Id ?? "unset";
        Ip = context.Connection.RemoteIpAddress?.ToString() ?? "unset";
    }

    public void Track()
    {
        var Connection = new SqliteConnection(ConnectionString);
        Connection.Open();
        var cmd = Connection.CreateCommand();
        cmd.CommandTimeout = CommandTimeout;
        cmd.CommandText = "INSERT INTO Visit(page, session_id, auth, connection_id, ip) VALUES (@page, @session_id, @auth, @connection_id, @ip)";
        cmd.Parameters.Add(new SqliteParameter("page", Page));
        cmd.Parameters.Add(new SqliteParameter("session_id", SessionId));
        cmd.Parameters.Add(new SqliteParameter("auth", Auth));
        cmd.Parameters.Add(new SqliteParameter("connection_id", ConnectionId));
        cmd.Parameters.Add(new SqliteParameter("ip", Ip));
        cmd.ExecuteNonQuery();
    }

    public static List<VisitSummary> GetSummary()
    {
        var sql = @"
        SELECT p.page, COUNT(1) AS total, COUNT(DISTINCT(p.session_id)) AS [unique], COUNT(DISTINCT(s.created)) as [n]
        FROM Visit p
        LEFT JOIN Session s ON p.session_id = s.session_id AND s.created >= @new_date
        GROUP BY P.page";
        var Connection = new SqliteConnection(ConnectionString);
        Connection.Open();
        var cmd = Connection.CreateCommand();
        cmd.CommandTimeout = CommandTimeout;
        cmd.CommandText = sql;
        cmd.Parameters.Add(new SqliteParameter("new_date", DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds()));
        var reader = cmd.ExecuteReader();
        var result = new List<VisitSummary>();
        while (reader.Read())
        {
            result.Add(new VisitSummary(reader.GetString(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3)));
        }
        return result;
    }
}

public record VisitSummary(string Page, int Total, int Unique, int N);