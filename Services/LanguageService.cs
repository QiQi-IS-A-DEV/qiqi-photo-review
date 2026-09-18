using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace PhotoFileFilter.Services;

public static class LanguageService
{
    private sealed record SavedLanguage(string Language);
    public const string English = "en", Vietnamese = "vi";
    public static string FilePath { get; } = Environment.GetEnvironmentVariable("QIQI_LANGUAGE_FILE") is { Length: > 0 } overridePath
        ? Path.GetFullPath(overridePath)
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QiQiStudio", "PhotoFileFilter", "language.json");
    public static string CurrentLanguage { get; private set; } = English;
    public static bool IsVietnamese => CurrentLanguage == Vietnamese;
    public static bool HasSavedLanguage => File.Exists(FilePath);

    private static readonly Dictionary<string, string> Vi = new(StringComparer.Ordinal)
    {
        ["T · Purple"]="T · Tím", ["Purple Label"]="Nhãn tím", ["Purple label"]="Nhãn tím", ["PURPLE LABEL"]="NHÃN TÍM",
        ["Fast · 1,200 px"]="Nhanh · 1.200 px", ["Balanced · 1,800 px"]="Cân bằng · 1.800 px", ["Detailed · 2,400 px"]="Chi tiết · 2.400 px",
        ["For sharper detail: open Settings, choose Preview quality (up to Original), or enable Load original resolution when zooming. Quality changes apply immediately."]="Để soi rõ hơn: mở Cài đặt, chọn Chất lượng preview (tối đa Original), hoặc bật Tải độ phân giải gốc khi zoom. Thay đổi được áp dụng ngay.",
        ["High · 3,600 px"]="Cao · 3.600 px", ["Ultra · 4,800 px"]="Rất cao · 4.800 px", ["Original · full resolution"]="Original · Độ phân giải gốc",
        ["Preview quality"]="Chất lượng preview", ["Load original resolution when zooming"]="Tải độ phân giải gốc khi zoom",
        ["Quality applies immediately to the current photo. Original keeps all decoded pixels and uses more RAM. Only the current photo is loaded at this quality."]="Áp dụng ngay cho ảnh đang xem. Original giữ toàn bộ pixel đã giải mã và dùng nhiều RAM hơn. Chất lượng này chỉ áp dụng cho ảnh đang xem.",
        ["When zooming in Loupe, load full resolution for detail; return to the selected quality at Fit."]="Khi zoom trong Loupe, tải ảnh đầy đủ để soi chi tiết; trở về chất lượng đã chọn khi về Fit.",
        ["RAW detail depends on the Windows codec or available JPG/embedded preview. The actual dimensions and source are shown below the image."]="Chi tiết RAW phụ thuộc codec Windows hoặc ảnh JPG/preview nhúng có sẵn. Kích thước và nguồn preview thực tế được ghi dưới ảnh.",
        ["Loading full-resolution preview…"]="Đang tải preview độ phân giải gốc…", ["Embedded RAW preview"]="Preview nhúng trong RAW", ["RAW via Windows codec"]="RAW qua codec Windows",
        ["The current preview could not be decoded."]="Không thể giải mã preview ảnh này.", ["Reset zoom to Fit"]="Đưa zoom về Fit", ["Zoom is relative to Fit, not a 1:1 pixel scale."]="Mức zoom tính theo Fit, không phải tỷ lệ pixel 1:1.",
        ["Help"]="Hướng dẫn", ["Settings"]="Cài đặt", ["Close"]="Đóng", ["Close Guide"]="Đóng hướng dẫn", ["Restore Defaults"]="Khôi phục mặc định",
        ["QiQi Studio · Import & Review"]="QiQi Studio · Nhập & Review", ["QiQi Studio · Filter Photos by TXT List"]="QiQi Studio · Lọc ảnh theo TXT",
        ["Hide panels"]="Ẩn bảng", ["Show panels"]="Hiện bảng", ["Clear session"]="Xóa phiên", ["Import folder…"]="Nhập thư mục…", ["Cancel import"]="Hủy nhập",
        ["Include subfolders"]="Gồm thư mục con", ["Import & Review  ·  Ctrl+1"]="Nhập & Review  ·  Ctrl+1", ["←  Back to Review  ·  Ctrl+1"]="←  Về Review  ·  Ctrl+1",
        ["Review  ·  Ctrl+1"]="Review  ·  Ctrl+1", ["TXT Filter  ·  Ctrl+2"]="Lọc TXT  ·  Ctrl+2", ["Filter Photos by TXT List"]="Lọc ảnh theo danh sách TXT",
        ["IMPORT & REVIEW"]="NHẬP & REVIEW ẢNH", ["Library"]="Thư viện", ["  |  Review workspace"]="  |  Không gian review", ["  /  Import → Review → Deliver"]="  /  Nhập → Review → Bàn giao", ["Dark theme"]="Giao diện tối",
        ["✓ Originals stay untouched"]="✓ Ảnh gốc được giữ nguyên", ["Filename List"]="Danh sách tên ảnh", ["Source Photos"]="Ảnh nguồn", ["Destination"]="Nơi lưu",
        ["Match Results"]="Kết quả đối chiếu", ["Requested names"]="Tên cần tìm", ["Files found"]="File tìm thấy", ["Names not found"]="Tên không tìm thấy",
        ["FILE TYPES"]="LOẠI FILE", ["SOURCE FOLDER"]="THƯ MỤC NGUỒN", ["SEPARATE FILENAMES BY"]="KÝ TỰ PHÂN CÁCH TÊN", ["WHEN A FILE ALREADY EXISTS"]="KHI FILE ĐÃ TỒN TẠI",
        ["Comma"]="Dấu phẩy", ["Space or tab"]="Khoảng trắng hoặc Tab", ["New line"]="Xuống dòng", ["Ignore file extensions in TXT"]="Bỏ qua phần mở rộng trong TXT",
        ["Search subfolders"]="Tìm trong thư mục con", ["You can also drop a photo folder in this area."]="Bạn cũng có thể kéo thư mục ảnh vào khu vực này.",
        ["TXT · drop here or click to browse"]="TXT · kéo vào đây hoặc bấm để chọn", ["IMG_1001.JPG becomes IMG_1001, so it can match JPG and RAW files with the same name."]="IMG_1001.JPG sẽ thành IMG_1001 để khớp file JPG và RAW cùng tên.",
        ["All"]="Tất cả", ["Scan Photos"]="Quét ảnh", ["Inside source folder"]="Trong thư mục nguồn", ["Different folder"]="Thư mục khác", ["Create subfolder"]="Tạo thư mục con",
        ["Browse…"]="Chọn…", ["↗  Save Report"]="↗  Lưu báo cáo", ["Copy all missing names"]="Sao chép tên chưa tìm thấy", ["Preview (Space)"]="Xem ảnh (Space)",
        ["Show in File Explorer"]="Hiện trong File Explorer", ["Copy full path"]="Sao chép đường dẫn đầy đủ", ["Include all files"]="Chọn tất cả file", ["COPY"]="CHÉP",
        ["NAME / FOLDER"]="TÊN / THƯ MỤC", ["TYPE"]="LOẠI", ["SIZE"]="DUNG LƯỢNG", ["Bring every selected photo into one place."]="Tập hợp các ảnh đã chọn vào cùng một nơi.",
        ["Add a filename list and scan your source folder to begin."]="Thêm danh sách tên ảnh và quét thư mục nguồn để bắt đầu.", ["Destination…"]="Nơi lưu…", ["Open Folder ↗"]="Mở thư mục ↗",
        ["Play sound when finished"]="Phát âm thanh khi xong", ["Cancel"]="Hủy", ["WORKFLOW"]="QUY TRÌNH", ["MATCHING NOTES"]="LƯU Ý ĐỐI CHIẾU", ["SHORTCUTS"]="PHÍM TẮT",
        ["QiQi Studio TXT Filter Guide"]="Hướng dẫn lọc ảnh TXT - QiQi Studio", ["Match a filename list with JPG or RAW originals, then copy the files into one folder."]="Đối chiếu danh sách tên với ảnh JPG hoặc RAW gốc, sau đó sao chép vào một thư mục.",
        ["1. Add the filename list. "]="1. Thêm danh sách tên ảnh. ", ["Choose a TXT file or drop it onto Filename List. The list can contain names such as IMG_1001 or IMG_1002.JPG."]="Chọn file TXT hoặc kéo vào ô Danh sách tên ảnh. Danh sách có thể chứa IMG_1001 hoặc IMG_1002.JPG.",
        ["2. Set the matching rules. "]="2. Thiết lập quy tắc đối chiếu. ", ["Choose how names are separated. Keep Ignore file extensions enabled when a JPG list should find RAW files with the same base name."]="Chọn ký tự phân cách. Bật bỏ qua phần mở rộng khi muốn dùng danh sách JPG để tìm RAW cùng tên.",
        ["3. Choose the source photos. "]="3. Chọn ảnh nguồn. ", ["Select JPG, RAW, All, or individual file types. Choose the folder that contains the originals and enable Search subfolders when needed."]="Chọn JPG, RAW, Tất cả hoặc từng loại file. Chọn thư mục ảnh gốc và bật tìm thư mục con khi cần.",
        ["4. Scan and check the results. "]="4. Quét và kiểm tra kết quả. ", ["Select Scan Photos or press F5. Review Files Found, Not Found, and Notes / Errors. Clear the COPY check box for any file you do not want to copy."]="Bấm Quét ảnh hoặc F5. Kiểm tra File tìm thấy, Không tìm thấy và Ghi chú / Lỗi. Bỏ chọn CHÉP với file không muốn sao chép.",
        ["5. Copy the selected files. "]="5. Sao chép file đã chọn. ", ["Choose the destination and the action for duplicate filenames, then select Copy Photos and confirm the summary."]="Chọn nơi lưu và cách xử lý tên trùng, sau đó bấm Sao chép ảnh và xác nhận.",
        ["Navigator"]="Điều hướng", ["Photo Source"]="Nguồn ảnh", ["CURRENT IMPORT"]="THƯ MỤC ĐANG REVIEW", ["NO PREVIEW"]="CHƯA CÓ PREVIEW", ["NO DATA"]="CHƯA CÓ DỮ LIỆU",
        ["PHOTO"]="ẢNH", ["You can also drag and drop one or more photos, or an entire folder, anywhere in this window."]="Bạn cũng có thể kéo một ảnh, nhiều ảnh hoặc cả thư mục vào cửa sổ này.",
        ["Photo Filter:"]="Lọc ảnh:", ["Photo Filter"]="Lọc ảnh", ["Tab  Hide panels"]="Tab  Ẩn bảng", ["Rating & Flags"]="Rating & Đánh dấu", ["Color Label"]="Nhãn màu", ["P Pick"]="P  Chọn", ["X Reject"]="X  Loại", ["U Clear"]="U  Bỏ cờ",
        ["Rotate & Zoom"]="Xoay & Phóng to", ["Left 90°"]="Trái 90°", ["Right 90°"]="Phải 90°", ["Export & RAW Workflow"]="Xuất ảnh & Quy trình RAW",
        ["Send to TXT Filter"]="Gửi sang Lọc TXT", ["Export filename list…"]="Xuất danh sách tên…", ["Copy filtered photos…"]="Chép ảnh đang lọc…", ["Open export folder"]="Mở thư mục xuất",
        ["Start with a photo folder"]="Bắt đầu với một thư mục ảnh", ["Import a folder or drop photos here. Your originals stay untouched while ratings and labels are saved in the local catalog."]="Nhập một thư mục hoặc kéo ảnh vào đây. Ảnh gốc được giữ nguyên; Rating và nhãn được lưu trong catalog cục bộ.",
        ["Next: review in Grid or Loupe, then export visible names or send them to TXT Filter."]="Tiếp theo: Review bằng Grid hoặc Loupe, rồi xuất tên đang hiện hoặc gửi sang Lọc TXT.",
        ["No photos match this filter"]="Không có ảnh phù hợp bộ lọc", ["Ratings and labels are still saved. Show all photos or choose another Photo Filter."]="Rating và nhãn vẫn được lưu. Hiện tất cả ảnh hoặc chọn bộ lọc khác.", ["Show all photos"]="Hiện tất cả ảnh",
        ["Deliver & RAW Workflow"]="Bàn giao & Quy trình RAW", ["CURRENT FILTER SCOPE"]="PHẠM VI BỘ LỌC HIỆN TẠI", ["COPY RATED PHOTOS IN CURRENT VIEW"]="CHÉP ẢNH ĐÃ RATING ĐANG HIỆN",
        ["Send every filename currently visible after Photo Filter"]="Gửi toàn bộ tên file đang hiện sau khi áp dụng Lọc ảnh",
        ["Review Session"]="Phiên Review", ["Storage & Cache"]="Lưu trữ & Cache", ["Keyboard Shortcuts"]="Phím tắt", ["Show photo in File Explorer"]="Hiện ảnh trong File Explorer",
        ["Release preview memory"]="Giải phóng bộ nhớ preview", ["Clear current review session…"]="Xóa phiên Review hiện tại…", ["Open Windows Disk Cleanup…"]="Mở Windows Disk Cleanup…",
        ["View the full guide · F1"]="Xem hướng dẫn đầy đủ · F1", ["QiQi Studio Import & Review Guide"]="Hướng dẫn Import & Review - QiQi Studio",
        ["FROM JPG REVIEW TO RAW DELIVERY"]="QUY TRÌNH TỪ REVIEW JPG ĐẾN FILE RAW", ["Your original photos and embedded metadata are never modified."]="Ảnh gốc và metadata nhúng luôn được giữ nguyên.",
        ["REVIEW KEYBOARD SHORTCUTS"]="PHÍM TẮT REVIEW", ["MOUSE & MULTI-SELECTION"]="CHUỘT & CHỌN NHIỀU ẢNH", ["Photo Review Settings"]="Cài đặt Review ảnh",
        ["IMAGE DISPLAY"]="HIỂN THỊ ẢNH", ["CUSTOM KEYBOARD SHORTCUTS"]="TÙY CHỈNH PHÍM TẮT", ["Preview resolution"]="Độ phân giải preview",
        ["Mouse click zoom (%)"]="Mức zoom khi bấm chuột (%)", ["Mouse wheel / keyboard zoom step (%)"]="Bước zoom con lăn / bàn phím (%)", ["On-screen message duration (ms)"]="Thời gian hiện thông báo (ms)",
        ["Message position"]="Vị trí thông báo", ["Default view"]="Chế độ xem mặc định", ["Start with both side panels hidden"]="Mở app với hai bảng bên được ẩn",
        ["Show histogram for the current preview"]="Hiện histogram của ảnh đang xem", ["Changes are saved automatically."]="Thay đổi được tự động lưu.", ["Open guide"]="Mở hướng dẫn",
        ["Show/hide side panels"]="Ẩn/hiện bảng bên", ["Undo last review action"]="Hoàn tác thao tác Review gần nhất", ["Reset zoom to 100%"]="Đưa zoom về Fit",
        ["Switch to Grid"]="Chuyển sang Grid", ["Switch to Loupe"]="Chuyển sang Loupe", ["Bottom of photo"]="Dưới ảnh", ["Center of photo"]="Giữa ảnh",
        ["All Photos"]="Tất cả ảnh", ["≥ 1 Star"]="≥ 1 sao", ["≥ 3 Stars"]="≥ 3 sao", ["5 Stars"]="5 sao", ["Unrated"]="Chưa chấm",
        ["Red Label"]="Nhãn đỏ", ["Yellow Label"]="Nhãn vàng", ["Green Label"]="Nhãn xanh lá", ["Blue Label"]="Nhãn xanh dương",
        ["Rename — keep both"]="Tự đổi tên — giữ cả hai", ["Skip existing files"]="Bỏ qua file đã có", ["Replace destination files"]="Thay thế file đích",
        ["No folder imported"]="Chưa nhập thư mục", ["No photo selected"]="Chưa chọn ảnh", ["No histogram data"]="Chưa có dữ liệu histogram",
        ["Select a photo to analyze its tonal range."]="Chọn ảnh để phân tích vùng sáng tối.", ["Loading preview…"]="Đang tải preview…", ["Loading photo…"]="Đang tải ảnh…",
        ["Press Space or Esc to close"]="Nhấn Space hoặc Esc để đóng", ["Show in File Explorer ↗"]="Hiện trong File Explorer ↗", ["Ready"]="Sẵn sàng",
        ["Waiting to scan"]="Đang chờ quét", ["No files selected for copying"]="Chưa chọn file để sao chép", ["Ready to copy"]="Sẵn sàng sao chép",
        ["Choose a TXT filename list and a source photo folder to begin."]="Chọn danh sách tên TXT và thư mục ảnh nguồn để bắt đầu.", ["Choose or drop a .txt file"]="Chọn hoặc kéo file .txt vào đây",
        ["Filename list selected by the client"]="Danh sách tên ảnh khách hàng đã chọn", ["Choose or drop the source photo folder"]="Chọn hoặc kéo thư mục ảnh nguồn",
        ["Results will appear after scanning."]="Kết quả sẽ xuất hiện sau khi quét.", ["Copy Photos  →"]="Sao chép ảnh  →", ["Histogram disabled"]="Histogram đã tắt",
        ["Search by filename or folder…"]="Tìm theo tên file hoặc thư mục…", ["Choose a destination folder."]="Chọn thư mục đích.",
        ["No data"]="Không có dữ liệu", ["Preview memory released."]="Đã giải phóng bộ nhớ preview.", ["Preview memory released. Select a photo to load it again."]="Đã giải phóng bộ nhớ preview. Chọn ảnh để tải lại.",
        ["Photo Preview · QiQi Studio"]="Xem ảnh · QiQi Studio"
        , ["APP LANGUAGE"]="NGÔN NGỮ ỨNG DỤNG", ["Interface language"]="Ngôn ngữ giao diện", ["Vietnamese"]="Tiếng Việt", ["English"]="English"
        , ["The app restarts after you confirm a language change."]="App sẽ khởi động lại sau khi bạn xác nhận đổi ngôn ngữ."
        , ["1. Select Import folder to load JPG, RAW, or other supported photos. You can also drag and drop photos or a folder into this window.\n2. Use Grid for a quick overview. Double-click a photo or press Space to inspect it in Loupe.\n3. Add star ratings, color labels, or Pick/Reject flags. Photo Filter controls which photos remain visible.\n4. Select Send to TXT Filter to pass the visible filenames directly to the matching workspace.\n5. Choose the folder containing your RAW files, confirm the file types, scan, and copy the matching files."]="1. Chọn Nhập thư mục để tải JPG, RAW hoặc định dạng được hỗ trợ. Bạn cũng có thể kéo ảnh hoặc thư mục vào cửa sổ.\n2. Dùng Grid để xem tổng quan. Nhấp đúp ảnh hoặc nhấn Space để xem kỹ trong Loupe.\n3. Chấm sao, gắn nhãn màu hoặc cờ Pick/Reject. Bộ Lọc ảnh quyết định ảnh nào đang hiển thị.\n4. Chọn Gửi sang Lọc TXT để chuyển trực tiếp tên các ảnh đang hiển thị.\n5. Chọn thư mục RAW, kiểm tra loại file, quét và sao chép các file khớp tên."
        , ["Use Ctrl+click to add or remove individual photos, or Shift+click to select a range. Double-click to open Loupe. In Loupe, click once to zoom toward the pointer at your chosen level; click again to return to Fit. Use the mouse wheel for gradual pointer-centered zoom. While zoomed in, drag to inspect hidden areas. Right-click for export, rating, color, flag, and rotation actions."]="Dùng Ctrl+click để thêm hoặc bỏ từng ảnh; Shift+click để chọn một dải ảnh. Nhấp đúp để mở Loupe. Trong Loupe, bấm một lần để zoom vào vị trí con trỏ; bấm lại để về Fit. Dùng con lăn để zoom dần và kéo ảnh khi đã phóng to. Nhấp chuột phải để xuất ảnh, chấm sao, gắn màu, cờ hoặc xoay."
        , ["Ratings, flags, color labels, and rotation are saved locally. Original files remain unchanged."]="Rating, cờ, nhãn màu và góc xoay được lưu trong app. File gốc không bị thay đổi."
        , ["Keys 0–9, *, P/X/U, and the arrow keys stay fixed to prevent command conflicts."]="Các phím 0–9, *, P/X/U và phím mũi tên được giữ cố định để tránh xung đột."
        , ["Ctrl/Shift + click to select multiple  ·  Originals stay untouched"]="Ctrl/Shift + click để chọn nhiều ảnh  ·  Ảnh gốc được giữ nguyên"
        , ["Switch between Import & Review and TXT Filter"]="Chuyển giữa Nhập & Review và Lọc TXT"
        , ["Previous/next photo · move up/down one Grid row"]="Ảnh trước/sau · di chuyển lên/xuống một hàng Grid"
        , ["Set rating · set color label · clear color label"]="Chấm sao · gắn nhãn màu · xóa nhãn màu"
        , ["Pick · Reject · remove flag"]="Pick · Reject · bỏ cờ", ["Undo the most recent review action"]="Hoàn tác thao tác Review gần nhất"
        , ["Rotate left / right by 90°"]="Xoay trái / phải 90°", ["Zoom in / out"]="Phóng to / thu nhỏ"
        , ["Reset to Fit · mouse wheel zooms toward the pointer"]="Về Fit · con lăn zoom theo vị trí con trỏ", ["Show or hide both side panels"]="Ẩn hoặc hiện hai bảng bên"
        , ["Open this guide · press Esc to close"]="Mở hướng dẫn · nhấn Esc để đóng"
        , ["• Matching is not case-sensitive.\n• With extensions ignored, IMG_1001.JPG can match IMG_1001.CR3 or IMG_1001.ARW.\n• The TXT file should contain filenames, not full folder paths.\n• QiQi Studio copies selected files and never modifies the originals."]="• Không phân biệt chữ hoa và chữ thường.\n• Khi bỏ qua phần mở rộng, IMG_1001.JPG có thể khớp IMG_1001.CR3 hoặc IMG_1001.ARW.\n• TXT nên chứa tên file, không phải đường dẫn đầy đủ.\n• QiQi Studio chỉ sao chép file đã chọn và không sửa ảnh gốc."
        , ["Switch between Review and TXT Filter\nChoose a TXT list\nScan the source folder\nPreview the selected result\nCancel an operation / close this guide"]="Chuyển giữa Review và Lọc TXT\nChọn danh sách TXT\nQuét thư mục nguồn\nXem file đang chọn\nHủy thao tác / đóng hướng dẫn"
    };

