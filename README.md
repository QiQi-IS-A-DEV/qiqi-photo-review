# QiQi Photo Review

<p align="center">
  <img src="src/PhotoFileFilter.App/Assets/qiqistudio_nobackground.png" alt="QiQi Studio" width="120" />
</p>

Ứng dụng desktop dành cho photographer trên Windows, hỗ trợ **import, review, chấm điểm, gắn nhãn màu và lọc ảnh RAW theo danh sách TXT**.

QiQi Photo Review tập trung vào quy trình thực tế sau buổi chụp:

1. Import thư mục JPG hoặc RAW để xem và lọc nhanh.
2. Chấm sao, gắn màu hoặc đánh dấu Pick/Reject.
3. Xuất danh sách tên ảnh đã chọn hoặc chuyển trực tiếp sang màn hình lọc TXT.
4. Tìm các file RAW cùng tên và sao chép chúng vào một thư mục riêng để chỉnh sửa.

Lần mở đầu tiên, ứng dụng cho phép chọn **Tiếng Việt** hoặc **English**. Có thể đổi lại trong **Cài đặt / Settings**; toàn bộ giao diện đang mở cập nhật ngay và không cần khởi động lại.

> Ứng dụng không chỉnh sửa, di chuyển hoặc xóa ảnh gốc. Rating, nhãn màu và góc xoay chỉ được lưu trong dữ liệu cục bộ của ứng dụng.

## Hướng dẫn sử dụng nhanh

### Review và chọn ảnh

1. Chọn **Import folder** hoặc kéo thả ảnh/thư mục vào cửa sổ.
2. Dùng Grid để duyệt ảnh; nhấp đúp hoặc nhấn `Space` để mở Loupe. Nhấn `Space` lần nữa để trở về Grid.
3. Chấm sao, gắn nhãn màu hoặc đặt cờ Pick/Reject. Dùng **Photo Filter** để chỉ giữ lại các ảnh cần xem.
4. Trong **Review + Deliver**, xuất danh sách TXT hoặc chọn **Send to TXT Filter** để chuyển trực tiếp các tên đang hiển thị.

### Tìm và sao chép RAW

1. Chọn danh sách `.txt` hoặc nhận danh sách từ màn hình Review.
2. Chọn thư mục chứa RAW, loại file cần tìm và quy tắc khớp tên, sau đó chọn **Scan**.
3. Kiểm tra kết quả; nhấn `Space` hoặc nhấp đúp để mở Quick Preview. Trong cửa sổ xem nhanh, dùng `←`/`→` để đổi ảnh, con lăn để zoom, kéo để pan, `R` để xoay và `Esc` để đóng.
4. Bỏ chọn file không cần, chọn thư mục đích và cách xử lý trùng tên, rồi chọn **Copy**.

## Tính năng chính

### Import và review ảnh

- Import toàn bộ thư mục bằng hộp thoại Windows có hiển thị ảnh để xác nhận đúng thư mục.
- Kéo thả một ảnh, nhiều ảnh hoặc cả thư mục trực tiếp vào cửa sổ.
- Tùy chọn đọc ảnh trong các thư mục con.
- Chuyển nhanh giữa **Grid** và **Loupe** bằng `Space`, `G` hoặc `E`.
- Grid ảo hóa chỉ tạo các ô đang hiển thị; thanh **Cỡ ảnh** thay đổi thumbnail từ 100–400 px.
- Filmstrip có thể kéo thay đổi chiều cao và ẩn/hiện bằng `Ctrl + F`; lựa chọn được tự động lưu.
- Chọn nhiều ảnh bằng `Ctrl + click` hoặc `Shift + click` rồi áp dụng rating, màu hoặc cờ cho cả nhóm.
- Tự lưu và khôi phục phiên làm việc gần nhất khi mở lại ứng dụng.

### Đánh giá và lọc ảnh

- Chấm từ 0 đến 5 sao.
- Gắn nhãn đỏ, vàng, xanh lá, xanh dương hoặc tím; rê chuột trên sao để xem trước rating trước khi lưu.
- Đánh dấu **Pick**, **Reject** hoặc bỏ cờ.
- Lọc theo rating, trạng thái và nhãn màu; khi bộ lọc không có kết quả, app giải thích nguyên nhân và cho phép trở về tất cả ảnh ngay tại chỗ.
- Hoàn tác thao tác review gần nhất bằng `Ctrl + Z`.
- Menu chuột phải hỗ trợ rating, nhãn màu, cờ, xoay, xuất ảnh và mở vị trí file.

