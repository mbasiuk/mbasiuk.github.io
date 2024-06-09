using Microsoft.Data.Sqlite;

public class Visit : LiteEntity
{
    private readonly string Page;
    private readonly string SessionId;
    private readonly string? Auth;
    protected string? Ip;

    protected string? TransactionId;

    public Visit(HttpContext context)
    {
        Page = context.Request.Path;
        SessionId = (string)context.Items[nameof(SessionId)]!;
        Auth = context.Request.Cookies["auth"]?.ToString() ?? "-";
        Ip = context.Request.Headers["X-Forwarded-For"].ToString() ?? "-";
        TransactionId = context.Request.Headers["Tid"].ToString() ?? "-";
    }

    public void Track()
    {
        var Connection = new SqliteConnection(ConnectionString);
        Connection.Open();
        var cmd = Connection.CreateCommand();
        cmd.CommandTimeout = CommandTimeout;
        cmd.CommandText = "INSERT INTO Visit(page, session_id, auth, ip. transactionId) VALUES (@page, @session_id, @auth, @ip, @transactionId)";
        cmd.Parameters.Add(new SqliteParameter("page", Page));
        cmd.Parameters.Add(new SqliteParameter("session_id", SessionId));
        cmd.Parameters.Add(new SqliteParameter("auth", Auth));
        cmd.Parameters.Add(new SqliteParameter("ip", Ip));
        cmd.Parameters.Add(new SqliteParameter("transactionId", TransactionId));
        cmd.ExecuteNonQuery();
    }

    public static List<VisitSummary> GetSummary()
    {
        var sql = @"
        SELECT p.page, COUNT(1) AS total, COUNT(DISTINCT(p.session_id)) AS [unique], COUNT(DISTINCT(s.created)) as [n], p.id
        FROM Visit p
        LEFT JOIN Session s ON p.session_id = s.session_id AND s.created >= @new_date
        LEFT JOIN IgnoredVisit i ON i.page = p.page
        WHERE i.id is null
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
            result.Add(new VisitSummary(reader.GetInt32(4), reader.GetString(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3)));
        }
        return result;
    }

    public static bool IgnoreByPage(Visits visits)
    {
        if (visits == null || visits.Values == null || visits.Values.Length == 0)
        {
            return false;
        }
        var values = visits.Values;
        var Connection = new SqliteConnection(ConnectionString);
        Connection.Open();
        var cmd = Connection.CreateCommand();
        var paramNames = visits.Values.Select((i, j) => "@id" + j);
        var sql = $"INSERT INTO IgnoredVisit(page) SELECT page FROM visit v WHERE v.Id IN ({string.Join(", ", paramNames)})";
        cmd.CommandText = sql;
        cmd.CommandTimeout = CommandTimeout;
        for (int i = 0; i < values.Length; i++)
        {
            int v = values[i];
            cmd.Parameters.AddWithValue("id" + i, v);
        }
        return cmd.ExecuteNonQuery() > 0;
    }
}

public record VisitSummary(int Id, string Page, int Total, int Unique, int N);
public record Visits(int[] Values);