namespace DataAbstractionAPI.Adapters.Tests;

using DataAbstractionAPI.Adapters.Csv;

public class CsvFileLockTests : IDisposable
{
    private readonly string _tempTestDir;

    public CsvFileLockTests()
    {
        _tempTestDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempTestDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempTestDir))
        {
            Directory.Delete(_tempTestDir, true);
        }
    }

    [Fact]
    public void CsvFileLock_AcquiresLock_OnCreation()
    {
        // Arrange
        var lockPath = Path.Combine(_tempTestDir, "test.csv.lock");

        // Act
        using (var fileLock = new CsvFileLock(lockPath))
        {
            // Assert
            Assert.True(File.Exists(lockPath), "Lock file should exist");
        }
    }

    [Fact]
    public void CsvFileLock_ReleasesLock_OnDispose()
    {
        // Arrange
        var lockPath = Path.Combine(_tempTestDir, "test.csv.lock");

        // Act
        using (var fileLock = new CsvFileLock(lockPath))
        {
            Assert.True(File.Exists(lockPath));
        }

        // Assert
        Assert.False(File.Exists(lockPath), "Lock file should be deleted after dispose");
    }

    [Fact]
    public void CsvFileLock_PreventsMultipleLocks_OnSameFile()
    {
        // Arrange
        var lockPath = Path.Combine(_tempTestDir, "test.csv.lock");

        // Act & Assert
        using (var firstLock = new CsvFileLock(lockPath))
        {
            Assert.Throws<IOException>(() => new CsvFileLock(lockPath));
        }
    }

    [Fact]
    public void CsvFileLock_AllowsLock_AfterPreviousLockReleased()
    {
        // Arrange
        var lockPath = Path.Combine(_tempTestDir, "test.csv.lock");

        // Act & Assert
        using (var firstLock = new CsvFileLock(lockPath))
        {
            Assert.True(File.Exists(lockPath));
        }

        // Should be able to acquire lock again
        using (var secondLock = new CsvFileLock(lockPath))
        {
            Assert.True(File.Exists(lockPath));
        }
    }
}

