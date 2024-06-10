using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
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

    public const string TimeIntervalPattern = @"^(?<num>\d+) (?<t>days|weeks|hours|minutes|months|years|all)$";

    protected static bool ParseInterval(out int startTimestamp, out int endTimestamp, string? interval)
    {
        var regex = new Regex(TimeIntervalPattern);
        int num = 0;
        string t = "days";
        if (interval is not null && regex.IsMatch(interval))
        {
            var matches = regex.Matches(interval);
            foreach (Match match in matches)
            {
                int.TryParse(match.Groups["num"].Value, out num);
                t = match.Groups["t"].Value;
            }
        }
        else
        {
            startTimestamp = 0;
            endTimestamp = 0;
            return false;
        }
        var start = DateTimeOffset.UtcNow;
        start = t switch
        {
            "minutes" => start.AddMinutes(-1 * num),
            "hours" => start.AddHours(-1 * num),
            "days" => start.AddDays(-1 * num),
            "weeks" => start.AddDays(-7 * num),
            "months" => start.AddMonths(-1 * num),
            "years" => start.AddYears(-1 * num),
            "all" => start.AddYears(-10),
            _ => start.AddDays(-1),
        };
        startTimestamp = (int)start.ToUnixTimeSeconds();
        endTimestamp = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return true;
    }

    public static List<VisitDetails> GetByPage(int pageId, int? startDate, int? endDate, string? dateInterval)
    {
        if (!ParseInterval(out int start, out int end, dateInterval))
        {
            start = startDate ?? (int)DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds();
            end = endDate ?? (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        var sql =
        @"select v.id,
            v.page,
            v.date,
            s.created,
            s.user_agent, 
            s.accept_lang,
            s.referer,
            s.origin, 
            s.platform,
            s.ua,
            s.mobile,
            v.ip
        from visit v
        inner join visit v1 on v1.page = v.page 
        inner join session s on s.session_id = v.session_id
        where v1.id=@id 
            and v.date > @startDate 
            and v.date < @endDate
            and s.ignore is null";
        var Connection = new SqliteConnection(ConnectionString);
        Connection.Open();
        var cmd = Connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("id", pageId);
        cmd.Parameters.AddWithValue("startDate", start);
        cmd.Parameters.AddWithValue("endDate", end);
        var result = new List<VisitDetails>();
        var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var details = new VisitDetails(
                Id: reader.GetInt32(0),
                Page: reader.GetString(1),
                Timestamp: reader.GetInt32(3),
                SessionTimestamp: reader.IsDBNull(4) ? null : reader.GetInt32(4),
                UserAgent: reader.IsDBNull(5) ? null! : reader.GetString(5),
                AcceptedLang: reader.IsDBNull(6) ? null! : reader.GetString(6),
                Referer: reader.IsDBNull(7) ? null! : reader.GetString(7),
                Origin: reader.IsDBNull(8) ? null! : reader.GetString(8),
                Platform: reader.IsDBNull(9) ? null! : reader.GetString(9),
                UA: reader.IsDBNull(10) ? null! : reader.GetString(10),
                Mobile: reader.IsDBNull(11) ? null! : reader.GetString(11),
                Ip: reader.IsDBNull(12) ? null! : reader.GetString(12)
            );
            result.Add(details);
        }
        return result;
    }
}

public record VisitSummary(int Id, string Page, int Total, int Unique, int N);
public record Visits(int[] Values);

public class VisitCriteria
{
    public int PageId { get; set; }
    public int? Start { get; set; }
    public int? End { get; set; }
    [RegularExpression(Visit.TimeIntervalPattern)]
    public string? Interval { get; set; }
}

public record VisitDetails(int Id, string Page, int Timestamp, int? SessionTimestamp, string UserAgent, string AcceptedLang, string Referer, string Origin, string Platform, string UA, string Mobile, string Ip);