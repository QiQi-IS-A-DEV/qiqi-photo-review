# Photo File Filter

Ứng dụng desktop Windows dùng **C# + WPF + .NET 8**, giao diện tiếng Việt. Đọc danh sách TXT, tìm ảnh theo tên và sao chép đến thư mục được chọn. Hoạt động tại máy, không cần tài khoản, backend, cơ sở dữ liệu hay Internet khi sử dụng.

## Đánh giá tính khả thi

Phạm vi này phù hợp với WPF: hộp thoại chọn file/thư mục, bảng kết quả, thao tác ổ đĩa bất đồng bộ, tiến độ và workspace review dùng phím tắt. Luồng lọc/copy chỉ đối chiếu tên nên không cần giải mã RAW. Workspace review dùng thumbnail do Windows cung cấp và JPG cùng tên làm phương án dự phòng. Chi phí quét gần O(n + m), với n là số tên yêu cầu và m là số file được duyệt; tốc độ sao chép phụ thuộc dung lượng ảnh, ổ đĩa và kết nối mạng.

## Chạy ứng dụng

- Bản portable: `artifacts/publish/win-x64/PhotoFileFilter.exe`. Bản này kèm .NET runtime, dùng cho Windows x64.
- Từ source: mở `PhotoFileFilter.sln` trong Visual Studio hỗ trợ .NET 8/WPF, chọn PhotoFileFilter làm startup project, bấm F5.
- Hoặc chạy `dotnet run --project PhotoFileFilter.csproj` với .NET SDK 8 trở lên. Target của cả ứng dụng và Core là .NET 8.

## Mới trong bản 1.16 — English interface and clearer copy

- All user-facing text in Import & Review and TXT Filter now uses consistent, professional English.
- The in-app guide now explains the complete JPG review to RAW matching workflow, multi-selection, mouse controls, and keyboard shortcuts.
- Long folder paths, photo details, status text, and help content now wrap, scroll, or expose a tooltip instead of disappearing at narrow sizes.
- Labels and actions were renamed for clarity while preserving the existing workflow and shortcuts.

## Bản 1.15 — Histogram và workspace tinh gọn

- OSD được chuyển xuống đáy ảnh để không che chủ thể.
- Navigator, Histogram, Rating, Transform, Workflow, Catalog, Cache và Phím tắt được gom thành panel có thể thu gọn theo phong cách workspace ảnh chuyên nghiệp.
- Histogram RGB phân tích preview, hiển thị tỷ lệ vùng gần cháy sáng, vùng tối bị bít và độ sáng trung bình.
- Popup **Cài đặt Review** cho phép chọn độ phân giải preview, bước zoom, OSD, chế độ mở, Histogram và nhóm phím tắt chính.
- Cả phím `` ` `` và `Ctrl+0` mặc định đều đưa zoom về 100%; cấu hình có thể đổi trong Cài đặt.

## Bản 1.14 — Zen Mode, phản hồi nhanh và hoàn tác

- Nút **Hướng dẫn** và phím `F1` mở popup hướng dẫn ngay trong cửa sổ hiện tại; `Esc` hoặc `F1` đóng popup.
- Phím `Tab` ẩn/hiện Navigator và Rating để dành toàn bộ chiều ngang cho ảnh trên laptop.
- Rating, Pick/Reject, nhãn màu và xoay ảnh hiện OSD ngay giữa vùng ảnh.
- `Ctrl + Z` hoàn tác thao tác rating, cờ, màu hoặc xoay gần nhất, bao gồm thao tác trên nhiều ảnh.
- Khi màn hình lọc TXT được nhúng trong Review, giao diện tối được giữ cố định để hai phần có cùng phong cách.

## Bản 1.13 — kéo ảnh khi zoom

- Trong Loupe, sau khi zoom lớn hơn 100%, giữ chuột trái và kéo để di chuyển ảnh theo cả chiều ngang lẫn chiều dọc.
- Phím `` ` ``, chuyển ảnh hoặc thu về 100% sẽ đưa ảnh lại chính giữa.

