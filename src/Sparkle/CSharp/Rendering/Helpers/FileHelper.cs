using Raylib_cs;

namespace Sparkle.CSharp.Rendering.Helpers;

public class FileHelper {
    
    /// <inheritdoc cref="Raylib.FileExists"/>
    public unsafe bool Exists(sbyte* path) => Raylib.FileExists(path);
    
    /// <inheritdoc cref="Raylib.DirectoryExists"/>
    public unsafe bool DirectoryExists(sbyte* path) => Raylib.DirectoryExists(path);
    
    /// <inheritdoc cref="Raylib.IsFileExtension(string, string)"/>
    public bool IsExtension(string path, string ext) => Raylib.IsFileExtension(path, ext);
    
    /// <inheritdoc cref="Raylib.GetFileExtension"/>
    public unsafe sbyte* GetExtension(sbyte* path) => Raylib.GetFileExtension(path);
    
    /// <inheritdoc cref="Raylib.GetFileName"/>
    public unsafe sbyte* GetName(sbyte* path) => Raylib.GetFileName(path);
    
    /// <inheritdoc cref="Raylib.GetFileNameWithoutExt"/>
    public unsafe sbyte* GetNameWithoutExt(sbyte* path) => Raylib.GetFileNameWithoutExt(path);
    
    
    /// <inheritdoc cref="Raylib.GetDirectoryPath"/>
    public unsafe sbyte* GetDirectoryPath(sbyte* path) => Raylib.GetDirectoryPath(path);
    
    /// <inheritdoc cref="Raylib.GetPrevDirectoryPath"/>
    public unsafe sbyte* GetPrevDirectoryPath(sbyte* path) => Raylib.GetPrevDirectoryPath(path);
    
    /// <inheritdoc cref="Raylib.GetWorkingDirectory"/>
    public unsafe sbyte* GetWorkingDirectory() => Raylib.GetWorkingDirectory();
    
    /// <inheritdoc cref="Raylib.GetApplicationDirectory()"/>
    public unsafe sbyte* GetApplicationDirectory() => Raylib.GetApplicationDirectory();
    
    /// <inheritdoc cref="Raylib.ChangeDirectory"/>
    public unsafe bool ChangeDirectory(sbyte* path) => Raylib.ChangeDirectory(path);
    
    /// <inheritdoc cref="Raylib.IsPathFile"/>
    public unsafe bool IsPathFile(sbyte* path) => Raylib.IsPathFile(path);
    
    /// <inheritdoc cref="Raylib.LoadDirectoryFiles"/>
    public unsafe FilePathList LoadDirectoryFiles(sbyte* path, int* count) => Raylib.LoadDirectoryFiles(path, count);
    
    /// <inheritdoc cref="Raylib.LoadDirectoryFilesEx"/>
    public unsafe FilePathList LoadDirectoryFilesEx(sbyte* path, sbyte* filter, bool scanSubDirs) => Raylib.LoadDirectoryFilesEx(path, filter, scanSubDirs);
    
    /// <inheritdoc cref="Raylib.UnloadDirectoryFiles"/>
    public void UnloadDirectoryFiles(FilePathList files) => Raylib.UnloadDirectoryFiles(files);
    
    
    /// <inheritdoc cref="Raylib.IsFileDropped"/>
    public bool IsFileDropped() => Raylib.IsFileDropped();
    
    /// <inheritdoc cref="Raylib.LoadDroppedFiles"/>
    public FilePathList LoadDroppedFiles() => Raylib.LoadDroppedFiles();
    
    /// <inheritdoc cref="Raylib.UnloadDroppedFiles"/>
    public void UnloadDroppedFiles(FilePathList files) => Raylib.UnloadDroppedFiles(files);
    
    
    /// <inheritdoc cref="Raylib.GetFileModTime(string)"/>
    public long GetFileModTime(string path) => Raylib.GetFileModTime(path);
}