### Xem ảnh

- Zoom bằng bàn phím hoặc con lăn chuột.
- Zoom theo đúng vị trí con trỏ để vùng đang quan sát không bị đẩy khỏi khung hình.
- Nhấp một lần trong Loupe để zoom đến mức tùy chọn; nhấp lại để trở về chế độ Fit.
- Kéo ảnh bằng chuột khi đang zoom lớn hơn 100%.
- Xoay trái hoặc phải 90° mà không thay đổi file gốc.
- Histogram RGB giúp kiểm tra nhanh vùng sáng, vùng tối và độ sáng trung bình.
- Inspector có hai tab **Review + Deliver** và **Tools**; Histogram luôn nằm đầu tab Review + Deliver.
- Có chế độ ẩn hai bảng bên để dành thêm không gian xem ảnh.
- Cho phép tùy chỉnh độ phân giải preview, bước zoom, vị trí thông báo và một số phím tắt.
- Trong **Cài đặt → Chất lượng preview**, chọn 1.200, 1.800, 2.400, 3.600, 4.800 px hoặc **Original · Độ phân giải gốc**. Thay đổi được áp dụng ngay trên ảnh đang xem.
- Tùy chọn **Tải độ phân giải gốc khi zoom** tự nạp ảnh đầy đủ khi zoom trong Loupe và về mức đã chọn khi trở lại Fit. App giữ tối đa 5 preview gần nhất trong ngân sách cache 192 MiB ước tính; ảnh vượt ngân sách không được giữ trong cache, còn thumbnail vẫn nhỏ.
- Dòng thông tin dưới ảnh cho biết kích thước pixel và nguồn preview thực tế; Original dùng nhiều RAM hơn. Mức zoom được tính theo Fit, không phải tỷ lệ pixel 1:1.

### Lọc RAW theo danh sách TXT

- Nhận danh sách tên ảnh từ file `.txt` hoặc trực tiếp từ màn hình Review.
- Header hiển thị bước đang thực hiện để người dùng biết cần chọn TXT, chọn thư mục nguồn, quét hay sao chép.
- Có popup hướng dẫn riêng, giải thích tuần tự cách chọn TXT, thiết lập quy tắc khớp tên, quét và sao chép.
- Có thể bỏ phần mở rộng để tên JPG khớp với file RAW cùng tên.
- Hỗ trợ tìm trong thư mục con và chọn nhiều loại file cùng lúc.
- Hiển thị file tìm thấy, tên không tìm thấy và các lỗi trong lúc quét.
- Quick Preview mở không khóa cửa sổ chính: `←`/`→` đổi ảnh, con lăn zoom, kéo để pan, `R` xoay góc xem và `Esc` đóng.
- Cho phép bỏ chọn từng file trước khi sao chép.
- Ba cách xử lý khi trùng tên: tự đổi tên, bỏ qua hoặc thay thế file đích.
- Xuất báo cáo TXT sau khi đối chiếu và sao chép.
- Khi sao chép, hiển thị tiến độ theo byte, tốc độ MiB/s và thời gian còn lại ước tính.
- Có thể **Tạm dừng / Tiếp tục** trong lúc sao chép. File đang ghi chỉ dùng tên tạm; các file hoàn tất được giữ làm checkpoint và mỗi file còn lại được kiểm tra lại trước khi tiếp tục.
- Nút **Clear session** trên header xóa đường dẫn và kết quả hiện tại nhưng không thay đổi file TXT hoặc ảnh gốc.

## Phím tắt Review

