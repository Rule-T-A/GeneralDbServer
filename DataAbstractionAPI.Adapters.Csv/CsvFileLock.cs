namespace DataAbstractionAPI.Adapters.Csv;

/// <summary>
/// Provides file locking mechanism for CSV operations to prevent concurrent access.
/// Uses a separate .lock file to coordinate exclusive access.
/// </summary>
public class CsvFileLock : IDisposable
{
    private readonly string _lockPath;
    private FileStream? _lockStream;
    private bool _disposed = false;

    public CsvFileLock(string lockPath)
    {
        _lockPath = lockPath;
        AcquireLock();
    }

    /// <summary>
    /// Acquires an exclusive lock by creating a lock file.
    /// </summary>
    private void AcquireLock()
    {
        try
        {
            // Try to create the lock file with exclusive access
            _lockStream = new FileStream(
                _lockPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None
            );

            // Write current process info to lock file for debugging
            var processInfo = System.Text.Encoding.UTF8.GetBytes(
                $"Locked by: {System.Diagnostics.Process.GetCurrentProcess().Id} at {DateTime.UtcNow:O}"
            );
            _lockStream.Write(processInfo);
            _lockStream.Flush();
        }
        catch (IOException ex)
        {
            if (File.Exists(_lockPath))
            {
                throw new IOException($"File is locked by another process. Lock file: {_lockPath}", ex);
            }
            throw;
        }
    }

    /// <summary>
    /// Releases the lock by deleting the lock file.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _lockStream?.Dispose();

                // Delete lock file if it exists
                try
                {
                    if (File.Exists(_lockPath))
                    {
                        File.Delete(_lockPath);
                    }
                }
                catch
                {
                    // Ignore errors when deleting lock file
                }
            }
            _disposed = true;
        }
    }

    ~CsvFileLock()
    {
        Dispose(false);
    }
}

