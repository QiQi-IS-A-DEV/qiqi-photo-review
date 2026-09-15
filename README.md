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
- Lọc theo rating, trạng thái và nhãn màu.
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

### Lọc RAW theo danh sách TXT

- Nhận danh sách tên ảnh từ file `.txt` hoặc trực tiếp từ màn hình Review.
- Có thể bỏ phần mở rộng để tên JPG khớp với file RAW cùng tên.
- Hỗ trợ tìm trong thư mục con và chọn nhiều loại file cùng lúc.
- Hiển thị file tìm thấy, tên không tìm thấy và các lỗi trong lúc quét.
- Cho phép bỏ chọn từng file trước khi sao chép.
- Ba cách xử lý khi trùng tên: tự đổi tên, bỏ qua hoặc thay thế file đích.
- Xuất báo cáo TXT sau khi đối chiếu và sao chép.

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
| `` ` `` hoặc `Ctrl + 0` | Đưa zoom về 100% |
| `Ctrl + Z` | Hoàn tác thao tác review gần nhất |
| `Tab` | Ẩn / hiện hai bảng bên |
| `F1` | Mở hướng dẫn sử dụng |

Các phím tắt chính có thể được thay đổi trong **Settings** của màn hình Review.

## Định dạng hỗ trợ

### Ảnh raster

`JPG`, `JPEG`, `PNG`, `TIF`, `TIFF`, `HEIC`

### Ảnh RAW

`ARW`, `CR2`, `CR3`, `NEF`, `RAF`, `ORF`, `RW2`, `DNG`

Việc tìm và sao chép file RAW không cần giải mã nội dung ảnh. Khả năng **hiển thị preview RAW** phụ thuộc codec ảnh đang được cài trong Windows. Nếu RAW không đọc được và có JPG/JPEG cùng tên trong cùng thư mục, ứng dụng sẽ dùng ảnh JPG đó làm preview và hiển thị thông báo rõ ràng.

## Yêu cầu hệ thống

- Windows 10 hoặc Windows 11, kiến trúc x64.
- Mã nguồn sử dụng C#, WPF và .NET 8.
- Visual Studio có workload **.NET desktop development**, hoặc .NET 8 SDK nếu chạy bằng dòng lệnh.

Bản publish `win-x64` là self-contained, đã kèm .NET runtime và không yêu cầu cài .NET riêng trên máy người dùng.

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

Bộ kiểm thử hiện gồm **120 kiểm tra**, bao phủ parser TXT, quét thư mục, bảo vệ ảnh gốc, chính sách trùng tên, session Review, rating, nhãn màu, xoay, zoom, histogram và bố cục WPF.

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

**1.17.0**

- Bổ sung zoom theo con trỏ và 1-click zoom với mức phóng đại tùy chỉnh.
- Đảo vị trí header: điều khiển hỗ trợ bên trái, thao tác chính bên phải.
- Sắp xếp lại header: thao tác chính ở bên trái, điều khiển hỗ trợ ở bên phải.
- Giao diện ứng dụng sử dụng tiếng Anh thống nhất.
- Popup hướng dẫn mô tả đầy đủ quy trình JPG → RAW.
- Cải thiện cách xuống dòng, tooltip và khả năng hiển thị văn bản trong cửa sổ nhỏ.
- Hỗ trợ tùy chỉnh phím tắt và cấu hình xem ảnh.