| Phím | Chức năng |
| --- | --- |
| `←` / `→` | Ảnh trước / ảnh sau |
| `↑` / `↓` | Di chuyển lên / xuống một hàng trong Grid |
| `0`–`5` | Xóa hoặc đặt rating |
| `6`–`9` / `T` | Gắn nhãn đỏ / vàng / xanh lá / xanh dương / tím |
| `*` | Xóa nhãn màu |
| `P` / `X` / `U` | Pick / Reject / bỏ cờ |
| `G` / `E` | Mở Grid / Loupe |
| `Space` | Chuyển giữa Grid và Loupe |
| `Ctrl + [` / `Ctrl + ]` | Xoay trái / phải 90° |
| `Ctrl + +` / `Ctrl + -` | Phóng to / thu nhỏ |
| `` ` `` hoặc `Ctrl + 0` | Đưa ảnh về Fit toàn khung |
| `Ctrl + Z` | Hoàn tác thao tác review gần nhất |
| `Tab` | Ẩn / hiện hai bảng bên |
| `Ctrl + F` | Ẩn / hiện Filmstrip |
| `F1` | Mở hướng dẫn sử dụng |
| `Ctrl + 1` / `Ctrl + 2` | Chuyển giữa Import & Review và TXT Filter |

Các phím tắt chính có thể được thay đổi trong **Settings** của màn hình Review.

## Định dạng hỗ trợ

### Ảnh raster

`JPG`, `JPEG`, `PNG`, `TIF`, `TIFF`, `HEIC`

### Ảnh RAW

`ARW`, `CR2`, `CR3`, `NEF`, `RAF`, `ORF`, `RW2`, `DNG`

Việc tìm và sao chép file RAW không cần giải mã nội dung ảnh. Khi xem ảnh, app thử giải mã RAW đầy đủ qua codec Windows. Nếu không đọc được, app thử JPG/JPEG cùng tên trong cùng thư mục, rồi preview nhúng trong RAW. Dòng thông tin dưới ảnh ghi rõ nguồn và kích thước preview; ảnh nhúng nhỏ không thể cung cấp chi tiết tương đương ảnh gốc chỉ bằng cách zoom.

## Yêu cầu hệ thống

- Windows 10 hoặc Windows 11, kiến trúc x64.
- Mã nguồn sử dụng C#, WPF và .NET 8.
- Visual Studio có workload **.NET desktop development**, hoặc .NET 8 SDK nếu chạy bằng dòng lệnh.

Bản publish `win-x64` là self-contained, đã kèm .NET runtime và không yêu cầu cài .NET riêng trên máy người dùng.

## Gửi app cho người dùng thử

Người dùng thông thường không cần Git, Visual Studio hoặc .NET. Hãy gửi file ZIP portable trong `artifacts/distribution`, sau đó hướng dẫn họ:

1. Giải nén toàn bộ file ZIP.
2. Nhấp đúp `PhotoFileFilter.exe` để mở ứng dụng.
3. Nếu Windows SmartScreen xuất hiện do ứng dụng chưa ký số, chọn **More info** rồi **Run anyway**.

File `.txt` chỉ là danh sách tên ảnh dùng làm đầu vào cho màn hình TXT Filter, không phải file chạy ứng dụng.

## Chạy từ mã nguồn

Mở `PhotoFileFilter.sln` bằng Visual Studio, chọn `PhotoFileFilter.App` làm Startup Project và nhấn `F5`.

Hoặc chạy bằng .NET CLI:

```powershell
dotnet run --project src/PhotoFileFilter.App/PhotoFileFilter.App.csproj
```

## Build và kiểm thử

```powershell
./scripts/Verify.ps1
```

Bộ kiểm thử hiện gồm **233 kiểm tra**, bao phủ parser TXT, quét thư mục, bảo vệ ảnh gốc, chính sách trùng tên, tạm dừng/tiếp tục và tiến độ copy theo byte, session Review và TXT Filter, chuyển ngôn ngữ trực tiếp, semantic theme, hướng dẫn sử dụng, rating, năm nhãn màu, export, Quick Preview, virtualization, xoay, zoom, preview, histogram và bố cục WPF.

GitHub Actions chạy cùng quy trình trên Windows cho mọi pull request vào `main` và các nhánh `phase-*`. Nếu kiểm tra giao diện thất bại, ảnh render được tải lên dưới dạng CI artifact để chẩn đoán.

## Đóng gói bản portable

```powershell
.\publish.ps1
```

File chạy được tạo tại:

```text
artifacts/publish/win-x64/PhotoFileFilter.exe
```

Script cũng chép `LICENSE`, `NOTICE` và `README.md` vào thư mục publish để bản portable giữ đầy đủ thông tin giấy phép và ghi nhận.

Thư mục `artifacts` không được đưa vào Git. Nếu cần phát hành file EXE qua GitHub, hãy đính kèm file từ thư mục này vào một GitHub Release.

## Dữ liệu được lưu ở đâu?

Ứng dụng lưu cấu hình và dữ liệu phiên tại:

```text
%LOCALAPPDATA%\QiQiStudio\PhotoFileFilter\
```

| File | Nội dung |
| --- | --- |
| `settings.json` | Cấu hình màn hình lọc TXT |
| `language.json` | Ngôn ngữ giao diện đã chọn |
| `review-preferences.json` | Chất lượng preview, zoom, giao diện và phím tắt |
| `review-session.json` | Thư mục, ảnh và trạng thái của phiên Review gần nhất |
| `review-ratings.json` | Rating, cờ, nhãn màu và góc xoay |
| `review-selection.txt` | Danh sách trung gian khi chuyển từ Review sang lọc TXT |

Preview trong bộ nhớ có thể được giải phóng từ mục **Storage & Cache**. Thumbnail do Windows quản lý có thể được dọn bằng **Windows Disk Cleanup**.

## Quy tắc bảo vệ dữ liệu

- Không xóa, đổi tên hoặc ghi metadata vào ảnh gốc.
- Không sao chép trực tiếp đè lên thư mục nguồn.
- Bỏ qua junction và symbolic link để tránh quét vòng lặp.
- Kiểm tra kích thước và thời gian sửa file trước khi sao chép.
- Ghi qua file tạm rồi mới hoàn tất file đích.
- Khi hủy, các file đã sao chép xong được giữ lại và file tạm được dọn.

## Cấu trúc dự án

```text
src/
├─ PhotoFileFilter.App/             Ứng dụng WPF và composition root
│  ├─ Features/
│  │  ├─ Onboarding/Views/          Chọn ngôn ngữ lần đầu
│  │  ├─ Review/                    Import, review, rating, Grid/Loupe và bàn giao
│  │  └─ TxtFilter/                 Đọc TXT, quét, xem nhanh và điều khiển sao chép
│  ├─ Shared/                       UI, dịch vụ và ViewModel dùng chung
│  ├─ Assets/                       Logo và icon đóng gói cùng app
│  └─ App.xaml                      Điểm khởi động và tài nguyên toàn app
└─ PhotoFileFilter.Core/            Parser, scanner, copy và bảo vệ đường dẫn; không phụ thuộc WPF
tests/
└─ PhotoFileFilter.Tests/           Kiểm thử logic, workflow và render WPF
docs/                               Kiến trúc, kế hoạch và tài liệu bảo trì
examples/                           Dữ liệu đầu vào mẫu
scripts/                            Script hỗ trợ phát triển
publish.ps1                         Đóng gói portable Windows x64
```

Xem [bản đồ kiến trúc và quy tắc đặt file](docs/PROJECT-STRUCTURE.md) trước khi thêm tính năng. Hướng dẫn gửi thay đổi nằm trong [CONTRIBUTING.md](CONTRIBUTING.md).

## Phiên bản hiện tại

**1.30.0 — open-source quality**

- Bổ sung GitHub Actions cho restore, Release build và toàn bộ WPF regression suite trên Windows.
- Thêm `Verify.ps1`, cấu hình SDK, deterministic build, warnings-as-errors và quy ước `.editorconfig`.
- Bổ sung pull-request template, Dependabot, chính sách bảo mật và release checklist.

**1.29.0 — semantic theme**

- Thay các khóa màu theo mã hex bằng token có ý nghĩa như `AppBackground`, `FieldBackground`, `SelectionBackground` và `WarningText`.
- Khai báo palette sáng/tối rõ ràng trong một nơi, không còn suy diễn theme từ tên khóa màu.
- Gom các màu riêng của workspace Review thành nhóm resource có tên để dễ đổi giao diện và bàn giao.

**1.28.0 — tạm dừng và tiếp tục sao chép**

- Thêm nút Pause/Resume vào thanh thao tác của TXT Filter khi đang copy.
- Giữ các file đã hoàn tất như checkpoint; file đang ghi vẫn là file tạm và được dọn khi hủy.
- Kiểm tra lại kích thước và thời gian sửa của từng nguồn sau khi tiếp tục để tránh chép dữ liệu đã thay đổi.

**1.27.0 — chuyển ngôn ngữ trực tiếp**

- Chuyển qua lại giữa Tiếng Việt và English ngay trong Settings mà không khởi động lại app.
- Giao diện tĩnh dùng binding bản địa hóa, không còn duyệt và thay chữ trong cây WPF.
- Đồng bộ lại nhãn động của Review, TXT Filter, Quick Preview và tiêu đề cửa sổ khi đổi ngôn ngữ.

**1.26.0 — chuẩn hóa cấu trúc dự án open source**

- Tách solution thành `src`, `tests`, `docs`, `examples` và `scripts`.
- Mã giao diện được gom theo workflow `Review`, `TxtFilter` và `Onboarding`; mã dùng chung nằm trong `Shared`.
- Namespace khớp với đường dẫn file; Core giữ độc lập với WPF.
- Bổ sung tài liệu kiến trúc, quy định quyền sở hữu thư mục và hướng dẫn đóng góp.

**1.25.1 — bảng hướng dẫn phím tắt dễ đọc**

- Phần phím tắt trong hướng dẫn Review và TXT Filter được trình bày thành bảng **Phím | Chức năng**.
- Các tổ hợp phím hiển thị dạng keycap, có đường phân cách từng hàng và hỗ trợ đầy đủ tiếng Anh/Việt.

**1.25.0 — bản ổn định hợp nhất vào main**

- Cập nhật hướng dẫn Review và TXT Filter theo đúng thao tác hiện tại, bằng cả tiếng Anh và tiếng Việt.
- Hoàn thiện kiến trúc workspace TXT Filter, cache preview, chỉ mục JPG đồng hành và tiến độ copy theo byte.
- Grid/Filmstrip được ảo hóa; thumbnail và chiều cao Filmstrip có thể điều chỉnh.
- Inspector gồm **Review + Deliver | Tools**, với Histogram ở đầu.
- Thêm nhãn tím, xem trước rating khi hover và Quick Preview có điều hướng/zoom/pan/xoay.
- `Space` chuyển Grid ↔ Loupe. Chế độ Compare thử nghiệm đã được loại bỏ trước bản ổn định.

**1.24.1 — Inspector theo góp ý kiểm duyệt**

- Hai tab: Review + Deliver (Đánh giá + Bàn giao) | Tools (Công cụ).
- Histogram đứng đầu, tiếp theo Rating & Flags, Rotate & Zoom và Deliver & RAW Workflow trong cùng vùng cuộn.

**1.24.0 — Inspector và điều khiển Review**

- Inspector chia Đánh giá / Bàn giao / Công cụ để truy cập nhanh từng nhóm thao tác.
- Filmstrip: Ctrl+F hoặc checkbox góc dưới phải để ẩn/hiện; kéo mép trên để đổi chiều cao, tự lưu.
- Nhãn tím: T, nút tím hoặc menu ngữ cảnh; hỗ trợ lọc, lưu catalog và undo.
- Rê chuột trên sao để xem trước rating trước khi click.
- Photo Source có nút chép đường dẫn và mở thư mục.
- Space vẫn chuyển Grid ↔ Loupe; không có Compare. Quick Preview TXT được giữ nguyên.

**1.23.1 — bỏ Compare, khôi phục Space ở Review**

- Loại bỏ nút, phím C và chế độ So sánh theo yêu cầu kiểm duyệt.
- Space chuyển qua lại Grid ↔ Loupe, giữ ảnh đang chọn; giữ phím không chuyển liên tục.
- Kéo chuột trong Loupe đã zoom để pan; không cần giữ Space.
- Giữ grid virtualization, thanh Cỡ ảnh và Quick Preview TXT nâng cấp.

**1.23.0 — bản thử nghiệm nội bộ**

- Grid và filmstrip có virtualization; thanh Cỡ ảnh 100–400 px được lưu qua lần mở app.
- Thumbnail tải theo ô xuất hiện, kể cả ảnh ngoài 120 mục đầu; giới hạn hàng đợi lưu thumbnail.
- Quick Preview TXT không khóa cửa sổ chính; ←/→ chuyển ảnh, con lăn zoom, kéo để pan, R xoay tạm thời, Esc đóng.

**1.22.0 — bản kiểm duyệt trên nhánh phase-3-copy-feedback**

- Tách TXT Filter thành UserControl, sở hữu phím tắt riêng khi nhúng trong Review.
- Cache preview LRU tối đa 5 ảnh / 192 MiB ước tính và chỉ mục JPG đồng hành cho RAW.
- Copy có tiến độ theo byte, tốc độ MiB/s và ETA; thanh tiến độ dễ quan sát hơn.
- Bỏ nút Close cạnh Settings; thêm phím 1/V/Enter và 2/E ở màn hình chọn ngôn ngữ.
- Các thay đổi ở giai đoạn này được kiểm duyệt trên nhánh phase trước khi hợp nhất.

**1.21.1**

- Hiển thị số phiên bản thực tế trên thanh tiêu đề của Review và Lọc TXT, tự lấy từ bản build.
- Cửa sổ app mở ở giữa màn hình và điều chỉnh kích thước để vừa vùng làm việc.

**1.21.0**

- Sửa import mới bị ẩn ảnh do giữ bộ lọc màu hoặc Rating từ thư mục trước; mở lại phiên vẫn khôi phục bộ lọc đã lưu.
- Thêm preview 3.600 px, 4.800 px và Original, áp dụng ngay mà không cần import lại.
- Thêm tùy chọn tự tải độ phân giải gốc khi zoom trong Loupe, mặc định bật.
- Đọc ảnh RAW đầy đủ qua codec Windows thay vì chỉ lấy thumbnail nhúng; ghi rõ nguồn preview và kích thước thực tế.
- Giới hạn thumbnail ở kích thước nhỏ và tính histogram theo từng hàng để giảm bộ nhớ khi soi ảnh lớn.

**1.20.0**

- Tinh chỉnh giao diện theo tinh thần Lightroom Classic với bề mặt phẳng, góc bo gọn và độ tương phản rõ hơn.
- Thêm trạng thái bắt đầu ngay trong Review, kèm nút Import và hướng dẫn bước tiếp theo.
- Thêm trạng thái riêng khi bộ lọc không có ảnh, cho phép trở về toàn bộ ảnh mà không ảnh hưởng Rating.
- Làm rõ phạm vi bàn giao: tên file lấy từ các ảnh đang hiện; thao tác chép ảnh chỉ lấy ảnh đã Rating trong bộ lọc hiện tại.
- Hiển thị tiến độ bốn bước trên màn hình TXT Filter để làm rõ thao tác tiếp theo.

- Thêm lựa chọn Tiếng Việt/English ở lần chạy đầu tiên.
- Cho phép đổi ngôn ngữ trong Settings, xác nhận rồi tự khởi động lại.
- Dịch các thao tác, hướng dẫn và trạng thái chính sang tiếng Việt; giữ thuật ngữ nhiếp ảnh quen thuộc bằng tiếng Anh.
- Bổ sung hướng dẫn riêng cho màn hình TXT Filter.
- Thêm nút xóa phiên TXT Filter, gồm đường dẫn đã lưu và kết quả quét hiện tại.
- Thêm `Ctrl+1` và `Ctrl+2` để chuyển workspace, đồng thời làm rõ trạng thái workspace trên header.
- Bổ sung zoom theo con trỏ và 1-click zoom với mức phóng đại tùy chỉnh.
- Đảo vị trí header: điều khiển hỗ trợ bên trái, thao tác chính bên phải.
- Giao diện ứng dụng sử dụng tiếng Anh thống nhất.
- Hai popup hướng dẫn mô tả riêng quy trình Review và quy trình lọc TXT.
- Cải thiện cách xuống dòng, tooltip và khả năng hiển thị văn bản trong cửa sổ nhỏ.
- Hỗ trợ tùy chỉnh phím tắt và cấu hình xem ảnh.

## Bản quyền và giấy phép

Copyright © 2026 **QiQi Studio (QiQi-IS-A-DEV)**.

Dự án được phát hành theo [Apache License 2.0](LICENSE). Bạn có thể sử dụng, sửa đổi và phân phối mã nguồn theo các điều kiện của giấy phép. Các thông tin ghi nhận cần giữ lại được liệt kê trong [NOTICE](NOTICE).

Apache-2.0 không cấp quyền sử dụng tên thương mại, nhãn hiệu hoặc nhận diện **QiQi Studio**, ngoại trừ việc mô tả hợp lý nguồn gốc của dự án và sao chép nội dung NOTICE.
