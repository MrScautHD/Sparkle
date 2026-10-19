# JSON storage and configuration

`JsonStorage` persists a JSON object for settings or save data. Missing and empty
files are initialized automatically. Existing non-empty files must contain a valid
JSON object and, when encrypted, must be opened with the correct key.

```csharp
using Sparkle.CSharp.IO.Json;

JsonStorage save = new JsonStorage("player", "saves");
save.SetValue("name", "Ada");
save.SetValue<string?>("checkpoint", null);

// Update multiple keys in a single read and file replacement.
save.SetValues(new Dictionary<string, object?> {
    ["level"] = 3,
    ["score"] = 1200,
    ["checkpoint"] = "forest"
});

if (save.TryGetValue<int>("score", out int score)) {
    Console.WriteLine(score);
}
```

`AddValue` rejects duplicate keys, while `SetValue` and `SetValues` create or replace
them. Null values are stored as JSON `null`; `ContainsKey` still returns `true` for
these entries, while `TryGetValue` returns `false` when the converted result is null.
`GetValue` returns the requested type's default for missing or unconvertible values.
File access, decryption and JSON parsing errors propagate to the caller.

Writes serialize and encrypt the complete object before touching the destination.
The resulting text is written and flushed to a temporary file in the same directory,
then moved over the destination. On file systems supporting atomic renames, readers
see either the previous file or the complete replacement. This does not guarantee
durability through every power failure. The destination directory needs write access.
Failed batch enumeration or serialization leaves the original file unchanged.

Each storage mutation reads the file and then writes it back. Applications must
synchronize concurrent updates, including updates from different storage instances
or processes, to prevent one update from overwriting another.

## Existing invalid files

Opening malformed JSON or using the wrong encryption key now throws instead of
silently resetting the file to `{}`. The original bytes remain available for recovery.
Applications that previously depended on automatic resets should catch the relevant
exception, preserve or repair the file, and explicitly choose whether to start over.
The encryption format is unchanged, so existing encrypted files remain compatible.

## Configuration defaults

```csharp
JsonConfig settings = new JsonConfig("settings", "config", new Dictionary<string, object> {
    ["volume"] = 50,
    ["fullscreen"] = false
}, removeUnknownValues: false);

settings.SetValue("volume", 80);
settings.Reset(); // Restores volume = 50 and fullscreen = false.
```

The constructor preserves existing values, adds missing defaults, and optionally
removes unknown top-level keys (`removeUnknownValues` defaults to `true`). It does
not validate existing value types or recursively merge nested objects. Null defaults
are supported. The defaults dictionary is copied and exposed through a read-only
view; nested mutable values are not deep-copied. `Reset` writes only the defaults,
including when unknown keys were retained during construction.

## Tests

Run the headless regression suite with the .NET 10 SDK:

```sh
dotnet test tests/Sparkle.Tests/Sparkle.Tests.csproj
```

The project is included in `Sparkle.slnx`, so the existing CI build and test steps
also run it. `src/Sparkle.Test` remains the interactive graphical demo.
