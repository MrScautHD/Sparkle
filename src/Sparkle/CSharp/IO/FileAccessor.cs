namespace Sparkle.CSharp.IO;

public static class FileAccessor {
    
    /// <summary>
    /// Creates a new file with the specified name and directory.
    /// </summary>
    /// <param name="directory">The directory where the file should be created.</param>
    /// <param name="name">The name of the file to be created.</param>
    /// <param name="replace">Specifies whether to replace an existing file with the same name in the directory. Default is false.</param>
    public static void Create(string directory, string name, bool replace = false) {
        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }

        if (replace || !File.Exists(GetPath(directory, name))) {
            File.Create(GetPath(directory, name)).Close();
        }
    }

    /// <summary>
    /// Writes the specified text to the file at the given path.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    /// <param name="text">The text to be written to the file.</param>
    public static void Write(string path, string text) {
        using StreamWriter writer = new StreamWriter(path, true);
        writer.Write(text);
    }

    /// <summary>
    /// Writes the specified text followed by a new line to the file at the given path.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    /// <param name="text">The text to be written to the file.</param>
    public static void WriteLine(string path, string text) {
        using StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(text);
    }

    /// <summary>
    /// Writes the specified text to the file at the given path, replacing any existing content.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    /// <param name="text">The text to be written to the file.</param>
    public static void WriteAll(string path, string text) {
        File.WriteAllText(path, text);
    }

    /// <summary>
    /// Writes all lines of text to the file at the specified path.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    /// <param name="text">An array of strings containing the lines of text to write to the file.</param>
    public static void WriteAllLines(string path, string[] text) {
        File.WriteAllLines(path, text);
    }

    /// <summary>
    /// Reads all text from the file at the specified path.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    /// <returns>A string containing the text read from the file.</returns>
    public static string Read(string path) {
        return File.ReadAllText(path);
    }

    /// <summary>
    /// Retrieves a specific line of text from a file at the given path.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    /// <param name="index">The index of the line to retrieve.</param>
    /// <returns>The specified line of text from the file.</returns>
    public static string ReadLine(string path, int index) {
        return File.ReadAllLines(path)[index];
    }

    /// <summary>
    /// Reads all text from the file at the specified path.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    /// <returns>The text content of the file.</returns>
    public static string ReadAll(string path) {
        return File.ReadAllText(path);
    }

    /// <summary>
    /// Reads all lines from the file at the specified path.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    /// <returns>An array of strings containing all the lines from the file.</returns>
    public static string[] ReadAllLines(string path) {
        return File.ReadAllLines(path);
    }

    /// <summary>
    /// Clears the content of the file at the specified path.
    /// </summary>
    /// <param name="path">The full path to the file.</param>
    public static void Clear(string path) {
        File.WriteAllText(path, string.Empty);
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