## Bản 1.12 — xoay, zoom và menu rõ hơn

- `Ctrl + [` / `Ctrl + ]` xoay trái/phải 90° cho ảnh hoặc nhóm ảnh đang chọn; góc xoay lưu trong catalog nhưng không sửa ảnh gốc.
- `Ctrl + +` / `Ctrl + -` và con lăn chuột zoom trong Loupe; phím `` ` `` hoặc chuyển ảnh sẽ trở về 100%.
- Phím `*` xóa nhãn màu của ảnh hoặc nhóm ảnh đang chọn.
- Menu chuột phải dùng nền và màu chữ cố định để luôn đọc rõ trên ảnh sáng.

## Bản 1.11 — chuyển lựa chọn JPG sang quy trình RAW

- Xuất tên ảnh đang hiện theo Library Filter ra TXT, không kèm phần mở rộng, để một tên JPG có thể khớp file RAW cùng tên.
- **Gửi sang lọc TXT** tự tạo danh sách, chuyển thẳng sang màn hình lọc và bật preset RAW; menu chuột phải hỗ trợ cùng thao tác cho nhóm ảnh đang chọn.
- Có thể giải phóng preview trong bộ nhớ, xóa phiên review hiện tại nhưng giữ rating/nhãn, hoặc mở Disk Cleanup để dọn thumbnail cache dùng chung của Windows.

## Bản 1.10 — import trực quan và tự khôi phục phiên

- Chỉ còn một nút **Import thư mục…**. Hộp Explorer hiển thị ảnh để xác nhận đúng thư mục, sau đó app import toàn bộ thư mục hiện tại.
- Kéo-thả một hoặc nhiều ảnh lẻ, hoặc kéo-thả cả thư mục, vẫn được hỗ trợ trong màn hình review.
- Phiên review được tự lưu và khôi phục khi mở lại app: nguồn đã import, ảnh đang xem, bộ lọc, Grid/Loupe, tùy chọn thư mục con và chính sách export.
- Dữ liệu phiên nằm tại `%LOCALAPPDATA%\QiQiStudio\PhotoFileFilter\review-session.json`.

## Bản 1.9 — hộp import Windows có ảnh

- **Import thư mục…** trở lại hộp Explorer quen thuộc của Windows với thanh địa chỉ, ổ đĩa, thư mục và tìm kiếm. Hộp thoại hiển thị các file ảnh trong folder để kiểm tra trực quan trước khi import folder hiện tại.
- Kéo-thả ảnh và folder vẫn hoạt động như trước.

## Bản 1.7 — export theo bộ lọc và menu chuột phải

- **Export ảnh đang lọc…** lấy đúng các ảnh có rating còn hiển thị trong Library Filter. Ví dụ đang lọc **5 sao** thì ảnh 4 sao không được export.
- Chuột phải trên một ảnh hoặc nhóm ảnh chọn bằng Ctrl/Shift để export đúng nhóm đó, chấm 0–5 sao, gắn màu 6–9, Pick/Reject/bỏ cờ, mở Loupe hoặc hiện file trong Explorer.
- Import thư mục đọc cả folder theo tùy chọn thư mục con. Kéo-thả ảnh/folder vẫn hoạt động.

## Bản 1.6 — nhãn màu và chọn nhiều

- Nhãn màu phủ toàn bộ nền ô ảnh theo cách hiển thị quen thuộc của Lightroom, kèm ô màu nhỏ. Tên ảnh tự dùng chữ tối trên nền màu; sao vàng giữ nguyên và có bóng viền đen nhẹ để không chìm.
- Sửa triệt để chữ biến mất trong các ComboBox ở cả Review và màn hình TXT. Giá trị đang chọn và danh sách xổ xuống đều dùng màu chữ/nền có độ tương phản rõ.
- Grid hỗ trợ chọn nhiều theo chuẩn Windows: **Ctrl + click** thêm/bớt từng ảnh, **Shift + click** chọn một dải. Các phím **0–9** và **P/X/U** áp dụng cho toàn bộ ảnh đang chọn; catalog được ghi một lần cho cả lô.

## Bản 1.5 — review nhanh và export

- Preview Loupe của ảnh raster được giải mã trực tiếp ở kích thước hiển thị thay vì ưu tiên thumbnail EXIF nhỏ, giúp ảnh JPG/PNG/TIFF sắc nét hơn khi xem lớn. RAW vẫn dùng preview do codec Windows cung cấp hoặc JPG cùng tên.
- Sửa màu và chữ của menu **Library Filter** để đọc rõ trên nền tối. Nút sao giờ chỉ sáng theo đúng rating hiện tại.
- Phím **6/7/8/9** gắn lần lượt nhãn **Đỏ/Vàng/Xanh lá/Xanh dương**; bấm lại cùng phím để bỏ màu. Nhãn màu được lưu vào catalog, hiện trên Grid/filmstrip và có bộ lọc riêng.
- Trong Grid, **↑/↓** di chuyển lên hoặc xuống đúng một hàng thumbnail; **←/→** tiếp tục di chuyển từng ảnh.
- **Export ảnh đã rate…** sao chép mọi ảnh có từ 1 sao trở lên vào thư mục riêng. Có đủ ba chính sách như luồng TXT: tự đổi tên, bỏ qua hoặc thay thế. Ảnh gốc luôn được bảo vệ.
- Import & Review và lọc TXT chuyển qua lại ngay trong **cùng một cửa sổ**. Màn hình TXT dùng cùng giao diện tối graphite và giữ nguyên dữ liệu khi quay lại Review.

## Bản 1.4 — workflow sau buổi chụp

- App mở thẳng vào **Import & Review** để photographer import thư mục, duyệt nhanh, chấm sao và đánh dấu Pick/Reject ngay sau buổi chụp.
- Nút **Lọc ảnh theo TXT…** chuyển sang công cụ lấy ảnh khách chọn. Nếu thư mục nguồn của công cụ này còn trống, app tự dùng thư mục đang review.
- Nút **Quay lại Review** giữ nguyên đường dẫn và kết quả đang làm. Đóng cửa sổ chính sẽ kết thúc ứng dụng.

## Bản 1.3 — Import & Review

- Workspace **Import & Review** theo bố cục Library quen thuộc: Navigator và thư mục bên trái, Grid/Loupe ở giữa, rating và thông tin bên phải, filmstrip phía dưới.
- **Import** ở đây là lập catalog tạm để duyệt; app không di chuyển, sao chép hay sửa ảnh gốc. Có thể kéo-thả một ảnh, nhiều ảnh, cả thư mục hoặc kết hợp chúng. Ảnh trùng được tự loại và thư mục có tùy chọn đọc cả thư mục con.
- Duyệt tốc độ cao bằng bàn phím: **←/→** ảnh trước/sau, **↑/↓** đổi hàng trong Grid, **0–5** chấm sao, **6–9** gắn màu, **P** Pick, **X** Reject, **U** bỏ cờ, **G** Grid, **E** Loupe, **Space** đổi Grid/Loupe.
- Lọc nhanh theo Pick, Reject, chưa chấm, từ 1 sao, từ 3 sao hoặc đúng 5 sao. Có thể sao chép danh sách tên của tập đang lọc để dùng lại trong workflow lọc/copy.
- Rating, cờ và nhãn màu được lưu cục bộ tại `%LOCALAPPDATA%\QiQiStudio\PhotoFileFilter\review-ratings.json` và tự khôi phục khi mở lại. Nếu kích thước hoặc thời gian sửa của ảnh thay đổi, app không áp dữ liệu cũ cho file đó. Metadata/XMP trong ảnh không bị sửa.
- Giao diện mặc định dùng bảng màu graphite và điểm nhấn xanh, đồng bộ với phong cách phần mềm ảnh chuyên nghiệp. Logo QiQi Studio tiếp tục được nhúng trong app và icon EXE.
- Thumbnail được tải nền; preview RAW phụ thuộc codec WIC của Windows hoặc JPG/JPEG cùng tên. Khả năng preview không ảnh hưởng file gốc hay chức năng lọc/copy.

## Bản 1.2 — hoàn thiện workflow

- Header thu gọn còn 70 px. Nút **Sao chép** nằm cố định ngoài mọi vùng cuộn; đã kiểm tra bố cục từ 960×620 trở lên. Bảng kết quả tự co giãn và có thanh cuộn bên trong. Bấm **Nơi lưu…** ở thanh dưới để tới cấu hình đích bên trái.
- Bật **Giao diện tối** ở header; lựa chọn được ghi nhớ. Màu bảng, ô nhập, menu và thanh cuộn đổi đồng bộ.
- Cột **COPY** cho phép chọn/bỏ từng file. Chuột phải trên dòng có Xem trước, Mở vị trí trong Explorer, Sao chép đường dẫn, Loại/đưa lại vào đợt copy và Chọn lại tất cả. Các bộ đếm, dung lượng, xác nhận và tiến độ copy dùng đúng tập file đã chọn. Bỏ chọn không thay đổi ảnh gốc hoặc thống kê tìm thấy/thiếu.
- Trong tab **Không tìm thấy**, bấm **Sao chép toàn bộ danh sách thiếu** để lấy các mã ngăn cách bằng xuống dòng, kể cả khi ô tìm kiếm đang lọc bớt phần hiển thị.
- Xem nhanh bằng **Space**, nhấp đúp dòng hoặc menu chuột phải. Đóng bằng Space/Esc. Preview chạy nền, không giữ khóa file sau khi tải và có nút mở vị trí nguồn.
- JPG/PNG/TIFF đọc thumbnail nếu có, hoặc tạo preview thu nhỏ với cạnh dài tối đa 1400 px. Áp dụng EXIF orientation khi đọc được để ảnh dọc hiển thị đúng. HEIC phụ thuộc codec có trên Windows.
- RAW thử đọc thumbnail qua codec Windows. Nếu không đọc được, thử JPG/JPEG cùng tên trong cùng thư mục và ghi rõ đây là ảnh thay thế. Không có JPG hoặc codec phù hợp thì báo không đọc được preview. **Không cam kết hỗ trợ mọi máy ảnh/định dạng RAW hay thời gian đọc tính bằng mili-giây; chưa có bộ trích xuất RAW riêng.** Khả năng preview không ảnh hưởng khả năng tìm/copy file.
- Hiển thị số file theo định dạng, ví dụ `3 CR3 · 2 JPG`.
- Viền nét đứt và nền nhấn khi kéo-thả hợp lệ. Ô tìm kiếm có placeholder và trạng thái focus rõ hơn.
- Âm báo hoàn tất có thể bật/tắt ở thanh dưới. Hủy tác vụ không phát âm báo; âm lượng phụ thuộc thiết lập âm thanh của Windows. Chưa đăng ký Windows Toast.

## Các tiện ích từ bản 1.1

- Giao diện studio với nền sáng, màu xanh trầm, các nút định dạng dạng chip, tab kết quả và trường nhập được thiết kế lại. Hai cột có thể cuộn riêng khi cửa sổ nhỏ.
- Logo `qiqistudio_nobackground.png` được nhúng nguyên bản vào giao diện. Icon EXE/thanh tiêu đề được đóng gói từ logo, có 7 kích thước từ 16 đến 256 px. File logo gốc không bị thay đổi.
- Kéo-thả TXT vào khu vực Danh sách ảnh; kéo-thả thư mục vào Ảnh gốc hoặc Nơi lưu ảnh. Kéo thư mục vào Nơi lưu ảnh tự chọn chế độ Thư mục khác.
- Tìm nhanh theo tên/vị trí tương đối của file và danh sách tên thiếu. **Tìm kiếm chỉ lọc hiển thị; Copy dùng các file được đánh dấu ở cột COPY, kể cả những file tạm ẩn bởi bộ lọc.** Mặc định chọn toàn bộ kết quả sau quét.
- Preset JPG, RAW và Tất cả cho lựa chọn định dạng nhanh.
- Nhớ TXT, thư mục nguồn/đích, định dạng, cách đọc và chính sách trùng tên tại `%LOCALAPPDATA%\QiQiStudio\PhotoFileFilter\settings.json`. Lưu khi bắt đầu tác vụ và khi đóng ứng dụng; luôn cần quét lại ở phiên tiếp theo. Nếu cấu hình hỏng hoặc không đọc được, dùng mặc định.
- Phím tắt: **Ctrl+O** chọn TXT, **F5** quét, **Esc** hủy tác vụ.

## Cách sử dụng

1. Chọn TXT và ký tự phân tách: dấu phẩy, dấu cách/tab, xuống dòng. Có thể bật nhiều loại cùng lúc.
2. Chọn các đuôi cần tìm và thư mục nguồn. Bật tìm trong thư mục con nếu cần.
3. Bấm **Quét ảnh**, xem file tìm thấy, tên chưa tìm thấy và lưu ý/lỗi. Quét không copy ảnh. Có thể bỏ dấu COPY ở các file không muốn lấy.
4. Bấm **Nơi lưu…**, chọn thư mục đích, tên thư mục con và cách xử lý trùng tên.
5. Bấm **Sao chép**, kiểm tra đường dẫn trong hộp thoại rồi xác nhận. Sau khi xong, bấm **Mở thư mục đích**. Có thể lưu báo cáo TXT.

### Quy tắc đối chiếu

- Tên và đuôi file không phân biệt hoa/thường. Loại bỏ tên yêu cầu trùng lặp.
- Mặc định bật **Bỏ phần mở rộng trong TXT**: `IMG_1001.JPG` trở thành `IMG_1001`, có thể khớp JPG và CR3 nếu chọn cả hai.
- Khi tắt: tên có đuôi khớp nguyên tên; tên không đuôi vẫn khớp các loại file đã chọn. Một file thực tế chỉ xuất hiện một lần dù nhiều yêu cầu cùng khớp.
- Một tên khớp JPG + RAW tính là **1 tên yêu cầu, 2 file tìm thấy, 0 tên thiếu**. Không bắt buộc đủ mọi đuôi mới xem là tìm thấy.
- Tên có dấu cách: tắt phân tách bằng dấu cách và dùng dấu phẩy/xuống dòng. Dấu nháy ngoài cùng được bỏ; không hỗ trợ cú pháp CSV có dấu phẩy trong tên.
- TXT hỗ trợ UTF-8, UTF-8 BOM và UTF-16 có BOM. File mã hóa cũ nên lưu lại UTF-8 trong Notepad. Nội dung phải là tên file, không phải đường dẫn.
- Khi bật bỏ đuôi, phần sau dấu chấm cuối cùng sẽ bị bỏ. Với tên ảnh có dấu chấm nhưng không có đuôi, hãy tắt tùy chọn này.

### Đích sao chép và trùng tên

- Chế độ cùng thư mục nguồn tạo **một** thư mục con tại thư mục nguồn đã chọn, ví dụ `Source/Selected`; không tạo riêng trong từng thư mục camera.
- Chế độ thư mục khác có thể bật/tắt tạo thư mục con.
- Các file được gom vào một thư mục đích, không giữ cây thư mục nguồn. Đường dẫn tương đối vẫn hiển thị trong bảng để phân biệt ảnh từ nhiều camera.
- **Tự đổi tên** (mặc định): giữ file đã có, đặt tên mới dạng `IMG_1001 (1).JPG`.
- **Bỏ qua**: giữ file đích đã có.
- **Thay thế**: thay file đích sau khi đã ghi xong bản tạm. Nếu hai ảnh nguồn cùng tên trong một lượt, ảnh sau được báo lỗi để không ghi đè ảnh vừa copy; chọn Tự đổi tên để giữ cả hai.
- Không copy đè lên các file nguồn trong kết quả quét. Không chấp nhận đích trùng trực tiếp với thư mục nguồn.
- Khi quét thư mục con, thư mục đích đang cấu hình được loại trừ để tránh lấy lại bản sao. Nếu đổi đích sau khi quét, danh sách file vẫn là kết quả quét trước đó; quét lại để áp dụng loại trừ mới.

### Tiến độ, hủy và lỗi

- Quét và copy chạy ngoài luồng giao diện. Không chạy hai tác vụ đồng thời.
- Đổi TXT, cách đọc, đuôi hoặc thư mục nguồn làm mất hiệu lực kết quả cũ và yêu cầu quét lại.
- Có thể hủy. File đã copy xong được giữ lại; file tạm đang copy được dọn. Không đóng ứng dụng giữa tác vụ; bấm Hủy trước.
- Báo lỗi từng file và tiếp tục các file khác; thư mục không đọc được được ghi ở tab Lưu ý/lỗi. Khi có cảnh báo quét, danh sách thiếu có thể gồm ảnh trong thư mục chưa đọc được.
- Kiểm tra kích thước và thời điểm sửa của ảnh trước copy; nếu thay đổi sau quét, yêu cầu quét lại. Đây không phải phép kiểm tra checksum nội dung.
- Bỏ qua liên kết file/thư mục (symlink/junction) để tránh quét vòng lặp hoặc ghi nhầm đường dẫn.
- Ghi bản tạm vào thư mục đích, hoàn tất rồi mới đổi sang tên đích. Nếu hệ thống tắt đột ngột có thể còn `.photofilter-*.tmp` trong thư mục đích.
- Không có thao tác xóa, di chuyển hay đổi tên ảnh gốc. Ghi đè chỉ áp dụng cho đích khi người dùng chọn Thay thế và xác nhận.

## Cấu trúc

```text
Core/             Models, parser, scanner, copy, kiểm tra đường dẫn
ViewModels/       Trạng thái giao diện và commands (MVVM)
Services/         Hộp thoại Windows, xác nhận, mở thư mục
Assets/           Logo nguyên bản và icon đa kích thước
Review/           Import thư mục, catalog rating, lọc và điều hướng review
scripts/          Công cụ đóng gói icon từ PNG
MainWindow.xaml   Giao diện
Styles.xaml       Màu sắc và style WPF
ThemeColors.xaml  Bảng màu sáng; ThemeService ánh xạ bảng màu tối
PreviewWindow.*   Cửa sổ xem trước ảnh
ReviewWindow.*    Workspace Grid/Loupe và filmstrip
Tests/            Kiểm thử filesystem, quy trình MVVM, render WPF
publish.ps1       Đóng gói portable tự chứa runtime
```

## Build, kiểm thử và đóng gói

```powershell
dotnet build PhotoFileFilter.sln -c Release
dotnet run --project Tests/PhotoFileFilter.Tests.csproj -c Release
.\publish.ps1
```

Tests dùng dữ liệu giả trong thư mục tạm riêng, không dùng ảnh cá nhân. Kiểm tra parser, mã hóa, nhiều extension, thư mục con, trùng tên, bảo vệ ảnh gốc, file thay đổi/bị xóa, hủy copy, xác nhận và vô hiệu hóa kết quả cũ. Render WPF được lưu tại `artifacts/screenshots`. Bộ kiểm thử không cần thư viện NuGet bên ngoài. Build/publish cần Internet nếu máy chưa cache targeting packs hoặc runtime tương ứng.

Phiên bản này chưa có bộ cài, ký số hoặc tự cập nhật. .NET 8 được dùng theo yêu cầu môi trường hiện tại; nên nâng target framework khi nâng môi trường phát triển.
