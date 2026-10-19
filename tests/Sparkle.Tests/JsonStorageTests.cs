using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sparkle.CSharp.IO;
using Sparkle.CSharp.IO.Json;

namespace Sparkle.Tests;

public class JsonStorageTests : IDisposable {
    private const string EncryptionKey = "0123456789abcdef";
    private readonly string _directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"sparkle-tests-{Guid.NewGuid():N}");

    [Theory]
    [InlineData("")]
    [InlineData(EncryptionKey)]
    public void ValuesSurviveReopening(string encryptionKey) {
        JsonStorage storage = new JsonStorage("save", this._directory, encryptionKey);
        Assert.Empty(storage.GetData());
        storage.AddValue("score", 42);
        storage.SetValue("player", "Zoë 🚀");

        JsonStorage reopened = new JsonStorage("save", this._directory, encryptionKey);
        Assert.Equal(42, reopened.GetValue<int>("score"));
        Assert.Equal("Zoë 🚀", reopened.GetValue<string>("player"));
        Assert.True(reopened.IsValid());
        Assert.False(reopened.TryAddValue("score", 99));
        Assert.Throws<ArgumentException>(() => reopened.AddValue("score", 99));
        Assert.True(reopened.TryRemoveValue("score"));
        Assert.False(reopened.TryRemoveValue("score"));
        Assert.Throws<KeyNotFoundException>(() => reopened.RemoveValue("score"));
    }

    [Theory]
    [InlineData("{ broken json")]
    [InlineData("[]")]
    [InlineData("null")]
    public void InvalidExistingContentIsNeverOverwritten(string original) {
        Directory.CreateDirectory(this._directory);
        string path = System.IO.Path.Combine(this._directory, "save.json");
        File.WriteAllText(path, original);

        Assert.Throws<JsonReaderException>(() => new JsonStorage("save", this._directory));
        Assert.Equal(original, File.ReadAllText(path));
    }

    [Fact]
    public void WrongEncryptionKeyDoesNotOverwriteData() {
        JsonStorage storage = new JsonStorage("save", this._directory, EncryptionKey);
        storage.SetValue("score", 42);
        byte[] original = File.ReadAllBytes(storage.Path);

        Assert.ThrowsAny<Exception>(() => new JsonStorage("save", this._directory, "fedcba9876543210"));
        Assert.Equal(original, File.ReadAllBytes(storage.Path));
        Assert.Equal(42, new JsonStorage("save", this._directory, EncryptionKey).GetValue<int>("score"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(EncryptionKey)]
    public void EmptyExistingFilesAreInitialized(string encryptionKey) {
        Directory.CreateDirectory(this._directory);
        File.WriteAllText(System.IO.Path.Combine(this._directory, "save.json"), " \n\t");
        Assert.Empty(new JsonStorage("save", this._directory, encryptionKey).GetData());
    }

    [Theory]
    [InlineData("")]
    [InlineData(EncryptionKey)]
    public void NullValuesCanBeAddedAndUpdated(string encryptionKey) {
        JsonStorage storage = new JsonStorage("save", this._directory, encryptionKey);
        storage.AddValue<string?>("player", null);
        storage.SetValue("score", 42);
        storage.SetValue<int?>("score", null);

        Assert.True(storage.ContainsKey("player"));
        Assert.Equal(JTokenType.Null, storage.GetData()["player"]!.Type);
        Assert.Equal(JTokenType.Null, storage.GetData()["score"]!.Type);
        Assert.False(storage.TryGetValue<string>("player", out string? player));
        Assert.Null(player);
    }

    [Fact]
    public void InvalidConversionsReturnDefaultWithoutChangingData() {
        JsonStorage storage = new JsonStorage("save", this._directory);
        storage.SetValue("score", "not a number");
        Assert.Equal(0, storage.GetValue<int>("score"));
        Assert.False(storage.TryGetValue<int>("score", out int value));
        Assert.Equal(0, value);
        Assert.False(storage.TryGetValue<string>("missing", out _));
        Assert.Equal("not a number", storage.GetValue<string>("score"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(EncryptionKey)]
    public void BatchUpdatesPreserveOtherKeysAndSupportNull(string encryptionKey) {
        JsonStorage storage = new JsonStorage("save", this._directory, encryptionKey);
        storage.SetValue("keep", true);
        storage.SetValue("score", 1);
        storage.SetValues(new Dictionary<string, object?> {
            ["score"] = 42,
            ["player"] = "Zoë",
            ["optional"] = null
        });

        JsonStorage reopened = new JsonStorage("save", this._directory, encryptionKey);
        Assert.True(reopened.GetValue<bool>("keep"));
        Assert.Equal(42, reopened.GetValue<int>("score"));
        Assert.Equal("Zoë", reopened.GetValue<string>("player"));
        Assert.Equal(JTokenType.Null, reopened.GetData()["optional"]!.Type);
        Assert.Single(Directory.GetFiles(this._directory));
    }

    [Fact]
    public void FailedBatchSerializationPreservesOriginalFile() {
        JsonStorage storage = new JsonStorage("save", this._directory);
        storage.SetValue("score", 1);
        byte[] original = File.ReadAllBytes(storage.Path);
        RecursiveValue recursive = new RecursiveValue();

        Assert.Throws<JsonSerializationException>(() => storage.SetValues(new Dictionary<string, object?> {
            ["score"] = 42,
            ["invalid"] = recursive
        }));
        Assert.Equal(original, File.ReadAllBytes(storage.Path));
        Assert.Single(Directory.GetFiles(this._directory));
    }

    [Fact]
    public void FailedBatchEnumerationPreservesOriginalFile() {
        JsonStorage storage = new JsonStorage("save", this._directory);
        storage.SetValue("score", 1);
        byte[] original = File.ReadAllBytes(storage.Path);

        Assert.Throws<InvalidOperationException>(() => storage.SetValues(FailingValues()));
        Assert.Equal(original, File.ReadAllBytes(storage.Path));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConfigReconcilesDefaultsAndResetsWithoutAliasingInput(bool removeUnknownValues) {
        JsonStorage storage = new JsonStorage("config", this._directory);
        storage.SetValue("volume", 80);
        storage.SetValue("legacy", true);
        Dictionary<string, object> defaults = new Dictionary<string, object> {
            ["volume"] = 50,
            ["optional"] = null!
        };

        JsonConfig config = new JsonConfig("config", this._directory, defaults, removeUnknownValues: removeUnknownValues);
        Assert.Equal(80, config.GetValue<int>("volume"));
        Assert.Equal(!removeUnknownValues, config.ContainsKey("legacy"));
        Assert.Equal(JTokenType.Null, config.GetData()["optional"]!.Type);
        defaults["volume"] = 0;
        defaults["injected"] = true;
        Assert.Equal(50, config.GetDefaultValues()["volume"]);
        Assert.False(config.GetDefaultValues().ContainsKey("injected"));
        IDictionary<string, object> exposed = Assert.IsAssignableFrom<IDictionary<string, object>>(config.GetDefaultValues());
        Assert.Throws<NotSupportedException>(() => exposed["volume"] = 0);

        config.Reset();
        Assert.Equal(50, config.GetValue<int>("volume"));
        Assert.False(config.ContainsKey("legacy"));
        Assert.False(config.ContainsKey("injected"));
        Assert.Equal(JTokenType.Null, config.GetData()["optional"]!.Type);
    }

    [Fact]
    public void ConfigDoesNotRewriteExistingValidValues() {
        Directory.CreateDirectory(this._directory);
        string path = System.IO.Path.Combine(this._directory, "config.json");
        string original = "{\"volume\":80}";
        File.WriteAllText(path, original);
        _ = new JsonConfig("config", this._directory, new Dictionary<string, object> { ["volume"] = 50 });
        Assert.Equal(original, File.ReadAllText(path));
    }

    [Fact]
    public void AtomicWritesReplaceCompleteContentAndCleanUpOnFailure() {
        Directory.CreateDirectory(this._directory);
        string path = System.IO.Path.Combine(this._directory, "save.txt");
        FileAccessor.WriteAllAtomic(path, "old content");
        FileAccessor.WriteAllAtomic(path, "new content 🚀");
        Assert.Equal("new content 🚀", File.ReadAllText(path));
        Assert.Single(Directory.GetFiles(this._directory));

        string blockedPath = System.IO.Path.Combine(this._directory, "blocked");
        Directory.CreateDirectory(blockedPath);
        Assert.ThrowsAny<IOException>(() => FileAccessor.WriteAllAtomic(blockedPath, "cannot replace a directory"));
        Assert.True(Directory.Exists(blockedPath));
        Assert.Single(Directory.GetFiles(this._directory));
        Assert.Equal("new content 🚀", File.ReadAllText(path));
    }

    [Fact]
    public void CreatePreservesExistingContentUnlessReplacementIsRequested() {
        FileAccessor.Create(this._directory, "save.txt");
        string path = System.IO.Path.Combine(this._directory, "save.txt");
        File.WriteAllText(path, "keep");
        Parallel.For(0, 20, _ => FileAccessor.Create(this._directory, "save.txt"));
        Assert.Equal("keep", File.ReadAllText(path));
        FileAccessor.Create(this._directory, "save.txt", true);
        Assert.Empty(File.ReadAllText(path));
    }

    private static IEnumerable<KeyValuePair<string, object?>> FailingValues() {
        yield return new KeyValuePair<string, object?>("score", 42);
        throw new InvalidOperationException("Enumeration failed.");
    }

    private sealed class RecursiveValue {
        public RecursiveValue Self => this;
    }

    public void Dispose() {
        if (Directory.Exists(this._directory)) {
            Directory.Delete(this._directory, true);
        }
    }
}
