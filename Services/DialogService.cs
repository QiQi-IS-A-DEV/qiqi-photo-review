using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using PhotoFileFilter.Core;

namespace PhotoFileFilter.Services;

public interface IDialogService
{
    string? SelectTextFile();
    string[]? SelectReviewSources(string? initialFolder);
    string? SelectFolder(string title);
    string? SaveReport();
    string? SaveReviewNameList(string suggestedName);
    bool ConfirmCopy(int count, string source, string destination, string policy);
    bool ConfirmClearReviewSession();
    void ShowError(string message);
    void OpenFolder(string path);
    void CopyText(string text);
    void RevealFile(string path);
    void ShowPreview(PhotoFile file);
    void PlayCompletionSound();
    void OpenWindowsThumbnailCleanup();
}
public sealed class DialogService : IDialogService
{
    public string? SelectTextFile()
    {
        var dialog = new OpenFileDialog { Title = "Choose a filename list", Filter = "TXT list (*.txt)|*.txt", CheckFileExists = true };
        return dialog.ShowDialog(Application.Current.MainWindow) == true ? dialog.FileName : null;
    }
    public string[]? SelectReviewSources(string? initialFolder)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Preview photos and import the current folder",
            Filter = "Supported photos|*.jpg;*.jpeg;*.png;*.tif;*.tiff;*.heic;*.arw;*.cr2;*.cr3;*.nef;*.raf;*.orf;*.rw2;*.dng|All files|*.*",
            CheckFileExists = false,
            CheckPathExists = true,
            ValidateNames = false,
            Multiselect = false,
            FileName = "Import this folder"
        };
        if (Directory.Exists(initialFolder)) dialog.InitialDirectory = initialFolder;
        if (dialog.ShowDialog(Application.Current.MainWindow) != true) return null;
        var selected = dialog.FileName;
        var folder = Directory.Exists(selected) ? selected : Path.GetDirectoryName(selected);
        return Directory.Exists(folder) ? [folder] : null;
    }
    public string? SelectFolder(string title)
    {
        var dialog = new OpenFolderDialog { Title = title, Multiselect = false };
        return dialog.ShowDialog(Application.Current.MainWindow) == true ? dialog.FolderName : null;
    }
    public string? SaveReport()
    {
        var dialog = new SaveFileDialog { Title = "Save report", FileName = "PhotoFileFilter-report.txt", Filter = "Text (*.txt)|*.txt" };
        return dialog.ShowDialog(Application.Current.MainWindow) == true ? dialog.FileName : null;
    }
    public string? SaveReviewNameList(string suggestedName)
    {
        var dialog = new SaveFileDialog { Title = "Export filename list", FileName = suggestedName, Filter = "TXT list (*.txt)|*.txt" };
        return dialog.ShowDialog(Application.Current.MainWindow) == true ? dialog.FileName : null;
    }
    public bool ConfirmCopy(int count, string source, string destination, string policy) =>
        MessageBox.Show(Application.Current.MainWindow,
            $"Copy {count:N0} files?\n\nFrom:\n{source}\n\nTo:\n{destination}\n\nIf a filename already exists: {policy}\n\nOriginal photos remain unchanged.",
            "Confirm Copy", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK;
    public bool ConfirmClearReviewSession() =>
        MessageBox.Show(Application.Current.MainWindow,
            "Clear the current review session?\n\nThe open photo list will be closed. Ratings, flags, color labels, and original files will be preserved.",
            "Clear Review Session", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK;
    public void ShowError(string message) => MessageBox.Show(Application.Current.MainWindow, message, "Photo File Filter", MessageBoxButton.OK, MessageBoxImage.Warning);
    public void OpenFolder(string path) => Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true });
    public void CopyText(string text) => Clipboard.SetText(text);
    public void RevealFile(string path)
    {
        if (!System.IO.File.Exists(path)) throw new System.IO.FileNotFoundException("The file no longer exists. Please scan again.", path);
        Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true });
    }
    public void ShowPreview(PhotoFile file) => new PreviewWindow(file) { Owner = Application.Current.MainWindow }.ShowDialog();
    public void PlayCompletionSound() { try { System.Media.SystemSounds.Asterisk.Play(); } catch (InvalidOperationException) { } }
    public void OpenWindowsThumbnailCleanup()
    {
        var drive = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.Windows))?.TrimEnd('\\') ?? "C:";
        Process.Start(new ProcessStartInfo("cleanmgr.exe", $"/d {drive}") { UseShellExecute = true });
    }
}
