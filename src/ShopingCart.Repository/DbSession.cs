using Microsoft.Data.Sqlite;

namespace ShopingCart.Repository;

// Scoped: หนึ่ง HTTP request ใช้ connection และ transaction ร่วมกันทุก repository
public sealed class DbSession : IDisposable
{
    public SqliteConnection Connection { get; }
    public SqliteTransaction? Transaction { get; private set; }

    public DbSession(string connectionString)
    {
        Connection = new SqliteConnection(connectionString);
        Connection.Open();
    }

    public void BeginTransaction()
    {
        // ล็อกการเขียนตั้งแต่เริ่ม เพื่อให้การอ่านสต็อกและตัดสต็อกไม่แข่งกับผู้ซื้ออื่น
        Transaction = Connection.BeginTransaction(deferred: false);
    }

    public void Commit()
    {
        Transaction!.Commit();
        Transaction.Dispose();
        Transaction = null;
    }

    public void Dispose()
    {
        // ถ้ามี exception ก่อน Commit การ Dispose จะ rollback ทุกคำสั่งใน transaction
        Transaction?.Dispose();
        Connection.Dispose();
    }
}
