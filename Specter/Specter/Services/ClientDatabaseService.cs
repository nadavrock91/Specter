using Specter.Models;
using SQLite;

namespace Specter.Services;

public class ClientDatabaseService
{
    private SQLiteAsyncConnection? _db;
    private bool _isInitialized = false;
    private static readonly SemaphoreSlim _initLock = new(1, 1);
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

        await _initLock.WaitAsync();
        try
        {
            if (_isInitialized && _db != null) return;
            var options = new SQLiteConnectionString(
                DatabasePath,
                storeDateTimeAsTicks: false,
                key: _dbPassword);
            _db = new SQLiteAsyncConnection(options);

            // Add the column before sqlite-net's automatic schema migration so
            // existing rows and future inserts both get a non-null zero default.
            await _db.RunInTransactionAsync(conn =>
            {
                var columns = conn.GetTableInfo("clients");
                if (columns.Count > 0 && !columns.Any(c => c.Name == nameof(Client.CallAttemptCount)))
                    conn.Execute("ALTER TABLE clients ADD COLUMN CallAttemptCount INTEGER NOT NULL DEFAULT 0");
                conn.CreateTable<Client>();
            });
            _isInitialized = true;
        }
        finally
        {
            _initLock.Release();
        }
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
            client.CallAttemptCount = 0;
            return await conn.InsertAsync(client);
        }
        // Editing a stale form must never overwrite a separately recorded attempt.
        return await conn.ExecuteAsync(
            "UPDATE clients SET Name = ?, PhoneNumber = ?, ClientNumber = ?, Notes = ?, ScheduledDate = ?, UpdatedAt = ? WHERE Id = ? AND Username = ?",
            client.Name, client.PhoneNumber, client.ClientNumber, client.Notes,
            client.ScheduledDate, client.UpdatedAt, client.Id, client.Username);
    }

    public async Task<int> LogCallAttemptAsync(int clientId, string username)
    {
        var conn = await GetConnectionAsync();
        int total = 0;
        await conn.RunInTransactionAsync(db =>
        {
            int changed = db.Execute(
                "UPDATE clients SET CallAttemptCount = CallAttemptCount + 1, UpdatedAt = ? WHERE Id = ? AND Username = ?",
                DateTime.UtcNow, clientId, username);
            if (changed != 1)
                throw new InvalidOperationException("The client no longer exists.");
            total = db.ExecuteScalar<int>("SELECT CallAttemptCount FROM clients WHERE Id = ?", clientId);
        });
        return total;
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
