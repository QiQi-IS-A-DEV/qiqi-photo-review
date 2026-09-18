using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using PhotoFileFilter;
using PhotoFileFilter.Core;
using PhotoFileFilter.Services;
using PhotoFileFilter.ViewModels;
using PhotoFileFilter.Review;

internal static class Program
{
    private static int _passed;
    private static string _root = "";
    [STAThread]
    private static int Main()
    {
        var app = new Application();
        app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/PhotoFileFilter;component/Styles.xaml", UriKind.Relative) });
        app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
        var result = 0;
        app.Dispatcher.InvokeAsync(async () =>
        {
            try { await Run(); }
            catch (Exception e) { Console.Error.WriteLine(e); result = 1; }
            finally { app.Shutdown(); }
        });
        app.Run();
        return result;
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("FAIL: " + message);
        Console.WriteLine("PASS: " + message); _passed++;
    }
    private static void Throws(Action action, string message)
    {
        try { action(); } catch (ArgumentException) { Check(true, message); return; }
        throw new Exception("FAIL: " + message);
    }
    private static string Write(string relative, string contents = "original-photo-data")
    {
        var path = Path.Combine(_root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, contents); return path;
    }
    private static ScanResult Scan(string source, string[] names, bool recursive = true, bool ignore = true, string? exclude = null) =>
        new PhotoScannerService().Scan(new(source, names, ["JPG", "CR3"], ignore, recursive, exclude), null, CancellationToken.None);
    private static async Task Run()
    {
        _root = Path.Combine(Path.GetTempPath(), "PhotoFileFilter-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
        var parser = new TxtParserService();
        var parsed = parser.Parse("\uFEFFIMG_01.JPG, img_01.jpg\r\nIMG_02\tIMG_03.CR3", new());
        Check(parsed.Names.SequenceEqual(new[] { "IMG_01", "IMG_02", "IMG_03" }) && parsed.DuplicateCount == 1, "Mixed delimiters, BOM, case-insensitive deduplication, extension normalization");
        Check(parser.Parse("Family Photo.JPG\nSecond Photo", new(false, false, true)).Names[0] == "Family Photo", "Spaces preserved when space delimiter is disabled");
        Throws(() => parser.Parse("test", new(false, false, false)), "No delimiter rejected");
        Throws(() => parser.Parse(" , \r\n", new()), "Empty list rejected");
        Throws(() => parser.Parse("../IMG_01", new()), "Path traversal rejected in list");
        var utf16 = Write("utf16.txt"); File.WriteAllText(utf16, "Ảnh_01\r\nIMG_02", Encoding.Unicode);
        Check((await parser.ReadAsync(utf16, new(), default)).Names[0] == "Ảnh_01", "UTF-16 BOM and Vietnamese names supported");
        var source = Path.Combine(_root, "source");
        var first = Write("source/IMG_01.JPG", "jpeg-original");
        Write("source/IMG_01.CR3", "raw-original");
        Write("source/Camera B/img_01.jpg", "other-camera");
        Write("source/Camera B/IMG_02.CR3");
        Write("source/unrelated.PNG");
        Write("source/Selected/IMG_03.JPG");
        var scan = Scan(source, ["img_01", "IMG_02", "IMG_03", "missing"], exclude: Path.Combine(source, "Selected"));
        Check(scan.Files.Count == 4 && scan.Missing.SequenceEqual(new[] { "IMG_03", "missing" }), "Recursive scan, multiple extensions, missing names, destination exclusion");
        Check(Scan(source, ["IMG_01", "IMG_02"], false).Files.Count == 2, "Nonrecursive scan excludes nested images");
        Check(Scan(source, ["IMG_01.JPG"], ignore: false).Files.Count == 2, "Exact-extension mode ignores RAW counterpart");
        Check(Scan(source, ["IMG_01"], ignore: false).Files.Count == 3, "Bare names match selected extensions in exact mode");
        Check(Scan(source, ["IMG_01", "IMG_01.JPG"], ignore: false).Files.Count == 3, "Overlapping requests never duplicate physical files");
        var output = Path.Combine(_root, "output");
        var copier = new FileCopyService();
        var copy = await copier.CopyAsync(scan, new(output, CollisionPolicy.Rename), null, default);
        Check(copy.Copied == 4 && copy.Errors.Count == 0 && File.Exists(Path.Combine(output, "IMG_01 (1).JPG")), "Rename policy preserves duplicate filenames from separate cameras");
        Check(File.ReadAllText(first) == "jpeg-original" && File.ReadAllText(Path.Combine(output, "IMG_01.JPG")) == "other-camera", "Original and copied contents verified");
        Check(!Directory.EnumerateFiles(output, "*.tmp").Any(), "No temporary files after successful copy");
        var skip = await copier.CopyAsync(scan, new(output, CollisionPolicy.Skip), null, default);
        Check(skip.Skipped == 4 && skip.Copied == 0, "Skip policy preserves existing files");
        var single = scan with { Files = scan.Files.Where(f => PathSafety.Equal(f.FullPath, first)).ToArray() };
        var replaced = await copier.CopyAsync(single, new(output, CollisionPolicy.Replace), null, default);
        Check(replaced.Copied == 1 && File.ReadAllText(Path.Combine(output, "IMG_01.JPG")) == "jpeg-original", "Replace commits new copy to existing destination");
        var replaceBatch = await copier.CopyAsync(scan, new(Path.Combine(_root, "replace-batch"), CollisionPolicy.Replace), null, default);
        Check(replaceBatch.Copied == 3 && replaceBatch.Errors.Count == 1, "Replace never overwrites another source's output within a batch");
        var self = await copier.CopyAsync(scan, new(Path.Combine(source, "Camera B"), CollisionPolicy.Replace), null, default);
        Check(self.Errors.Count == 3 && File.ReadAllText(Path.Combine(source, "Camera B/img_01.jpg")) == "other-camera", "Copy destination cannot overwrite any scanned original");
        File.AppendAllText(first, "-changed");
        var changed = await copier.CopyAsync(single, new(output, CollisionPolicy.Replace), null, default);
        Check(changed.Errors.Count == 1 && File.ReadAllText(Path.Combine(output, "IMG_01.JPG")) == "jpeg-original", "Source changed since scan is reported without replacing destination");
        var missingPath = Write("deleted/IMG_99.JPG");
        var deletedScan = Scan(Path.GetDirectoryName(missingPath)!, ["IMG_99"]); File.Delete(missingPath);
        var deleted = await copier.CopyAsync(deletedScan, new(output, CollisionPolicy.Rename), null, default);
        Check(deleted.Errors.Count == 1, "Deleted source becomes per-file error");
        using var cancel = new CancellationTokenSource();
        var cancelResult = await copier.CopyAsync(scan, new(Path.Combine(_root, "cancelled"), CollisionPolicy.Rename), new InlineProgress(_ => cancel.Cancel()), cancel.Token);
        Check(cancelResult.Cancelled && cancelResult.Copied == 0 && !Directory.EnumerateFiles(Path.Combine(_root, "cancelled")).Any(), "Cancellation cleans partial copy and preserves originals");
        Throws(() => PathSafety.Destination(source, false, source, false, ""), "Output equal to source rejected");
        Throws(() => PathSafety.Destination(source, true, "", true, "../escape"), "Invalid output subfolder rejected");
        Throws(() => PathSafety.Destination(source, true, "", true, "CON"), "Windows reserved folder rejected");
        Check(PathSafety.Destination(source, true, "", false, "Selected") == Path.Combine(source, "Selected"), "Alongside mode always creates a subfolder");
        var list = Write("list.txt", "IMG_01,IMG_02,IMG_99");
        var dialogs = new FakeDialogs { ReportPath = Path.Combine(_root, "report.txt") };
        var vm = new MainViewModel(dialogs) { TxtPath = list, SourceFolder = source };
        Check(!vm.CopyCommand.CanExecute(null), "Copy disabled before scan");
        await vm.ScanAsync();
        Check(vm.MatchedCount == 5 && vm.MissingCount == 1 && vm.CopyCommand.CanExecute(null), "View model scan updates counts and enables Copy");
        vm.SpecificFolder = true; vm.OutputFolder = Path.Combine(_root, "vm-output"); vm.Subfolder = false;
        dialogs.Confirm = false;
        await vm.CopyAsync();
        Check(!Directory.Exists(vm.OutputFolder), "Cancelling confirmation creates no destination");
        dialogs.Confirm = true;
        await vm.CopyAsync();
        Check(vm.Status == "Copy complete" && Directory.GetFiles(vm.OutputFolder).Length == 5 && !vm.Busy, "View model confirmation and async copy complete successfully");
        vm.ExportCommand.Execute(null);
        Check(File.ReadAllText(dialogs.ReportPath).Contains("IMG_99"), "Export includes missing names");
        vm.SearchText = "camera b";
        Check(vm.VisibleFiles.Count == 3 && vm.MatchedCount == 5 && vm.CopyLabel.Contains("5"), "Search filters relative paths without changing the copy set");
        vm.SearchText = "img_99";
        Check(vm.VisibleFiles.Count == 0 && vm.VisibleMissing.Count == 1, "Search also filters missing names case-insensitively");
        vm.ClearSearchCommand.Execute(null);
        Check(vm.VisibleFiles.Count == 5, "Clearing search restores all results");
        vm.Alongside = true;
        Check(vm.UseSubfolder, "Alongside mode displays subfolder as enabled even after specific-folder mode disabled it");
        vm.SpecificFolder = true;
        var settingsPath = Path.Combine(_root, "preferences", "settings.json");
        var settings = new SettingsService(settingsPath);
        var remembered = new MainViewModel(dialogs, settings) { TxtPath = list, SourceFolder = source, SpecificFolder = true, OutputFolder = output, Subfolder = false, Policy = 2 };
        remembered.SelectJpgCommand.Execute(null);
        remembered.SaveSettings();
        var restored = new MainViewModel(dialogs, settings);
        Check(restored.TxtPath == list && restored.OutputFolder == output && restored.SpecificFolder && !restored.Subfolder && restored.Policy == 2 && restored.Extensions.Where(e => e.Selected).Select(e => e.Name).SequenceEqual(new[] { "JPG", "JPEG" }), "Preferences restore paths, collision policy, output mode and JPG preset");
        Check(!restored.CopyCommand.CanExecute(null), "Restored settings never restore stale scan results");
        File.WriteAllText(settingsPath, "invalid JSON");
        Check(new MainViewModel(dialogs, settings).Alongside, "Corrupted settings fall back to defaults");
        File.WriteAllText(settingsPath, "{\"Policy\":999,\"Extensions\":null,\"TxtPath\":null}");
        Check(new MainViewModel(dialogs, settings).Policy == 2, "Invalid saved preference values are normalized safely");
        Check(restored.ApplyDroppedPaths([list], "txt") && restored.ApplyDroppedPaths([source], "source"), "TXT and source folder drops are accepted");
        Check(!restored.ApplyDroppedPaths([first], "txt") && !restored.ApplyDroppedPaths([list], "source"), "Invalid drop types are rejected");
        Check(restored.ApplyDroppedPaths([output], "output") && restored.SpecificFolder && restored.OutputFolder == output, "Output folder drop selects specific destination mode");
        vm.SearchText = "nothing";
        vm.CopyMissingCommand.Execute(null);
        Check(dialogs.ClipboardText == "IMG_99", "Copy missing command copies every missing code regardless of search filter");
        vm.SearchText = "";
        vm.SelectedFile = vm.Files[0];
        vm.CopyPathCommand.Execute(null); vm.RevealFileCommand.Execute(null); vm.PreviewCommand.Execute(null);
        Check(dialogs.ClipboardText == vm.SelectedFile.FullPath && dialogs.RevealedPath == vm.SelectedFile.FullPath && dialogs.PreviewedPath == vm.SelectedFile.FullPath, "Row actions target the selected file");
        vm.ToggleIncludedCommand.Execute(null);
        Check(vm.CopyCount == 4 && vm.MatchedCount == 5 && vm.CopyLabel.Contains("4"), "Exclusion updates copy count without changing scan totals");
        vm.OutputFolder = Path.Combine(_root, "subset-output");
        await vm.CopyAsync();
        Check(dialogs.ConfirmedCount == 4 && Directory.GetFiles(vm.OutputFolder).Length == 4, "Confirmation and copy use exactly the included subset");
        // Select a root JPG while excluding the camera JPG: the excluded original still must be protected.
        var protectionScan = Scan(source, ["IMG_01"]);
        var rootJpg = protectionScan.Files.Single(f => f.RelativePath == "IMG_01.JPG");
        var protectedResult = await copier.CopyAsync(protectionScan, new(Path.Combine(source, "Camera B"), CollisionPolicy.Replace, new HashSet<string> { rootJpg.FullPath }), null, default);
        Check(protectedResult.Errors.Count == 1 && File.ReadAllText(Path.Combine(source, "Camera B/img_01.jpg")) == "other-camera", "Excluded originals remain protected from overwrite during subset copy");
        foreach (var file in vm.Files) file.IncludeInCopy = false;
        Check(!vm.CopyCommand.CanExecute(null), "Copy is disabled when every file is excluded");
        vm.IncludeAllCommand.Execute(null);
        Check(vm.CopyCount == 5 && vm.FormatBreakdown.Contains("JPG") && vm.FormatBreakdown.Contains("CR3"), "Restore selection and format breakdown reflect scanned files");
        Check(dialogs.SoundCount >= 2, "Completed copies can notify with sound");
        var previewPath = Path.Combine(_root, "preview", "IMG_700.JPG"); Directory.CreateDirectory(Path.GetDirectoryName(previewPath)!);
        var pixels = Enumerable.Repeat((byte)140, 32 * 24 * 3).ToArray();
        var sample = BitmapSource.Create(32, 24, 96, 96, PixelFormats.Rgb24, null, pixels, 32 * 3);
        var jpeg = new JpegBitmapEncoder(); jpeg.Frames.Add(BitmapFrame.Create(sample));
        using (var stream = File.Create(previewPath)) jpeg.Save(stream);
        var previewService = new PreviewService();
        var jpegResult = await Task.Run(() => previewService.Load(previewPath, default));
        Check(jpegResult.Image is { IsFrozen: true }, "JPEG preview is decoded off-thread into an immutable image");
        var histogram = new HistogramService().Calculate(jpegResult.Image!, default);
        Check(histogram.Chart.IsFrozen && histogram.HighlightPercent == 0 && histogram.ShadowPercent == 0 && histogram.AverageLuminance is > 0.5 and < 0.6, "RGB histogram reports luminance and clipping from the decoded preview");
        var portraitPath = Path.Combine(_root, "preview", "portrait.jpg");
        var metadata = new BitmapMetadata("jpg"); metadata.SetQuery("/app1/ifd/{ushort=274}", (ushort)6);
        var portraitEncoder = new JpegBitmapEncoder(); portraitEncoder.Frames.Add(BitmapFrame.Create(sample, null, metadata, null));
        using (var stream = File.Create(portraitPath)) portraitEncoder.Save(stream);
        var portrait = previewService.Load(portraitPath, default);
        Check(portrait.Image is { PixelWidth: 24, PixelHeight: 32 }, "JPEG preview respects portrait EXIF orientation");
        var rawPreviewPath = Write("preview/IMG_700.CR3", "not-a-raw-decoder-fixture");
        var rawResult = await Task.Run(() => previewService.Load(rawPreviewPath, default));
        Check(rawResult.Image != null && rawResult.Description.Contains("matching JPG preview"), "Unsupported RAW uses an explicitly labelled matching JPG preview");
        Check(ReferenceEquals(jpegResult.Image, previewService.Load(previewPath, default).Image), "Repeated preview reuses the frozen cached bitmap");
        var beforeChange = rawResult.Image;
        File.SetLastWriteTimeUtc(previewPath, DateTime.UtcNow.AddMinutes(1));
        Check(!ReferenceEquals(beforeChange, previewService.Load(rawPreviewPath, default).Image), "Changing a companion JPG invalidates its RAW preview cache");
        var retainedPreview = previewService.Load(previewPath, default);
        previewService.ClearCache();
        Check(!ReferenceEquals(retainedPreview.Image, previewService.Load(previewPath, default).Image), "Release preview memory clears retained Loupe images");
        var evicting = new PreviewService(cacheCapacity: 1);
        var firstCached = evicting.Load(previewPath, default);
        evicting.Load(portraitPath, default);
        Check(!ReferenceEquals(firstCached.Image, evicting.Load(previewPath, default).Image), "LRU evicts previews when the entry limit is reached");
        var tinyCache = new PreviewService(cacheByteLimit: 1);
        Check(!ReferenceEquals(tinyCache.Load(previewPath, default).Image, tinyCache.Load(previewPath, default).Image), "Images above the byte budget are not retained");
        Check(!ReferenceEquals(previewService.Load(previewPath, default, 96).Image, previewService.Load(previewPath, default, 1400).Image), "Thumbnail quality cannot poison the Loupe cache");
        using (var cancelledPreview = new CancellationTokenSource())
        {
            cancelledPreview.Cancel();
            try { previewService.Load(previewPath, cancelledPreview.Token); throw new Exception("Cache ignored cancellation"); }
            catch (OperationCanceledException) { Check(true, "Cache hits respect cancellation"); }
        }
        var deletedCompanion = Write("companion-delete/photo.cr3", "unsupported");
        var companionJpg = Path.ChangeExtension(deletedCompanion, ".jpg");
        File.Copy(previewPath, companionJpg);
        Check(previewService.Load(deletedCompanion, default).Image != null, "Companion index finds an existing JPG");
        File.Delete(companionJpg);
        Check(previewService.Load(deletedCompanion, default).Image == null, "Deleted JPG cannot be returned from a stale preview cache");
        Check(previewService.Load(Path.Combine(_root, "missing.CR3"), default).Image == null, "Missing/unsupported preview reports unavailable without crashing");
        // A lossless, high-frequency fixture exposes accidental thumbnail upscaling and decode caps.
        const int detailWidth = 6000, detailHeight = 900;
        var detailPixels = new byte[detailWidth * detailHeight * 3];
        for (var i = 0; i < detailPixels.Length; i++) detailPixels[i] = (byte)(i % 251);
        var detailSource = BitmapSource.Create(detailWidth, detailHeight, 96, 96, PixelFormats.Bgr24, null, detailPixels, detailWidth * 3);
        var detailPath = Path.Combine(_root, "detail", "detail.png"); Directory.CreateDirectory(Path.GetDirectoryName(detailPath)!);
        var detailEncoder = new PngBitmapEncoder(); detailEncoder.Frames.Add(BitmapFrame.Create(detailSource));
        using (var stream = File.Create(detailPath)) detailEncoder.Save(stream);
        foreach (var edge in new[] { 3600, 4800 })
        {
            var highQuality = await Task.Run(() => previewService.Load(detailPath, default, edge));
            Check(highQuality.Image?.PixelWidth == edge && highQuality.Image.PixelHeight == edge * detailHeight / detailWidth, $"Preview decodes actual {edge}px detail beyond the old 2400px limit");
        }
        var originalDetail = await Task.Run(() => previewService.Load(detailPath, default, 0));
        Check(originalDetail.Image is { PixelWidth: detailWidth, PixelHeight: detailHeight, IsFrozen: true }, "Original preview retains the complete source resolution");
        var actualPixels = new byte[detailPixels.Length]; new FormatConvertedBitmap(originalDetail.Image!, PixelFormats.Bgr24, null, 0).CopyPixels(actualPixels, detailWidth * 3, 0);
        Check(actualPixels.SequenceEqual(detailPixels), "Original preview preserves every pixel of a lossless source without resampling");
        Check(previewService.Load(previewPath, default, 4800).Image?.PixelWidth == 32, "High preview quality does not enlarge a low-resolution source during decoding");
        Check(previewService.Load(portraitPath, default, 0).Image is { PixelWidth: 24, PixelHeight: 32 }, "Original quality preserves EXIF orientation");
        var detailHistogram = new HistogramService().Calculate(originalDetail.Image!, default);
        Check(detailHistogram.Chart.IsFrozen && detailHistogram.AverageLuminance is > 0 and < 1, "Full-resolution histogram works with bounded row buffers");
        using (var detailVm = new ReviewViewModel(new FakeDialogs(), new ReviewCatalogService(Path.Combine(_root, "detail-catalog.json")), new ReviewSessionService(Path.Combine(_root, "detail-session.json")), new ReviewPreferencesService(Path.Combine(_root, "detail-preferences.json"))))
        {
            detailVm.PreviewMaxEdge = 1800;
            await detailVm.ImportAsync(Path.GetDirectoryName(detailPath)!);
            await WaitUntil(() => !detailVm.IsPreviewLoading, "initial detail preview");
            Check(detailVm.PreviewImage?.PixelWidth == 1800, "Review initially respects its selected preview quality");
            detailVm.PreviewMaxEdge = 4800;
            await WaitUntil(() => !detailVm.IsPreviewLoading, "quality change");
            Check(detailVm.PreviewImage?.PixelWidth == 4800 && detailVm.CurrentPhoto?.Thumbnail?.PixelWidth <= 240, "Changing quality immediately reloads the current photo without retaining it as a catalog thumbnail");
            detailVm.ViewMode = 1; detailVm.ZoomTo(2, 100, 50);
            await WaitUntil(() => !detailVm.IsPreviewLoading, "original zoom preview");
            Check(detailVm.PreviewImage?.PixelWidth == detailWidth && detailVm.PanX == -100 && detailVm.PanY == -50, "Loupe zoom loads full-resolution detail and preserves the cursor anchor");
            detailVm.ResetZoom();
            await WaitUntil(() => !detailVm.IsPreviewLoading, "return to Fit");
            Check(detailVm.PreviewImage?.PixelWidth == 4800 && detailVm.ZoomLabel == "Fit", "Fit restores the chosen preview quality and is labelled accurately");
            detailVm.FullResolutionOnZoom = false; detailVm.ZoomTo(2, 0, 0);
            await WaitUntil(() => !detailVm.IsPreviewLoading, "fixed quality zoom");
            Check(detailVm.PreviewImage?.PixelWidth == 4800, "Full-resolution loading on zoom can be disabled");
            detailVm.PreviewMaxEdge = 0;
            await WaitUntil(() => !detailVm.IsPreviewLoading, "explicit Original quality");
            Check(detailVm.PreviewImage?.PixelWidth == detailWidth, "Explicit Original quality works independently of automatic zoom loading");
            detailVm.FilterIndex = 7;
            await detailVm.ImportAsync(Path.GetDirectoryName(detailPath)!);
            Check(detailVm.FilterIndex == 0 && detailVm.FilteredPhotos.Count == 1 && detailVm.CurrentPhoto != null, "A new folder import clears a stale color filter so imported photos are visible");
            detailVm.FilterIndex = 4;
            await detailVm.ImportPathsAsync([detailPath]);
            Check(detailVm.FilterIndex == 0 && detailVm.CurrentPhoto != null, "Dropped files also clear a stale rating filter");
            detailVm.PreviewMaxEdge = 1200; detailVm.PreviewMaxEdge = 0;
            await detailVm.ImportPathsAsync([previewPath]);
            await WaitUntil(() => !detailVm.IsPreviewLoading, "navigation during decoding");
            Check(detailVm.CurrentPhoto?.Name == "IMG_700.JPG" && detailVm.PreviewImage?.PixelWidth == 32, "A completed older decode cannot replace the current photo during rapid quality changes and navigation");
        }
        var savedDetailPreferences = new ReviewPreferencesService(Path.Combine(_root, "detail-preferences.json")).Load();
        Check(savedDetailPreferences.PreviewMaxEdge == 0 && !savedDetailPreferences.FullResolutionOnZoom, "Original quality and automatic zoom preference survive saving");
        var legacyPreferencesPath = Write("legacy-preview-preferences.json", "{\"PreviewMaxEdge\":1800}");
        using (var legacyVm = new ReviewViewModel(new FakeDialogs(), new ReviewCatalogService(Path.Combine(_root, "legacy-catalog.json")), new ReviewSessionService(Path.Combine(_root, "legacy-session.json")), new ReviewPreferencesService(legacyPreferencesPath)))
            Check(legacyVm.PreviewMaxEdge == 1800 && legacyVm.FullResolutionOnZoom, "Existing quality preferences are kept while enabling original resolution on zoom");
        var reviewRoot = Path.Combine(_root, "review"); Directory.CreateDirectory(reviewRoot);
        File.Copy(previewPath, Path.Combine(reviewRoot, "IMG_10.JPG"));
        File.Copy(previewPath, Path.Combine(reviewRoot, "IMG_2.JPG"));
        File.Copy(previewPath, Path.Combine(reviewRoot, "IMG_1.JPG"));
        Write("review/notes.txt", "ignored");
        Directory.CreateDirectory(Path.Combine(reviewRoot, "nested")); File.Copy(previewPath, Path.Combine(reviewRoot, "nested", "IMG_3.JPG"));
        var imported = new ReviewImportService().Import(reviewRoot, true, null, default);
        Check(imported.Photos.Select(p => p.Name).Take(3).SequenceEqual(new[] { "IMG_1.JPG", "IMG_2.JPG", "IMG_10.JPG" }) && imported.Photos.Count == 4, "Review import supports nested images and natural filename ordering");
        Check(new ReviewImportService().Import(reviewRoot, false, null, default).Photos.Count == 3, "Review import can exclude subfolders");
        var droppedFiles = new ReviewImportService().ImportPaths([Path.Combine(reviewRoot, "IMG_10.JPG"), Path.Combine(reviewRoot, "IMG_2.JPG")], true, null, default);
        Check(droppedFiles.Photos.Select(p => p.Name).SequenceEqual(new[] { "IMG_2.JPG", "IMG_10.JPG" }), "Review accepts multiple dropped image files");
        var mixedDrop = new ReviewImportService().ImportPaths([reviewRoot, Path.Combine(reviewRoot, "IMG_2.JPG")], true, null, default);
        Check(mixedDrop.Photos.Count == 4, "Dropping a folder and one of its photos does not duplicate the image");
        var catalogPath = Path.Combine(_root, "review-catalog.json");
        var reviewDialogs = new FakeDialogs();
        reviewDialogs.ReviewSources = [reviewRoot];
        using (var pickerReview = new ReviewViewModel(reviewDialogs, new ReviewCatalogService(Path.Combine(_root, "picker-catalog.json")), new ReviewSessionService(Path.Combine(_root, "picker-session.json"))))
        {
            pickerReview.ImportCommand.Execute(null);
            var pickerDeadline = DateTime.UtcNow.AddSeconds(5);
            while ((pickerReview.Busy || pickerReview.Photos.Count == 0) && DateTime.UtcNow < pickerDeadline) await Task.Delay(20);
            Check(pickerReview.Photos.Count == 4 && pickerReview.Folder == Path.GetFullPath(reviewRoot), "The single import button imports the folder selected in the visual image browser");
        }
        var sessionPath = Path.Combine(_root, "review-session.json");
        using var reviewVm = new ReviewViewModel(reviewDialogs, new ReviewCatalogService(catalogPath), new ReviewSessionService(sessionPath));
        await reviewVm.ImportAsync(reviewRoot);
        reviewVm.CurrentPhoto = reviewVm.Photos.Single(p => p.Name == "IMG_2.JPG");
        reviewVm.SetRating(4); reviewVm.SetFlag(ReviewFlag.Pick); reviewVm.SetColor(ReviewColor.Red);
        Check(reviewVm.CurrentPhoto.Rating == 4 && reviewVm.PickCount == 1 && reviewVm.ColorCount == 1 && File.Exists(catalogPath), "Review shortcuts store rating, Pick and color label immediately");
        reviewVm.FilterIndex = 1;
        Check(reviewVm.FilteredPhotos.Count == 1 && reviewVm.FilteredPhotos[0].Name == "IMG_2.JPG", "Review filter shows Pick photos");
        reviewVm.FilterIndex = 3;
        Check(reviewVm.FilteredPhotos.Count == 1, "Review filter applies minimum star rating");
        reviewVm.FilterIndex = 7;
        Check(reviewVm.FilteredPhotos.Count == 1 && reviewVm.FilteredPhotos[0].ColorLabel == ReviewColor.Red, "Review filter selects a color label");
        Check(reviewVm.VisibleScopeSummary.Contains("1/4") && reviewVm.SendVisibleLabel.Contains("1") && reviewVm.CopyVisibleRatedLabel.Contains("1"), "Delivery actions state the visible filter scope and rated-photo count");
        reviewVm.FilterIndex = 5;
        Check(reviewVm.HasNoFilteredPhotos && reviewVm.FilteredPhotos.Count == 0, "An empty filter result exposes a recoverable empty state");
        reviewVm.ShowAllCommand.Execute(null);
        Check(reviewVm.FilterIndex == 0 && reviewVm.FilteredPhotos.Count == 4 && !reviewVm.HasNoFilteredPhotos, "Show all photos recovers from an empty filter without changing ratings");
        reviewVm.FilterIndex = 0; reviewVm.CurrentPhoto = reviewVm.FilteredPhotos[0]; reviewVm.MoveGrid(3);
        Check(reviewVm.CurrentPhoto == reviewVm.FilteredPhotos[3], "Grid up/down navigation moves by a complete thumbnail row");
        reviewVm.FilterIndex = 7;
        reviewVm.CopyNamesCommand.Execute(null);
        Check(reviewDialogs.ClipboardText == "IMG_2", "Review copies filtered photo names without extensions");
        var transferList = reviewVm.CreateTransferList(reviewVm.FilteredPhotos)!;
        Check(File.ReadAllLines(transferList).SequenceEqual(new[] { "IMG_2" }), "Review creates an extension-free TXT list that can drive RAW filtering");
        reviewDialogs.Confirm = true;
        var ratedExport = Path.Combine(_root, "rated-export");
        await reviewVm.ExportRatedAsync(ratedExport);
        Check(File.Exists(Path.Combine(ratedExport, "IMG_2.JPG")), "Review exports rated photos with the protected copy service");
        var fiveStar = reviewVm.Photos.Single(photo => photo.Name == "IMG_1.JPG");
        reviewVm.SetRating(fiveStar == null ? [] : [fiveStar], 5); reviewVm.FilterIndex = 4;
        var fiveStarExport = Path.Combine(_root, "five-star-export");
        await reviewVm.ExportRatedAsync(fiveStarExport);
        Check(File.Exists(Path.Combine(fiveStarExport, "IMG_1.JPG")) && !File.Exists(Path.Combine(fiveStarExport, "IMG_2.JPG")), "Rated export respects the active 5-star filter");
        var batchRated = reviewVm.Photos.Where(photo => photo.Name != "IMG_2.JPG").Take(2).ToArray();
        reviewVm.SetRating(batchRated, 3); reviewVm.SetColor(batchRated, ReviewColor.Blue);
        Check(batchRated.All(photo => photo.Rating == 3 && photo.ColorLabel == ReviewColor.Blue), "A multi-selection receives rating and color in one batch");
        reviewVm.SetColor(batchRated, ReviewColor.None);
        Check(batchRated.All(photo => photo.ColorLabel == ReviewColor.None), "The asterisk action can clear color labels from a multi-selection");
        Check(reviewVm.OverlayVisible && reviewVm.OverlayMessage.Contains("COLOR LABEL CLEARED"), "Review actions show immediate on-image feedback");
        reviewVm.Undo();
        Check(batchRated.All(photo => photo.ColorLabel == ReviewColor.Blue), "Undo restores a color change for the complete multi-selection");
        reviewVm.SetColor(batchRated, ReviewColor.None);
        var rotatedPhoto = reviewVm.Photos.Single(photo => photo.Name == "IMG_2.JPG");
        reviewVm.Rotate([rotatedPhoto], 90); reviewVm.Rotate([rotatedPhoto], -90); reviewVm.Rotate([rotatedPhoto], 90);
        Check(rotatedPhoto.Rotation == 90, "Review rotation normalizes left and right 90-degree turns");
        reviewVm.ResetZoom(); reviewVm.ZoomBy(1);
        Check(Math.Abs(reviewVm.Zoom - 1.25) < 0.001, "Loupe zoom increases in controlled steps");
        reviewVm.ResetZoom(); reviewVm.ZoomTo(2, 200, -100);
        Check(Math.Abs(reviewVm.PanX + 200) < 0.001 && Math.Abs(reviewVm.PanY - 100) < 0.001, "Cursor-centered zoom keeps the pointed image location under the pointer");
        reviewVm.PanTo(120, -80);
        Check(reviewVm.PanX == 120 && reviewVm.PanY == -80, "A zoomed Loupe image can be panned horizontally and vertically");
        reviewVm.ResetZoom();
        Check(Math.Abs(reviewVm.Zoom - 1) < 0.001 && reviewVm.PanX == 0 && reviewVm.PanY == 0, "Reset zoom returns to 100 percent and centers the image");
        var selectedExport = Path.Combine(_root, "selected-export");
        await reviewVm.ExportSelectedAsync(batchRated, selectedExport);
        Check(batchRated.All(photo => File.Exists(Path.Combine(selectedExport, photo.Name))), "Context export copies exactly the selected photo group");
        reviewVm.FilterIndex = 7; reviewVm.ViewMode = 1; reviewVm.CurrentPhoto = reviewVm.FilteredPhotos.Single(p => p.Name == "IMG_2.JPG");
        reviewVm.SaveSessionNow();
        using var restoredSession = new ReviewViewModel(reviewDialogs, new ReviewCatalogService(catalogPath), new ReviewSessionService(sessionPath));
        await restoredSession.RestoreSessionAsync();
        Check(restoredSession.Photos.Count == 4 && restoredSession.FilterIndex == 7 && restoredSession.ViewMode == 1 && restoredSession.CurrentPhoto?.Name == "IMG_2.JPG", "Review session restores imported sources, filter, view mode and current photo");
        var preferencesPath = Path.Combine(_root, "custom-review-preferences.json");
        using (var preferencesVm = new ReviewViewModel(reviewDialogs, new ReviewCatalogService(Path.Combine(_root, "preferences-catalog.json")), new ReviewSessionService(Path.Combine(_root, "preferences-session.json")), new ReviewPreferencesService(preferencesPath)))
        {
            preferencesVm.PreviewMaxEdge = 2400; preferencesVm.ZoomStepPercent = 50; preferencesVm.ClickZoomPercent = 300; preferencesVm.OverlayPosition = 1;
            preferencesVm.HelpShortcut = "Ctrl+H"; preferencesVm.ZenShortcut = "F"; preferencesVm.ResetZoomShortcut = "Ctrl+0";
        }
        using (var restoredPreferences = new ReviewViewModel(reviewDialogs, new ReviewCatalogService(Path.Combine(_root, "preferences-catalog-2.json")), new ReviewSessionService(Path.Combine(_root, "preferences-session-2.json")), new ReviewPreferencesService(preferencesPath)))
            Check(restoredPreferences.PreviewMaxEdge == 2400 && restoredPreferences.ZoomStepPercent == 50 && restoredPreferences.ClickZoomPercent == 300 && restoredPreferences.OverlayPosition == 1 && restoredPreferences.HelpShortcut == "Ctrl+H" && restoredPreferences.ZenShortcut == "F" && restoredPreferences.ResetZoomShortcut == "Ctrl+0", "Custom review display and shortcut preferences persist");
        var clearSessionPath = Path.Combine(_root, "clear-session.json");
        var clearCatalogPath = Path.Combine(_root, "clear-catalog.json");
        var clearDialogs = new FakeDialogs { ConfirmClearSession = true };
        using (var clearReview = new ReviewViewModel(clearDialogs, new ReviewCatalogService(clearCatalogPath), new ReviewSessionService(clearSessionPath)))
        {
            await clearReview.ImportAsync(reviewRoot); clearReview.SetRating(3); clearReview.SaveSessionNow();
            clearReview.ClearSessionCommand.Execute(null);
            Check(clearReview.Photos.Count == 0 && !File.Exists(clearSessionPath) && File.Exists(clearCatalogPath), "Clearing the current session closes the catalog but preserves ratings metadata");
        }
        using var reopenedReview = new ReviewViewModel(reviewDialogs, new ReviewCatalogService(catalogPath), new ReviewSessionService(Path.Combine(_root, "reopened-session.json")));
        await reopenedReview.ImportAsync(reviewRoot);
        var persisted = reopenedReview.Photos.Single(p => p.Name == "IMG_2.JPG");
        Check(persisted.Rating == 4 && persisted.Flag == ReviewFlag.Pick && persisted.ColorLabel == ReviewColor.Red && persisted.Rotation == 90, "Ratings, flags, color labels and display rotation persist across review sessions");
        File.AppendAllText(persisted.FullPath, "changed");
        using var changedReview = new ReviewViewModel(reviewDialogs, new ReviewCatalogService(catalogPath), new ReviewSessionService(Path.Combine(_root, "changed-session.json")));
        await changedReview.ImportAsync(reviewRoot);
        Check(changedReview.Photos.Single(p => p.Name == "IMG_2.JPG").Rating == 0, "Changed image does not inherit a stale catalog rating");
        vm.DarkMode = true; vm.SaveSettings();
        Check(((SolidColorBrush)Application.Current.Resources["Surface"]).Color != Colors.White, "Dark mode changes application surface resources");
        vm.DarkMode = false;
        var filterSettingsPath = Path.Combine(_root, "filter-session-settings.json");
        var guidedFilter = new MainViewModel(dialogs);
        Check(guidedFilter.WorkflowHint.StartsWith("Step 1/4"), "TXT Filter identifies the first required workflow step");
        guidedFilter.TxtPath = list;
        Check(guidedFilter.WorkflowHint.StartsWith("Step 2/4"), "TXT Filter advances its workflow hint after selecting a list");
        guidedFilter.SourceFolder = source;
        Check(guidedFilter.WorkflowHint.StartsWith("Step 3/4"), "TXT Filter advances its workflow hint after selecting a source folder");
        var filterSession = new MainViewModel(dialogs, new SettingsService(filterSettingsPath))
        {
            TxtPath = list,
            SourceFolder = source,
            OutputFolder = Path.Combine(_root, "old-output"),
            SearchText = "IMG"
        };
        filterSession.ClearSession();
        var clearedFilterSettings = new SettingsService(filterSettingsPath).Load();
        Check(filterSession.TxtPath.Length == 0 && filterSession.SourceFolder.Length == 0 && filterSession.OutputFolder.Length == 0 && filterSession.HasNoResults && clearedFilterSettings?.TxtPath.Length == 0 && clearedFilterSettings.SourceFolder.Length == 0, "Clearing the TXT Filter session removes saved paths and current results");
        var screenshot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../artifacts/screenshots"));
        Directory.CreateDirectory(screenshot);
        var window = new MainWindow { DataContext = vm, Width = 1240, Height = 1080 };
        Check(window.Icon != null && Application.GetResourceStream(new Uri("pack://application:,,,/PhotoFileFilter;component/Assets/qiqistudio_nobackground.png")) != null, "QiQi Studio icon and original logo are embedded resources");
        ((TxtFilterView)window.Content).ShowHelpPopup();
        Check(((FrameworkElement)((TxtFilterView)window.Content).FindName("FilterHelpOverlay")).Visibility == Visibility.Visible, "TXT Filter has its own in-window workflow guide");
        await Render(window, Path.Combine(screenshot, "filter-help.png"));
        ((TxtFilterView)window.Content).HideHelpPopup();
        await Render(window, Path.Combine(screenshot, "results.png"));
        window.Width = 1280; window.Height = 940;
        await Render(window, Path.Combine(screenshot, "default.png"));
        vm.DarkMode = true;
        await Render(window, Path.Combine(screenshot, "dark-results.png"));
        Check(((SolidColorBrush)((System.Windows.Controls.DataGrid)((TxtFilterView)window.Content).FindName("ResultsGrid")).RowBackground).Color == ((SolidColorBrush)Application.Current.Resources["Surface"]).Color, "Normal result rows use the dark surface as well as alternating rows");
        vm.DarkMode = false;
        ((FrameworkElement)((TxtFilterView)window.Content).FindName("OutputSettings")).BringIntoView();
        await Render(window, Path.Combine(screenshot, "output-settings.png"));
        vm.Comma = false;
        Check(vm.MatchedCount == 0 && !vm.CopyCommand.CanExecute(null), "Changing scan settings invalidates previous results");
        window.DataContext = new MainViewModel(dialogs);
        await Render(window, Path.Combine(screenshot, "empty.png"));
        window.Width = 1040; window.Height = 760;
        await Render(window, Path.Combine(screenshot, "compact.png"));
        window.Width = 960; window.Height = 620;
        await Render(window, Path.Combine(screenshot, "laptop.png"));
        ((MainViewModel)window.DataContext).DarkMode = true;
        await Render(window, Path.Combine(screenshot, "dark-laptop.png"));
        window.Width = 1280; window.Height = 940;
        await Render(window, Path.Combine(screenshot, "dark.png"));
        ThemeService.Apply(false);
        var previewInfo = new FileInfo(portraitPath);
        var previewWindow = new PreviewWindow(new PhotoFile(portraitPath, "portrait.jpg", "portrait.jpg", previewInfo.Length, previewInfo.LastWriteTimeUtc))
        { WindowStartupLocation = WindowStartupLocation.Manual, Left = -20000, Top = -20000, ShowInTaskbar = false, ShowActivated = false };
        previewWindow.Show();
        var previewDeadline = DateTime.UtcNow.AddSeconds(5);
        while (((System.Windows.Controls.Image)previewWindow.FindName("PreviewImage")).Source == null && DateTime.UtcNow < previewDeadline) await Task.Delay(30);
        Check(((System.Windows.Controls.Image)previewWindow.FindName("PreviewImage")).Source != null, "Preview window loads and displays its image asynchronously");
        await Render(previewWindow, Path.Combine(screenshot, "preview.png"));
        previewWindow.Close();
        reviewVm.FilterIndex = 0;
        var reviewWindow = new ReviewWindow(reviewVm) { WindowStartupLocation = WindowStartupLocation.Manual, Left = -20000, Top = -20000, Width = 1360, Height = 820, ShowInTaskbar = false, ShowActivated = false };
        Check(((System.Windows.Controls.ListBox)reviewWindow.FindName("GridPhotos")).SelectionMode == System.Windows.Controls.SelectionMode.Extended, "Grid supports Ctrl-click and Shift-click multi-selection");
        typeof(ReviewWindow).GetMethod("OnToggleZen", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(reviewWindow, [reviewWindow, new RoutedEventArgs()]);
        Check(((System.Windows.Controls.ColumnDefinition)reviewWindow.FindName("NavigatorColumn")).Width.Value == 0 && ((System.Windows.Controls.ColumnDefinition)reviewWindow.FindName("RatingColumn")).Width.Value == 0, "Zen mode hides both review sidebars");
        typeof(ReviewWindow).GetMethod("OnToggleZen", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(reviewWindow, [reviewWindow, new RoutedEventArgs()]);
        Check(((System.Windows.Controls.ColumnDefinition)reviewWindow.FindName("NavigatorColumn")).Width.Value > 0 && ((System.Windows.Controls.ColumnDefinition)reviewWindow.FindName("RatingColumn")).Width.Value > 0, "Zen mode restores both review sidebars");
        await Render(reviewWindow, Path.Combine(screenshot, "review-grid.png"));
        reviewWindow.ShowHelpPopup();
        Check(((FrameworkElement)reviewWindow.FindName("HelpOverlay")).Visibility == Visibility.Visible, "F1 help is available as an in-window popup");
        await Render(reviewWindow, Path.Combine(screenshot, "review-help.png"));
        typeof(ReviewWindow).GetMethod("HideHelpPopup", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(reviewWindow, null);
        typeof(ReviewWindow).GetMethod("OnShowSettings", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(reviewWindow, [reviewWindow, new RoutedEventArgs()]);
        Check(((FrameworkElement)reviewWindow.FindName("SettingsOverlay")).Visibility == Visibility.Visible, "Review settings open as an in-window popup");
        var qualityCombo = (System.Windows.Controls.ComboBox)reviewWindow.FindName("PreviewQualityCombo");
        var previousQuality = reviewVm.PreviewMaxEdge;
        qualityCombo.SelectedValue = 0;
        Check(reviewVm.PreviewMaxEdge == 0 && qualityCombo.SelectedItem is PreviewQualityOption { MaxEdge: 0 }, "The quality selector applies Original to the review view model");
        qualityCombo.SelectedValue = previousQuality;
        await Render(reviewWindow, Path.Combine(screenshot, "review-settings.png"));
        typeof(ReviewWindow).GetMethod("HideSettingsPopup", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(reviewWindow, null);
        reviewVm.ViewMode = 1;
        await Render(reviewWindow, Path.Combine(screenshot, "review-loupe.png"));
        typeof(ReviewWindow).GetMethod("OnSendFilteredToFilter", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(reviewWindow, [reviewWindow, new RoutedEventArgs()]);
        var embeddedFilter = (FrameworkElement)((System.Windows.Controls.ContentControl)reviewWindow.FindName("FilterHost")).Content;
        var embeddedVm = (MainViewModel)embeddedFilter.DataContext;
        Check(((FrameworkElement)reviewWindow.FindName("FilterHost")).Visibility == Visibility.Visible && ((FrameworkElement)reviewWindow.FindName("ReviewScreen")).Visibility == Visibility.Collapsed && reviewWindow.Title == $"QiQi Studio · Filter Photos by TXT List · v{typeof(ReviewWindow).Assembly.GetName().Version!.ToString(3)}", "TXT filter switches inside the same application window and retains the build version in its title");
        Check(File.Exists(embeddedVm.TxtPath) && embeddedVm.Extensions.Where(option => option.Selected).All(option => option.Name is "ARW" or "CR2" or "CR3" or "NEF" or "RAF" or "ORF" or "RW2" or "DNG"), "Sending review names fills the TXT input and selects the RAW preset");
        var embeddedFilterWindow = embeddedFilter as TxtFilterView ?? throw new Exception("TXT Filter must be a hosted UserControl.");
        Check(embeddedFilterWindow.InputBindings.Count == 3, "Hosted filter owns its scan, browse and cancel input bindings");
        embeddedFilterWindow.ShowHelpPopup();
        Check(((FrameworkElement)embeddedFilterWindow.FindName("FilterHelpOverlay")).Visibility == Visibility.Visible, "TXT Filter guide works inside the Review window");
        await Render(reviewWindow, Path.Combine(screenshot, "filter-help-embedded.png"));
        embeddedFilterWindow.HideHelpPopup();
        await Render(reviewWindow, Path.Combine(screenshot, "filter-embedded.png"));
        typeof(ReviewWindow).GetMethod("ShowReviewScreen", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(reviewWindow, null);
        Check(((FrameworkElement)reviewWindow.FindName("ReviewScreen")).Visibility == Visibility.Visible && reviewWindow.Title == $"QiQi Studio · Import & Review · v{typeof(ReviewWindow).Assembly.GetName().Version!.ToString(3)}", "Review screen restores in the same window with its versioned title");
        LanguageService.UseForCurrentProcess(LanguageService.Vietnamese);
        Check(LanguageService.Text("Help") == "Hướng dẫn" && LanguageService.Text("Filter Photos by TXT List") == "Lọc ảnh theo danh sách TXT", "Vietnamese language catalog translates both workspaces");
        using var vietnameseVm = new ReviewViewModel(reviewDialogs, new ReviewCatalogService(Path.Combine(_root, "vi-catalog.json")), new ReviewSessionService(Path.Combine(_root, "vi-session.json")), new ReviewPreferencesService(Path.Combine(_root, "vi-preferences.json")));
        Check(vietnameseVm.Filters[0] == "Tất cả ảnh" && vietnameseVm.ExportPolicies[0].StartsWith("Tự đổi tên"), "Vietnamese view model options preserve stable filter indexes");
        var vietnameseReview = new ReviewWindow(vietnameseVm) { Width = 1360, Height = 820 };
        LanguageService.Apply(vietnameseReview);
        Check(((FrameworkElement)vietnameseReview.FindName("ReviewEmptyState")).Visibility == Visibility.Visible && vietnameseReview.Title == $"QiQi Studio · Nhập & Review · v{typeof(ReviewWindow).Assembly.GetName().Version!.ToString(3)}", "Review presents a localized starting action and versioned title before a folder is imported");
        await Render(vietnameseReview, Path.Combine(screenshot, "vietnamese-review.png"));
        vietnameseReview.ShowHelpPopup();
        await Render(vietnameseReview, Path.Combine(screenshot, "vietnamese-review-help.png"));
        typeof(ReviewWindow).GetMethod("HideHelpPopup", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(vietnameseReview, null);
        typeof(ReviewWindow).GetMethod("OnShowSettings", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(vietnameseReview, [vietnameseReview, new RoutedEventArgs()]);
        await Render(vietnameseReview, Path.Combine(screenshot, "vietnamese-settings.png"));
        var vietnameseFilter = new MainWindow { DataContext = new MainViewModel(dialogs), Width = 1280, Height = 940 };
        LanguageService.Apply(vietnameseFilter);
        await Render(vietnameseFilter, Path.Combine(screenshot, "vietnamese-filter.png"));
        ((TxtFilterView)vietnameseFilter.Content).ShowHelpPopup();
        await Render(vietnameseFilter, Path.Combine(screenshot, "vietnamese-filter-help.png"));
        var languageChooser = new LanguageWindow { Width = 560, Height = 390 };
        await Render(languageChooser, Path.Combine(screenshot, "language-first-run.png"));
        LanguageService.UseForCurrentProcess(LanguageService.English);
        Check(dialogs.Errors.Count == 0, "No unexpected UI errors during full workflow");
        Console.WriteLine($"\n{_passed} checks passed. Fixtures: {_root}\nScreenshots: {screenshot}");
        // Keep fixtures for manual reproduction; they contain generated text, never user photos.
    }
    private static async Task Render(Window window, string path)
    {
        var content = (FrameworkElement)window.Content;
        content.DataContext = window.DataContext;
        content.SetValue(System.Windows.Documents.TextElement.FontFamilyProperty, window.FontFamily);
        content.SetValue(System.Windows.Documents.TextElement.FontSizeProperty, window.FontSize);
        content.SetValue(System.Windows.Documents.TextElement.ForegroundProperty, window.Foreground);
        window.Content = null;
        content.Measure(new Size(window.Width, window.Height));
        content.Arrange(new Rect(0, 0, window.Width, window.Height)); content.UpdateLayout();
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        content.UpdateLayout();
        if (window is MainWindow)
        {
        var copyAction = (System.Windows.Controls.Button)((TxtFilterView)content).FindName("CopyAction");
        var point = copyAction.TransformToAncestor(content).Transform(new Point());
        Check(point.Y >= 0 && point.Y + copyAction.ActualHeight <= content.ActualHeight && point.X + copyAction.ActualWidth <= content.ActualWidth, "Copy button stays inside viewport: " + Path.GetFileName(path));
        var ancestor = VisualTreeHelper.GetParent(copyAction);
        while (ancestor != null && ancestor != content)
        {
            if (ancestor is System.Windows.Controls.ScrollViewer) throw new Exception("Copy button must not be in a scrolling container.");
            ancestor = VisualTreeHelper.GetParent(ancestor);
        }
        }
        var bitmap = new RenderTargetBitmap((int)window.Width, (int)window.Height, 96, 96, PixelFormats.Pbgra32);
        var background = new DrawingVisual();
        using (var drawing = background.RenderOpen()) drawing.DrawRectangle(window.Background, null, new Rect(0, 0, window.Width, window.Height));
        bitmap.Render(background);
        bitmap.Render(content);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path); encoder.Save(stream);
        window.Content = content;
        Check(stream.Length > 10000, "WPF layout rendered: " + Path.GetFileName(path));
    }
    private static async Task WaitUntil(Func<bool> ready, string operation)
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (!ready() && DateTime.UtcNow < deadline) await Task.Delay(20);
        if (!ready()) throw new Exception("Timed out waiting for " + operation);
    }
    private sealed class InlineProgress(Action<OperationProgress> action) : IProgress<OperationProgress> { public void Report(OperationProgress value) => action(value); }
    private sealed class FakeDialogs : IDialogService
    {
        public bool Confirm { get; set; }
        public string ReportPath { get; set; } = "";
        public List<string> Errors { get; } = [];
        public string ClipboardText { get; private set; } = "";
        public string RevealedPath { get; private set; } = "";
        public string PreviewedPath { get; private set; } = "";
        public int ConfirmedCount { get; private set; }
        public int SoundCount { get; private set; }
        public string[]? ReviewSources { get; set; }
        public bool ConfirmClearSession { get; set; }
        public string ReviewNameListPath { get; set; } = "";
        public string? SelectTextFile() => null;
        public string[]? SelectReviewSources(string? initialFolder) => ReviewSources;
        public string? SelectFolder(string title) => null;
        public string? SaveReport() => ReportPath;
        public string? SaveReviewNameList(string suggestedName) => string.IsNullOrWhiteSpace(ReviewNameListPath) ? null : ReviewNameListPath;
        public bool ConfirmCopy(int count, string source, string destination, string policy) { ConfirmedCount = count; return Confirm; }
        public bool ConfirmClearReviewSession() => ConfirmClearSession;
        public void ShowError(string message) => Errors.Add(message);
        public void OpenFolder(string path) { }
        public void CopyText(string text) => ClipboardText = text;
        public void RevealFile(string path) => RevealedPath = path;
        public void ShowPreview(PhotoFile file) => PreviewedPath = file.FullPath;
        public void PlayCompletionSound() => SoundCount++;
        public void OpenWindowsThumbnailCleanup() { }
    }
}
