# Kế hoạch cải tiến và kiểm duyệt

Baseline: `3feb6b4` trên `main`, 163 kiểm tra tự động đạt trước khi sửa.
Các nhánh nối tiếp nhau (stacked); kiểm duyệt và gộp theo thứ tự. Không tự động gộp main.

## Phase 1 — codex/phase-1-filter-view

Tách TXT Filter thành `TxtFilterView` (UserControl), giữ MainWindow làm host độc lập mỏng.
Review host trực tiếp view, view sở hữu InputBindings; bỏ TakeContentForEmbedding và bắt phím copy/paste tại shell.
Vòng đời settings vẫn do host đóng ứng dụng quản lý, có chặn đóng khi đang thao tác.
Kiểm chứng: 164 checks, render TXT riêng/nhúng, EN/VI và laptop.
Duyệt tay: Ctrl+1/2, F5, Ctrl+O, Esc, F1; chuyển qua lại khi scan/copy; đóng và mở lại app kiểm tra settings.
Đây là bước tách TXT view, chưa phải viết lại toàn bộ ReviewWindow thành MVVM shell.

## Phase 2 — codex/phase-2-preview-cache

Cache preview LRU có giới hạn số ảnh và byte, kiểm tra thay đổi file và chất lượng decode.
Lập chỉ mục JPG cùng tên theo thư mục thay cho quét lại ở từng ảnh RAW.
Kiểm chứng: 173 checks; cache tối đa 5 ảnh / 192 MiB (ước tính bộ nhớ bitmap), index tối đa 16 thư mục, TTL 2 giây. Không cache kết quả lỗi. Không phải giới hạn tổng RAM của app hay benchmark thư mục 8.000 ảnh.
Duyệt tay: qua lại ảnh lớn, đổi chất lượng, sửa/xóa JPG đồng hành, thư mục trên ổ ngoài.

## Phase 3 — codex/phase-3-copy-feedback

Tiến độ theo byte, tốc độ truyền và thời gian còn lại; giữ copy tuần tự và commit file an toàn.
Thanh tiến độ 8px, bỏ nút đóng app cạnh Settings, chọn ngôn ngữ bằng 1/V/Enter hoặc 2/E.
Kiểm chứng: 178 checks ở Release; build 0 warnings / 0 errors; render kiểm tra TXT nhúng, Review và chọn ngôn ngữ.
Tốc độ là trung bình byte đã ghi trong phiên (MiB/s); ETA là ước tính theo tốc độ đó. Skip/error được tính là công việc đã xử lý, không tính thành byte truyền thành công. Tóm tắt cuối vẫn tách copied/skipped/errors.
Chưa có Pause/Resume; không thay đổi thuật toán collision hay tự copy song song.
Duyệt tay: copy RAW lớn, Skip/Rename/Replace, hủy giữa file và kiểm tra không còn file tạm.

## Các phase tiếp theo — chưa triển khai trong ba nhánh đầu

4. VirtualizingWrapPanel + thumbnail slider: cần kiểm tra 3.000–8.000 ảnh, selection, keyboard navigation và DPI; không chỉ đổi panel rồi tuyên bố tối ưu.
5. Semantic theme và resource localization: thay trọn bộ màu/chuỗi cả hai workspace, bỏ restart và traversal; kiểm tra EN/VI, contrast, control tạo trễ.
6. Compare đồng bộ zoom/pan, navigator viewport, clipping overlay, inspector tabs, quick preview có điều hướng.
7. Pause/resume copy và checkpoint: cần định nghĩa việc xác minh đích sau khi dừng, đổi nguồn, ngắt ổ đĩa, tránh copy trùng khi Rename.

## Nhận xét về bản đánh giá

Các vấn đề Window embedding, WrapPanel thường, quét JPG lặp, theme hex và dịch bằng traversal có cơ sở trong mã nguồn.
Các điểm số và dự đoán freeze/OOM chưa phải benchmark đã đo. Không dùng chúng làm số liệu hiệu năng thực tế.
Copy song song không mặc nhiên nhanh hơn trên HDD/USB; ưu tiên đo byte/s trước khi đổi concurrency.
Baseline đã có source protection, temporary-file commit, hủy có cleanup, EXIF orientation, lựa chọn Original và test race khi điều hướng preview.
Đóng hộp chọn ngôn ngữ lần đầu là hủy khởi chạy; không cần ép thêm một hộp xác nhận khi chưa có công việc chưa lưu.

## Chạy và kiểm duyệt

```powershell
git switch codex/phase-3-copy-feedback
dotnet run --project PhotoFileFilter.csproj -c Release
# Bộ kiểm tra dùng fixture tự tạo, không cần ảnh thật:
dotnet run --project Tests/PhotoFileFilter.Tests.csproj -c Release
```

Phase 1 so với main; phase 2 so với phase 1; phase 3 so với phase 2.
Kiểm tra ảnh RAW thật, codec máy bạn, ổ USB và thư mục lớn vẫn cần bạn duyệt trước khi gộp.
Không có benchmark thực tế cho 30–80 GB hoặc 8.000 ảnh trong đợt này.

## Phase 4 — codex/phase-4-virtualized-grid

Đã triển khai grid pixel-scrolling với virtualized fixed-size tiles và filmstrip VirtualizingStackPanel.
Thanh chỉnh thumbnail 100–400 px, lưu qua lần mở app; điều hướng lên/xuống dùng số cột thực tế.
Thumbnail tải theo ô được hiện, không còn giới hạn 120 ảnh đầu; tối đa 2 decode thumbnail đồng thời và giữ 512 thumbnail từ hàng đợi.
Kiểm tra danh sách tổng hợp 8.000 mục: số container dưới 60 ở viewport 800×500, cuộn cuối, ScrollIntoView, selection, resize và thay collection.
Đây là kiểm tra container và fixture JPG, chưa phải benchmark codec 8.000 RAW thật.
Duyệt: bấm G, kéo thanh Cỡ ảnh dưới grid, cuộn và chọn nhiều ảnh; thử thư mục hơn 120 ảnh.
