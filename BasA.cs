using Microsoft.Data.Sqlite;

public class BasA : LiteEntity
{
    public Guid? Id { get; set; }
    public Guid? ReadId { get; set; }
    public DateTimeOffset? Created { get; set; }
    public DateTimeOffset? Demanded { get; set; }
    public DateTimeOffset? Expire { get; set; }

    public static BasA Create()
    {
        var a = new BasA()
        {
            Id = Guid.NewGuid(),
            ReadId = Guid.NewGuid()
        };
        using var Connection = new SqliteConnection(ConnectionString);
        Connection.Open();
        using var cmd = Connection.CreateCommand();
        cmd.CommandTimeout = CommandTimeout;
        cmd.CommandText = "INSERT INTO basa (id, rid) VALUES(@id, @rid)";
        cmd.Parameters.Add(new SqliteParameter("id", a.Id));
        cmd.Parameters.Add(new SqliteParameter("rid", a.ReadId));
        var result = cmd.ExecuteNonQuery();
        using var selectCmd = Connection.CreateCommand();
        selectCmd.CommandTimeout = CommandTimeout;
        selectCmd.CommandText = "SELECT created, demanded, expire from basa where id=@id";
        selectCmd.Parameters.AddWithValue("id", a.Id);
        var reader = selectCmd.ExecuteReader();
        if (!reader.Read())
        {
            return null!;
        }
        a.Created = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt32(0));
        if (!reader.IsDBNull(1))
        {
            a.Demanded = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt32(1));
        }
        a.Expire = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt32(2));
        return a;
    }

    public BasA FindByReadId(Guid readId)
    {
        using var Connection = new SqliteConnection(ConnectionString);
        using var selectCmd = Connection.CreateCommand();
        selectCmd.CommandTimeout = CommandTimeout;
        selectCmd.CommandText = "SELECT created, demanded, expire from basa where rid=@rid";
        selectCmd.Parameters.AddWithValue("rid", readId);
        var reader = selectCmd.ExecuteReader();
        if (!reader.Read())
        {
            return null!;
        }
        var a = new BasA
        {
            ReadId = readId,
            Created = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt32(0)),
            Expire = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt32(2))
        };
        int? demanded = reader.GetInt32(1);
        if (demanded.HasValue)
        {
            a.Demanded = DateTimeOffset.FromUnixTimeSeconds(demanded.Value);
        }
        return a;
    }

    public BasA FindById(Guid id)
    {
        using var Connection = new SqliteConnection(ConnectionString);
        using var selectCmd = Connection.CreateCommand();
        selectCmd.CommandTimeout = CommandTimeout;
        selectCmd.CommandText = "SELECT rid, created, demanded, expire from basa where id=@id";
        selectCmd.Parameters.AddWithValue("id", id);
        var reader = selectCmd.ExecuteReader();
        if (!reader.Read())
        {
            return null!;
        }
        var a = new BasA
        {
            Id = id,
            ReadId = Guid.Parse(reader.GetString(0)),
            Created = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt32(1)),
            Expire = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt32(3))
        };
        int? demanded = reader.GetInt32(2);
        if (demanded.HasValue)
        {
            a.Demanded = DateTimeOffset.FromUnixTimeSeconds(demanded.Value);
        }
        return a;
    }
}
