using Specter.Models;
using SQLite;

namespace Specter.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _db;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _isInitialized;

    public DatabaseService()
    {
    }

    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, "specter.db");

    public async Task<ISQLiteAsyncConnection> GetConnectionAsync()
    {
        if (!_isInitialized)
            await InitAsync();

        return _db!;
    }

    public async Task InitAsync()
    {
        if (_isInitialized)
            return;

        await _initLock.WaitAsync();
        try
        {
            if (_isInitialized)
                return;

            _db = new SQLiteAsyncConnection(
                DatabasePath,
                SQLiteOpenFlags.ReadWrite |
                SQLiteOpenFlags.Create |
                SQLiteOpenFlags.SharedCache);

            await _db.CreateTableAsync<User>();

            try { await _db.ExecuteAsync("ALTER TABLE users ADD COLUMN PasswordHash BLOB"); } catch { }
            try { await _db.ExecuteAsync("ALTER TABLE users ADD COLUMN PasswordSalt BLOB"); } catch { }
            try { await _db.ExecuteAsync("ALTER TABLE users ADD COLUMN DisplayName TEXT NOT NULL DEFAULT ''"); } catch { }
            try { await _db.ExecuteAsync("ALTER TABLE users ADD COLUMN CreatedAt TEXT"); } catch { }

            _isInitialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task EnsureTableAsync<T>() where T : new()
    {
        await InitAsync();
        await _db!.CreateTableAsync<T>();
    }
}
