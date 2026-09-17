# QiQi Photo Review

<p align="center">
  <img src="Assets/qiqistudio_nobackground.png" alt="QiQi Studio" width="120" />
</p>

Ứng dụng desktop dành cho photographer trên Windows, hỗ trợ **import, review, chấm điểm, gắn nhãn màu và lọc ảnh RAW theo danh sách TXT**.

QiQi Photo Review tập trung vào quy trình thực tế sau buổi chụp:

1. Import thư mục JPG hoặc RAW để xem và lọc nhanh.
2. Chấm sao, gắn màu hoặc đánh dấu Pick/Reject.
3. Xuất danh sách tên ảnh đã chọn hoặc chuyển trực tiếp sang màn hình lọc TXT.
4. Tìm các file RAW cùng tên và sao chép chúng vào một thư mục riêng để chỉnh sửa.

Lần mở đầu tiên, ứng dụng cho phép chọn **Tiếng Việt** hoặc **English**. Có thể đổi lại trong **Cài đặt / Settings**; ứng dụng sẽ xác nhận, lưu phiên và tự khởi động lại để áp dụng đồng bộ.

> Ứng dụng không chỉnh sửa, di chuyển hoặc xóa ảnh gốc. Rating, nhãn màu và góc xoay chỉ được lưu trong dữ liệu cục bộ của ứng dụng.

## Tính năng chính

### Import và review ảnh

- Import toàn bộ thư mục bằng hộp thoại Windows có hiển thị ảnh để xác nhận đúng thư mục.
- Kéo thả một ảnh, nhiều ảnh hoặc cả thư mục trực tiếp vào cửa sổ.
- Tùy chọn đọc ảnh trong các thư mục con.
- Chuyển nhanh giữa chế độ **Grid** và **Loupe**.
- Chọn nhiều ảnh bằng `Ctrl + click` hoặc `Shift + click` rồi áp dụng rating, màu hoặc cờ cho cả nhóm.
- Tự lưu và khôi phục phiên làm việc gần nhất khi mở lại ứng dụng.

### Đánh giá và lọc ảnh

- Chấm từ 0 đến 5 sao.
- Gắn nhãn màu đỏ, vàng, xanh lá hoặc xanh dương.
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
- Có chế độ ẩn hai bảng bên để dành thêm không gian xem ảnh.
- Cho phép tùy chỉnh độ phân giải preview, bước zoom, vị trí thông báo và một số phím tắt.
- Trong **Cài đặt → Chất lượng preview**, chọn 1.200, 1.800, 2.400, 3.600, 4.800 px hoặc **Original · Độ phân giải gốc**. Thay đổi được áp dụng ngay trên ảnh đang xem.
- Tùy chọn **Tải độ phân giải gốc khi zoom** tự nạp ảnh đầy đủ khi zoom trong Loupe và về mức đã chọn khi trở lại Fit. App chỉ giữ preview lớn cho ảnh hiện tại, còn thumbnail vẫn nhỏ.
- Dòng thông tin dưới ảnh cho biết kích thước pixel và nguồn preview thực tế; Original dùng nhiều RAM hơn. Mức zoom được tính theo Fit, không phải tỷ lệ pixel 1:1.

### Lọc RAW theo danh sách TXT

- Nhận danh sách tên ảnh từ file `.txt` hoặc trực tiếp từ màn hình Review.
- Header hiển thị bước đang thực hiện để người dùng biết cần chọn TXT, chọn thư mục nguồn, quét hay sao chép.
- Có popup hướng dẫn riêng, giải thích tuần tự cách chọn TXT, thiết lập quy tắc khớp tên, quét và sao chép.
- Có thể bỏ phần mở rộng để tên JPG khớp với file RAW cùng tên.
- Hỗ trợ tìm trong thư mục con và chọn nhiều loại file cùng lúc.
- Hiển thị file tìm thấy, tên không tìm thấy và các lỗi trong lúc quét.
- Cho phép bỏ chọn từng file trước khi sao chép.
- Ba cách xử lý khi trùng tên: tự đổi tên, bỏ qua hoặc thay thế file đích.
- Xuất báo cáo TXT sau khi đối chiếu và sao chép.
- Nút **Clear session** trên header xóa đường dẫn và kết quả hiện tại nhưng không thay đổi file TXT hoặc ảnh gốc.

## Phím tắt Review

| Phím | Chức năng |
| --- | --- |
| `←` / `→` | Ảnh trước / ảnh sau |
| `↑` / `↓` | Di chuyển lên / xuống một hàng trong Grid |
| `0`–`5` | Xóa hoặc đặt rating |
| `6`–`9` | Gắn nhãn đỏ / vàng / xanh lá / xanh dương |
| `*` | Xóa nhãn màu |
| `P` / `X` / `U` | Pick / Reject / bỏ cờ |
| `G` / `E` | Mở Grid / Loupe |
| `Space` | Chuyển giữa Grid và Loupe |
| `Ctrl + [` / `Ctrl + ]` | Xoay trái / phải 90° |
| `Ctrl + +` / `Ctrl + -` | Phóng to / thu nhỏ |
| `` ` `` hoặc `Ctrl + 0` | Đưa ảnh về Fit toàn khung |
| `Ctrl + Z` | Hoàn tác thao tác review gần nhất |
| `Tab` | Ẩn / hiện hai bảng bên |
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

Mở `PhotoFileFilter.sln` bằng Visual Studio, chọn `PhotoFileFilter` làm Startup Project và nhấn `F5`.

Hoặc chạy bằng .NET CLI:

```powershell
dotnet run --project PhotoFileFilter.csproj
```

## Build và kiểm thử

```powershell
dotnet build PhotoFileFilter.sln -c Release
dotnet run --project Tests/PhotoFileFilter.Tests.csproj -c Release
```

Bộ kiểm thử hiện gồm **163 kiểm tra**, bao phủ parser TXT, quét thư mục, bảo vệ ảnh gốc, chính sách trùng tên, session Review và TXT Filter, chuyển ngôn ngữ, rating, nhãn màu, phạm vi export, xoay, zoom, độ phân giải preview, histogram và bố cục WPF.

## Đóng gói bản portable

```powershell
.\publish.ps1
```

File chạy được tạo tại:

```text
artifacts/publish/win-x64/PhotoFileFilter.exe
```

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
Core/                 Parser TXT, scanner, copy và kiểm tra đường dẫn
Review/               Import, catalog, session, rating và histogram
Services/             Hộp thoại Windows, preview và cấu hình
ViewModels/           Trạng thái giao diện và commands
Tests/                Kiểm thử logic và render bố cục WPF
Assets/               Logo và icon ứng dụng
MainWindow.*           Màn hình lọc ảnh theo TXT
ReviewWindow.*         Workspace Import & Review
PreviewWindow.*        Cửa sổ xem nhanh ảnh
publish.ps1            Script đóng gói portable cho Windows x64
```

## Phiên bản hiện tại

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