    public static void Initialize() { try { if (File.Exists(FilePath)) CurrentLanguage = Normalize(JsonSerializer.Deserialize<SavedLanguage>(File.ReadAllText(FilePath))?.Language); } catch { CurrentLanguage = English; } }
    public static void Set(string language) { UseForCurrentProcess(language); Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!); File.WriteAllText(FilePath, JsonSerializer.Serialize(new SavedLanguage(CurrentLanguage), new JsonSerializerOptions { WriteIndented = true })); }
    public static void UseForCurrentProcess(string language) => CurrentLanguage = Normalize(language);
    private static string Normalize(string? value) => string.Equals(value, Vietnamese, StringComparison.OrdinalIgnoreCase) ? Vietnamese : English;
    public static string Text(string value)
    {
        if (!IsVietnamese) return value;
        if (Vi.TryGetValue(value, out var translated)) return translated;
        if (value.StartsWith("Checked ")) return value.Replace("Checked ", "Đã kiểm tra ").Replace(" files…", " file…");
        if (value.StartsWith("Imported ")) return value.Replace("Imported ", "Đã nhập ").Replace(" photos", " ảnh").Replace(" items could not be read", " mục không đọc được");
        if (value.StartsWith("Restored the previous review session")) return value.Replace("Restored the previous review session", "Đã khôi phục phiên Review trước").Replace(" photos", " ảnh");
        if (value.StartsWith("Scanned ")) return value.Replace("Scanned ", "Đã quét ").Replace(" files", " file").Replace("removed", "loại").Replace("duplicate names", "tên trùng");
        if (value.StartsWith("Matched ")) return value.Replace("Matched ", "Khớp ").Replace(" files", " file").Replace(" names not found", " tên không tìm thấy").Replace("Review the results and choose a destination.", "Kiểm tra kết quả và chọn nơi lưu.");
        if (value.StartsWith("Copying ")) return value.Replace("Copying ", "Đang sao chép ").Replace(" photos", " ảnh");
        if (value.StartsWith("Copied ")) return value.Replace("Copied ", "Đã sao chép ").Replace(" photos", " ảnh").Replace("skipped", "bỏ qua").Replace("errors", "lỗi");
        if (value.StartsWith("Exported ")) return value.Replace("Exported ", "Đã xuất ").Replace(" filenames", " tên file");
        if (value.StartsWith("Rotated ")) return value.Replace("Rotated ", "Đã xoay ").Replace(" photos", " ảnh").Replace(" left", " trái").Replace(" right", " phải").Replace("Original files remain unchanged.", "File gốc được giữ nguyên.");
        return value;
    }
    public static string[] Texts(params string[] values) => values.Select(Text).ToArray();

    public static void Apply(DependencyObject root)
    {
        if (!IsVietnamese) return;
        ApplyCore(root, new());
    }
    private static void ApplyCore(DependencyObject node, HashSet<DependencyObject> visited)
    {
        if (!visited.Add(node)) return;
        if (node is Window window) window.Title = Text(window.Title);
        if (node is TextBlock text && text.Inlines.Count <= 1 && !System.Windows.Data.BindingOperations.IsDataBound(text, TextBlock.TextProperty)) text.SetCurrentValue(TextBlock.TextProperty, Text(text.Text));
        if (node is Run run && !System.Windows.Data.BindingOperations.IsDataBound(run, Run.TextProperty)) run.SetCurrentValue(Run.TextProperty, Text(run.Text));
        if (node is ContentControl content && content.Content is string value) content.SetCurrentValue(ContentControl.ContentProperty, Text(value));
        if (node is HeaderedContentControl header && header.Header is string title) header.SetCurrentValue(HeaderedContentControl.HeaderProperty, Text(title));
        if (node is HeaderedItemsControl itemsHeader && itemsHeader.Header is string itemsTitle) itemsHeader.SetCurrentValue(HeaderedItemsControl.HeaderProperty, Text(itemsTitle));
        if (node is FrameworkElement element) { if (element.ToolTip is string tip) element.ToolTip = Text(tip); if (element.ContextMenu is { } menu) ApplyCore(menu, visited); }
        if (node is FrameworkElement tagged && tagged.Tag is string tag && Vi.ContainsKey(tag)) tagged.Tag = Text(tag);
        foreach (var child in LogicalTreeHelper.GetChildren(node).OfType<DependencyObject>().ToArray()) ApplyCore(child, visited);
        if (node is Visual or System.Windows.Media.Media3D.Visual3D) for (var i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++) ApplyCore(VisualTreeHelper.GetChild(node, i), visited);
    }
    public static void Restart() { if (Environment.ProcessPath is { Length: > 0 } path) Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); Application.Current.Shutdown(); }
}
