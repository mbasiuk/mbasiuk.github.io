using Microsoft.Data.Sqlite;
using SQLitePCL;
using System.Security.Cryptography;
using System.Text;

public class BasA : LiteEntity
{
    public Guid? Id { get; set; }
    public Guid? ReadId { get; set; }
    public DateTimeOffset? Created { get; set; }
    public DateTimeOffset? Demanded { get; set; }
    public int? DemandedTimestamp { get; set; }
    public DateTimeOffset? Expire { get; set; }

    public string? Duration { get; set; }
    public string? DurationCustom { get; set; }
    public Guid? LicenseKey { get; set; }
    public string? Message { get; set; }
    public string? Signature { get; set; }

    public static BasA Create(BasA b)
    {
        var a = new BasA
        {
            Id = Guid.NewGuid(),
            ReadId = Guid.NewGuid(),
            Expire = DateTimeOffset.UtcNow.AddMonths(1),
            LicenseKey = b.LicenseKey,
            Message = b.Message,
            DurationCustom = b.DurationCustom
        };

        if (a.Message != null)
        {
            a.Message = a.Message.Replace("<", "o[").Replace(">", "]o");
        }

        if (a.DurationCustom != null)
        {
            a.DurationCustom = a.DurationCustom.Replace("<", "o[").Replace(">", "]o");
        }

        if (b.Duration == "6months")
        {
            a.Expire = DateTimeOffset.UtcNow.AddMonths(6);
        }

        using var Connection = new SqliteConnection(ConnectionString);
        Connection.Open();
        using var cmd = Connection.CreateCommand();
        cmd.CommandTimeout = CommandTimeout;
        cmd.CommandText = "INSERT INTO basa (id, rid, expire, license, message, expireWanted) VALUES(@id, @rid, @expire, @license, @message, @expireWanted)";
        cmd.Parameters.Add(new SqliteParameter("id", a.Id));
        cmd.Parameters.Add(new SqliteParameter("rid", a.ReadId));
        cmd.Parameters.AddWithValue("expire", (int)a.Expire.Value.ToUnixTimeSeconds());
        if (a.LicenseKey.HasValue)
        {
            cmd.Parameters.AddWithValue("license", a.LicenseKey.Value);
        }
        else
        {
            cmd.Parameters.AddWithValue("license", DBNull.Value);
        }

        if (a.Message == null)
        {
            cmd.Parameters.AddWithValue("message", DBNull.Value);
        }
        else
        {
            cmd.Parameters.AddWithValue("message", a.Message);
        }

        if (a.DurationCustom == null)
        {
            cmd.Parameters.AddWithValue("expireWanted", DBNull.Value);
        }
        else
        {
            cmd.Parameters.AddWithValue("expireWanted", a.DurationCustom);
        }

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

    public static BasA FindByReadId(Guid readId)
    {
        using var Connection = new SqliteConnection(ConnectionString);
        Connection.Open();
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
        if (!reader.IsDBNull(1))
        {
            a.Demanded = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt32(1));
        }
        return a;
    }

    public static BasA FindById(Guid id)
    {
        using var Connection = new SqliteConnection(ConnectionString);
        using var selectCmd = Connection.CreateCommand();
        selectCmd.CommandTimeout = CommandTimeout;
        selectCmd.CommandText = "SELECT rid, created, demanded, expire from basa where id=@id";
        selectCmd.Parameters.AddWithValue("id", id);
        Connection.Open();
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
        if (!reader.IsDBNull(2))
        {
            a.DemandedTimestamp = reader.GetInt32(2);
            a.Demanded = DateTimeOffset.FromUnixTimeSeconds(a.DemandedTimestamp.Value);
        }
        return a;
    }

    public static bool Demand(Guid id)
    {
        using var Connection = new SqliteConnection(ConnectionString);
        using var selectCmd = Connection.CreateCommand();
        selectCmd.CommandTimeout = CommandTimeout;
        selectCmd.CommandText = @"UPDATE basa 
                    SET demanded=unixepoch('now')
                    WHERE id=@id
                        AND demanded IS NULL 
                        AND expire > unixepoch('now')";
        selectCmd.Parameters.AddWithValue("id", id);
        Connection.Open();
        var updatedRows = selectCmd.ExecuteNonQuery();
        return updatedRows > 0;
    }

    public void SignReadId(string salt = "salt")
    {
        if (ReadId == null)
        {
            return;
        }
        var input = $"{salt}{ReadId}{DemandedTimestamp}";
        using var sha256Hash = SHA256.Create();
        var buffer = Encoding.ASCII.GetBytes(input);
        var bytes = sha256Hash.ComputeHash(buffer);
        var builder = new StringBuilder();
        for (int i = 0; i < 3; i++)
        {
            builder.Append(bytes[i].ToString("x2")); // "x2" for hexadecimal format
        }
        Signature = builder.ToString();
    }

    public string? CannotDemandReason()
    {
        if (Expire > DateTimeOffset.UtcNow)
        {
            return "expired";
        }
        if (Demanded.HasValue)
        {
            return "already_demanded";
        }
        return null;
    }
}
