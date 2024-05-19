using System.Globalization;
using Microsoft.Data.Sqlite;

public class Session : LiteEntity
{
    protected string Key = "t";
    protected string SessionId { get; set; } = null!;
    protected DateTimeOffset? Created { get; set; }
    protected DateTimeOffset? Expires { get; set; }
    protected long Id { get; set; }
    protected string? ConnectionId { get; set; }
    protected string? AcceptedLang { get; set; }
    protected string? LocalIp { get; set; }
    protected int? LocalPort { get; set; }
    protected string? Ip { get; set; }
    protected int? Port { get; set; }
    protected string? UserAgent { get; set; }

    public Session(HttpContext context)
    {
        SessionId = context.Request.Cookies[Key]!;
        if (SessionId == null)
        {
            UpdateFromContext(context);
            SessionId = DateTimeOffset.UtcNow.ToString("yyMMdd") + Guid.NewGuid().ToString().Substring(6);
            using var Connection = new SqliteConnection(ConnectionString);
            Connection.Open();
            using var cmd = Connection.CreateCommand();
            cmd.CommandTimeout = CommandTimeout;
            cmd.CommandText = "INSERT INTO Session (session_id, connection_id, accept_lang, local_ip, local_port, ip, port, user_agent) VALUES(@session_id, @connection_id, @accept_lang, @local_ip, @local_port, @ip, @port, @user_agent)";
            cmd.Parameters.Add(new SqliteParameter("session_id", SessionId));
            CreateContextParams(cmd);
            var result = cmd.ExecuteNonQuery();
            var select = Connection.CreateCommand();
            select.CommandTimeout = CommandTimeout;
            select.CommandText = "SELECT expires FROM Session WHERE session_id = @session_id";
            select.Parameters.Add(new SqliteParameter("session_id", SessionId));
            using var reader = select.ExecuteReader();
            reader.Read();
            Expires = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt32(0));
            context.Response.Cookies.Append(Key, SessionId, new CookieOptions(){ Expires = Expires, Secure = true, HttpOnly = true, IsEssential = true });
            if (result == 0) { throw new Exception("unable to save"); }
        }
        context.Items[nameof(SessionId)] = SessionId;
    }

    private void CreateContextParams(SqliteCommand cmd)
    {
        cmd.Parameters.Add(new SqliteParameter("connection_id", ConnectionId));
        cmd.Parameters.Add(new SqliteParameter("accept_lang", AcceptedLang));
        cmd.Parameters.Add(new SqliteParameter("local_ip", LocalIp));
        cmd.Parameters.Add(new SqliteParameter("local_port", LocalPort));
        cmd.Parameters.Add(new SqliteParameter("ip", Ip));
        cmd.Parameters.Add(new SqliteParameter("port", Port));
        cmd.Parameters.Add(new SqliteParameter("user_agent", UserAgent));
    }

    private void UpdateFromContext(HttpContext context)
    {
        ConnectionId = context.Connection.Id;
        AcceptedLang = context.Request.Headers.AcceptLanguage;
        LocalIp = context.Connection.LocalIpAddress?.ToString();
        LocalPort = context.Connection.LocalPort;
        Ip = context.Connection.RemoteIpAddress?.ToString();
        Port = context.Connection.RemotePort;
        UserAgent = context.Request.Headers.UserAgent;
    }
}