using Microsoft.Data.Sqlite;

public class Session : LiteEntity
{
    protected string Key = "t";
    protected string SessionId { get; set; } = null!;
    protected DateTimeOffset? Created { get; set; }
    protected DateTimeOffset? Expires { get; set; }
    protected long Id { get; set; }
    protected string? AcceptedLang { get; set; }
    protected string? LocalIp { get; set; }
    protected int? LocalPort { get; set; }
    protected string? Ip { get; set; }
    protected int? Port { get; set; }
    protected string? UserAgent { get; set; }
    protected string? Origin { get; set; }
    protected string? Referer { get; set; }
    protected string? Platform { get; set; }
    protected string? UA { get; set; }
    protected string? Mobile { get; set; }


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
            cmd.CommandText = "INSERT INTO Session (session_id, accept_lang, local_ip, local_port, ip, port, user_agent, origin, referer, platform, ua, mobile) VALUES(@session_id, @accept_lang, @local_ip, @local_port, @ip, @port, @user_agent, @origin, @referer, @platform, @ua, @mobile)";
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
            context.Response.Cookies.Append(Key, SessionId, new CookieOptions() { Expires = Expires, Secure = true, HttpOnly = true, IsEssential = true });
            if (result == 0) { throw new Exception("unable to save"); }
        }
        context.Items[nameof(SessionId)] = SessionId;
    }

    public void Clear(HttpContext context)
    {
        context.Response.Cookies.Delete(Key);
    }

    private void CreateContextParams(SqliteCommand cmd)
    {
        cmd.Parameters.Add(new SqliteParameter("accept_lang", AcceptedLang));
        cmd.Parameters.Add(new SqliteParameter("local_ip", LocalIp));
        cmd.Parameters.Add(new SqliteParameter("local_port", LocalPort));
        cmd.Parameters.Add(new SqliteParameter("ip", Ip));
        cmd.Parameters.Add(new SqliteParameter("port", Port));
        cmd.Parameters.Add(new SqliteParameter("user_agent", UserAgent));
        
        cmd.Parameters.Add(new SqliteParameter("origin", Origin));
        cmd.Parameters.Add(new SqliteParameter("referer", Referer));
        cmd.Parameters.Add(new SqliteParameter("platform", Platform));
        cmd.Parameters.Add(new SqliteParameter("ua", UA));
        cmd.Parameters.Add(new SqliteParameter("mobile", Mobile));
    }

    private void UpdateFromContext(HttpContext context)
    {
        AcceptedLang = context.Request.Headers.AcceptLanguage;
        AcceptedLang ??= "-";
        LocalIp = context.Connection.LocalIpAddress?.ToString();
        LocalIp ??= "-";
        LocalPort = context.Connection.LocalPort;
        LocalPort ??= 0;
        Ip = context.Request.Headers["X-Forwarded-For"].ToString() ?? "-";
        Port = context.Connection.RemotePort;
        Port ??= 0;
        UserAgent = context.Request.Headers.UserAgent;
        UserAgent ??= "-";
        Origin = context.Request.Headers.Origin;
        Origin ??= "-";
        Referer = context.Request.Headers.Referer;
        Referer ??= "-";
        Platform = context.Request.Headers["sec-ch-ua-platform"];
        Platform ??= "-";
        UA = context.Request.Headers["sec-ch-ua"];
        UA ??= "-";
        Mobile = context.Request.Headers["sec-ch-ua-mobile"];
        Mobile ??= "-";
    }
}