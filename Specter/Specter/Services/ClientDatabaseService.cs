using Specter.Models;
using SQLite;

namespace Specter.Services;

public class ClientDatabaseService
{
    private SQLiteAsyncConnection? _db;
    private bool _isInitialized = false;
    private static readonly object _initLock = new();
    private string? _dbPassword;

    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, "specter_clients.db");

    public bool HasPassword => !string.IsNullOrEmpty(_dbPassword);

    public void SetPassword(string password)
    {
        _dbPassword = password;
    }

    public async Task CloseAndClearAsync()
    {
        if (_db != null)
        {
            await _db.CloseAsync();
            _db = null;
        }
        _isInitialized = false;
        _dbPassword = null;
    }

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        await InitAsync();
        return _db!;
    }

    private async Task InitAsync()
    {
        if (_isInitialized && _db != null) return;

        if (string.IsNullOrEmpty(_dbPassword))
            throw new InvalidOperationException(
                "Client database password not set. Call SetPassword() before opening.");

        lock (_initLock)
        {
            if (_isInitialized && _db != null) return;
            var options = new SQLiteConnectionString(
                DatabasePath,
                storeDateTimeAsTicks: false,
                key: _dbPassword);
            _db = new SQLiteAsyncConnection(options);
        }

        await _db!.CreateTableAsync<Client>();
        _isInitialized = true;
    }

    public async Task<List<Client>> GetClientsAsync(string username)
    {
        var conn = await GetConnectionAsync();
        return await conn.Table<Client>()
            .Where(c => c.Username == username)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Client?> GetClientAsync(int id)
    {
        var conn = await GetConnectionAsync();
        return await conn.Table<Client>()
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveClientAsync(Client client)
    {
        var conn = await GetConnectionAsync();
        client.UpdatedAt = DateTime.UtcNow;
        if (client.Id == 0)
        {
            client.CreatedAt = DateTime.UtcNow;
            return await conn.InsertAsync(client);
        }
        return await conn.UpdateAsync(client);
    }

    public async Task DeleteClientAsync(int id)
    {
        var conn = await GetConnectionAsync();
        await conn.DeleteAsync<Client>(id);
    }

    public async Task<List<Client>> SearchClientsAsync(
        string username, string query)
    {
        var conn = await GetConnectionAsync();
        query = query.ToLowerInvariant();
        var all = await conn.Table<Client>()
            .Where(c => c.Username == username)
            .ToListAsync();
        return all.Where(c =>
            c.Name.ToLowerInvariant().Contains(query) ||
            c.PhoneNumber.Contains(query) ||
            c.ClientNumber.Contains(query) ||
            c.Notes.ToLowerInvariant().Contains(query))
            .OrderBy(c => c.Name)
            .ToList();
    }
}
