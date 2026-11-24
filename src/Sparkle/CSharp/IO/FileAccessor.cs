namespace Sparkle.CSharp.IO;

public static class FileAccessor {
    
    /// <summary>
    /// A static synchronization object used to coordinate access to file-related operations
    /// within the <see cref="FileAccessor"/> class. This ensures thread safety when performing
    /// concurrent file operations, preventing race conditions.
    /// </summary>
    private static readonly Lock FileLock = new();
    
    /// <summary>
    /// Creates a new file with the specified name and directory.
    /// </summary>
    /// <param name="directory">The directory where the file should be created.</param>
    /// <param name="name">The name of the file to be created.</param>
    /// <param name="replace">Specifies whether to replace an existing file with the same name in the directory. Default is false.</param>
    public static void Create(string directory, string name, bool replace = false) {
        lock (FileLock) {
            Directory.CreateDirectory(directory);
            string path = GetPath(directory, name);
            if (!replace && File.Exists(path)) {
                return;
            }

            // OpenOrCreate avoids truncating a file created by another caller.
            using FileStream stream = new FileStream(path, replace ? FileMode.Create : FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
        }
    }
    
    /// <summary>
    /// Writes the specified text to the file at the given path.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    /// <param name="text">The text to be written to the file.</param>
    public static void Write(string path, string text) {
        lock (FileLock) {
            using (FileStream stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read)) {
                using (StreamWriter writer = new StreamWriter(stream)) {
                    writer.Write(text);
                }
            }
        }
    }
    
    /// <summary>
    /// Writes the specified text followed by a new line to the file at the given path.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    /// <param name="text">The text to be written to the file.</param>
    public static void WriteLine(string path, string text) {
        lock (FileLock) {
            using (FileStream stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read)) {
                using (StreamWriter writer = new StreamWriter(stream)) {
                    writer.WriteLine(text);
                }
            }
        }
    }
    
    /// <summary>
    /// Writes the specified text to the file at the given path, replacing any existing content.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    /// <param name="text">The text to be written to the file.</param>
    public static void WriteAll(string path, string text) {
        lock (FileLock) {
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) {
                using (StreamWriter writer = new StreamWriter(stream)) {
                    writer.Write(text);
                }
            }
        }
    }

    /// <summary>
    /// Writes text to a temporary file in the destination directory, then replaces the destination.
    /// Readers see either the old content or the complete new content on file systems supporting atomic renames.
    /// </summary>
    /// <param name="path">The path to the destination file. Its directory must already exist.</param>
    /// <param name="text">The text to write.</param>
    public static void WriteAllAtomic(string path, string text) {
        lock (FileLock) {
            string fullPath = Path.GetFullPath(path);
            string temporaryPath = Path.Combine(Path.GetDirectoryName(fullPath)!, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");

            try {
                using (FileStream stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
                    using StreamWriter writer = new StreamWriter(stream);
                    writer.Write(text);
                    writer.Flush();
                    stream.Flush(true);
                }

                File.Move(temporaryPath, fullPath, true);
            }
            finally {
                File.Delete(temporaryPath);
            }
        }
    }
    
    /// <summary>
    /// Writes all lines of text to the file at the specified path.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    /// <param name="text">An array of strings containing the lines of text to write to the file.</param>
    public static void WriteAllLines(string path, string[] text) {
        lock (FileLock) {
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) {
                using (StreamWriter writer = new StreamWriter(stream)) {
                    for (int i = 0; i < text.Length; i++) {
                        writer.WriteLine(text[i]);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Reads all text from the file at the specified path.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    /// <returns>A string containing the text read from the file.</returns>
    public static string Read(string path) {
        lock (FileLock) {
            using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite)) {
                using (StreamReader reader = new StreamReader(stream)) {
                    return reader.ReadToEnd();
                }
            }
        }
    }
    
    /// <summary>
    /// Retrieves a specific line of text from a file at the given path.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    /// <param name="index">The index of the line to retrieve.</param>
    /// <returns>The specified line of text from the file.</returns>
    public static string ReadLine(string path, int index) {
        lock (FileLock) {
            return ReadAllLines(path)[index];
        }
    }
    
    /// <summary>
    /// Reads all text from the file at the specified path.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    /// <returns>The text content of the file.</returns>
    public static string ReadAll(string path) {
        lock (FileLock) {
            using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite)) {
                using (StreamReader reader = new StreamReader(stream)) {
                    return reader.ReadToEnd();
                }
            }
        }
    }
    
    /// <summary>
    /// Reads all lines from the file at the specified path.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    /// <returns>An array of strings containing all the lines from the file.</returns>
    public static string[] ReadAllLines(string path) {
        lock (FileLock) {
            using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite)) {
                using (StreamReader reader = new StreamReader(stream)) {
                    List<string> lines = new List<string>();
                    string? line;
                    while ((line = reader.ReadLine()) != null) {
                        lines.Add(line);
                    }

                    return lines.ToArray();
                }
            }
        }
    }
    
    /// <summary>
    /// Clears the content of the file at the specified path.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    public static void Clear(string path) {
        lock (FileLock) {
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) {
                stream.SetLength(0);
            }
        }
    }
    
    /// <summary>
    /// Combines the specified file name and directory path to create a full file path.
    /// </summary>
    /// <param name="directory">The directory where the file is located.</param>
    /// <param name="name">The name of the file.</param>
    /// <returns>The full path to the file.</returns>
    public static string GetPath(string directory, string name) {
        return Path.Combine(directory, name);
    }
}
