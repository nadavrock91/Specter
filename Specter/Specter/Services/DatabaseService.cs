using Specter.Models;
using SQLite;

namespace Specter.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _db;
    private string? _dbPassword;
    private static readonly object _initLock = new();
    private bool _isInitialized;

    public DatabaseService()
    {
    }

    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, "specter.db");

    public bool HasPassword => !string.IsNullOrEmpty(_dbPassword);

    public void SetPassword(string password)
    {
        _dbPassword = password;
    }

    public string? GetDbPassword() => _dbPassword;

    public async Task<bool> TryOpenWithPasswordAsync(string password)
    {
        if (!File.Exists(DatabasePath))
            return true;

        try
        {
            var options = new SQLiteConnectionString(
                DatabasePath,
                storeDateTimeAsTicks: false,
                key: password);
            var testConn = new SQLiteAsyncConnection(options);
            await testConn.ExecuteScalarAsync<int>(
                "SELECT count(*) FROM sqlite_master");
            await testConn.CloseAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task CloseConnectionAndClearPasswordAsync()
    {
        if (_db != null)
        {
            await _db.CloseAsync();
            _db = null;
        }

        _isInitialized = false;
        _dbPassword = null;
    }

    public async Task<ISQLiteAsyncConnection> GetConnectionAsync()
    {
        if (!_isInitialized)
            await InitAsync();

        return _db!;
    }

    public async Task InitAsync()
    {
        if (_isInitialized && _db != null)
            return;

        if (string.IsNullOrEmpty(_dbPassword))
            throw new InvalidOperationException(
                "Database password not set. Call SetPassword() before opening the database.");

        lock (_initLock)
        {
            if (_isInitialized && _db != null)
                return;

            var options = new SQLiteConnectionString(
                DatabasePath,
                storeDateTimeAsTicks: false,
                key: _dbPassword);
            _db = new SQLiteAsyncConnection(options);
        }

        await _db.CreateTableAsync<User>();

        try { await _db.ExecuteAsync("ALTER TABLE users ADD COLUMN PasswordHash BLOB"); } catch { }
        try { await _db.ExecuteAsync("ALTER TABLE users ADD COLUMN PasswordSalt BLOB"); } catch { }
        try { await _db.ExecuteAsync("ALTER TABLE users ADD COLUMN DisplayName TEXT NOT NULL DEFAULT ''"); } catch { }
        try { await _db.ExecuteAsync("ALTER TABLE users ADD COLUMN CreatedAt TEXT"); } catch { }

        _isInitialized = true;
    }

    public async Task EnsureTableAsync<T>() where T : new()
    {
        await InitAsync();
        await _db!.CreateTableAsync<T>();
    }
}
