using Specter.Helpers;
using Specter.Models;

namespace Specter.Services;

public class AuthService
{
    public const string SessionUserKey = "specter_session_user";
    public const string SessionPasswordKey = "specter_session_password";

    private readonly DatabaseService _db;
    private readonly ClientDatabaseService _clientDb;
    private User? _currentUser;
    private string _lastRegisterError = "";

    public AuthService(DatabaseService db, ClientDatabaseService clientDb)
    {
        _db = db;
        _clientDb = clientDb;
    }

    public User? CurrentUser => _currentUser;
    public bool IsLoggedIn => _currentUser != null;
    public string CurrentUsername => _currentUser?.Username ?? "";
    public string LastRegisterError => _lastRegisterError;

    public async Task<bool> RegisterAsync(string username, string password)
    {
        _lastRegisterError = "";
        username = username?.Trim().ToLowerInvariant() ?? "";

        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
        {
            _lastRegisterError = "Username must be at least 3 characters.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
        {
            _lastRegisterError = "Password must be at least 4 characters.";
            return false;
        }

        if (_db.HasPassword && _db.GetDbPassword() != password)
            await _db.CloseConnectionAndClearPasswordAsync();

        if (File.Exists(DatabaseService.DatabasePath) &&
            !await _db.TryOpenWithPasswordAsync(password))
        {
            _lastRegisterError =
                "This database already exists and cannot be opened with that password.";
            return false;
        }

        _db.SetPassword(password);
        _clientDb.SetPassword(password);

        var conn = await _db.GetConnectionAsync();
        var existing = await conn.Table<User>()
            .Where(user => user.Username == username)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            _lastRegisterError = "Username already exists.";
            return false;
        }

        var (hash, salt) = PasswordHasher.HashPassword(password);
        var user = new User
        {
            Username = username,
            PasswordHash = hash,
            PasswordSalt = salt,
            DisplayName = username,
            CreatedAt = DateTime.UtcNow
        };

        await conn.InsertAsync(user);
        return true;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        username = username?.Trim().ToLowerInvariant() ?? "";

        if (_db.HasPassword && _db.GetDbPassword() != password)
            await _db.CloseConnectionAndClearPasswordAsync();

        bool canOpen = await _db.TryOpenWithPasswordAsync(password);
        if (!canOpen)
            return false;

        _db.SetPassword(password);
        _clientDb.SetPassword(password);

        var conn = await _db.GetConnectionAsync();
        var user = await conn.Table<User>()
            .Where(candidate => candidate.Username == username)
            .FirstOrDefaultAsync();

        if (user?.PasswordHash is not { Length: > 0 } hash ||
            user.PasswordSalt is not { Length: > 0 } salt ||
            !PasswordHasher.Verify(password, salt, hash))
        {
            await _db.CloseConnectionAndClearPasswordAsync();
            return false;
        }

        _currentUser = user;
        await SecureStorage.SetAsync(SessionUserKey, username);
        await SecureStorage.SetAsync(SessionPasswordKey, password);
        return true;
    }

    public async Task Logout()
    {
        _currentUser = null;
        SecureStorage.Remove(SessionUserKey);
        SecureStorage.Remove(SessionPasswordKey);
        await _db.CloseConnectionAndClearPasswordAsync();
        await _clientDb.CloseAndClearAsync();
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        try
        {
            var savedUser = await SecureStorage.GetAsync(SessionUserKey);
            var savedPassword = await SecureStorage.GetAsync(SessionPasswordKey);

            if (string.IsNullOrEmpty(savedUser) || string.IsNullOrEmpty(savedPassword))
                return false;

            _db.SetPassword(savedPassword);
            _clientDb.SetPassword(savedPassword);

            if (await LoginAsync(savedUser, savedPassword))
                return true;

            await _db.CloseConnectionAndClearPasswordAsync();
            SecureStorage.Remove(SessionUserKey);
            SecureStorage.Remove(SessionPasswordKey);
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[Auth.Restore] Failed: {ex.GetType().FullName}: {ex.Message}");
            await _db.CloseConnectionAndClearPasswordAsync();
            SecureStorage.Remove(SessionUserKey);
            SecureStorage.Remove(SessionPasswordKey);
            return false;
        }
    }
}
