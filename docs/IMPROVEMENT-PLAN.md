# Kế hoạch cải tiến và kiểm duyệt

Baseline: `3feb6b4` trên `main`, 163 kiểm tra tự động đạt trước khi sửa.
Các nhánh nối tiếp nhau (stacked); kiểm duyệt và gộp theo thứ tự. Không tự động gộp main.

## Phase 1 — phase-1-filter-view

Tách TXT Filter thành `TxtFilterView` (UserControl), giữ MainWindow làm host độc lập mỏng.
Review host trực tiếp view, view sở hữu InputBindings; bỏ TakeContentForEmbedding và bắt phím copy/paste tại shell.
Vòng đời settings vẫn do host đóng ứng dụng quản lý, có chặn đóng khi đang thao tác.
Kiểm chứng: 164 checks, render TXT riêng/nhúng, EN/VI và laptop.
Duyệt tay: Ctrl+1/2, F5, Ctrl+O, Esc, F1; chuyển qua lại khi scan/copy; đóng và mở lại app kiểm tra settings.
Đây là bước tách TXT view, chưa phải viết lại toàn bộ ReviewWindow thành MVVM shell.

## Phase 2 — phase-2-preview-cache

Cache preview LRU có giới hạn số ảnh và byte, kiểm tra thay đổi file và chất lượng decode.
Lập chỉ mục JPG cùng tên theo thư mục thay cho quét lại ở từng ảnh RAW.
Kiểm chứng: 173 checks; cache tối đa 5 ảnh / 192 MiB (ước tính bộ nhớ bitmap), index tối đa 16 thư mục, TTL 2 giây. Không cache kết quả lỗi. Không phải giới hạn tổng RAM của app hay benchmark thư mục 8.000 ảnh.
Duyệt tay: qua lại ảnh lớn, đổi chất lượng, sửa/xóa JPG đồng hành, thư mục trên ổ ngoài.

## Phase 3 — phase-3-copy-feedback

Tiến độ theo byte, tốc độ truyền và thời gian còn lại; giữ copy tuần tự và commit file an toàn.
Thanh tiến độ 8px, bỏ nút đóng app cạnh Settings, chọn ngôn ngữ bằng 1/V/Enter hoặc 2/E.
Kiểm chứng: 178 checks ở Release; build 0 warnings / 0 errors; render kiểm tra TXT nhúng, Review và chọn ngôn ngữ.
Tốc độ là trung bình byte đã ghi trong phiên (MiB/s); ETA là ước tính theo tốc độ đó. Skip/error được tính là công việc đã xử lý, không tính thành byte truyền thành công. Tóm tắt cuối vẫn tách copied/skipped/errors.
Chưa có Pause/Resume; không thay đổi thuật toán collision hay tự copy song song.
Duyệt tay: copy RAW lớn, Skip/Rename/Replace, hủy giữa file và kiểm tra không còn file tạm.

## Backlog sau ba nhánh đầu (cập nhật tiến độ ở các mục cuối)

4. [Đã triển khai ở phase 4] VirtualizingWrapPanel + thumbnail slider: cần kiểm tra 3.000–8.000 ảnh, selection, keyboard navigation và DPI; không chỉ đổi panel rồi tuyên bố tối ưu.
5. Semantic theme và resource localization: thay trọn bộ màu/chuỗi cả hai workspace, bỏ restart và traversal; kiểm tra EN/VI, contrast, control tạo trễ.
6. Compare đã triển khai ở phase 5. Navigator viewport, clipping overlay, inspector tabs và quick preview tiếp tục tách nhánh.
7. Pause/resume copy và checkpoint: cần định nghĩa việc xác minh đích sau khi dừng, đổi nguồn, ngắt ổ đĩa, tránh copy trùng khi Rename.

## Nhận xét về bản đánh giá

Các vấn đề Window embedding, WrapPanel thường, quét JPG lặp, theme hex và dịch bằng traversal có cơ sở trong mã nguồn.
Các điểm số và dự đoán freeze/OOM chưa phải benchmark đã đo. Không dùng chúng làm số liệu hiệu năng thực tế.
Copy song song không mặc nhiên nhanh hơn trên HDD/USB; ưu tiên đo byte/s trước khi đổi concurrency.
Baseline đã có source protection, temporary-file commit, hủy có cleanup, EXIF orientation, lựa chọn Original và test race khi điều hướng preview.
Đóng hộp chọn ngôn ngữ lần đầu là hủy khởi chạy; không cần ép thêm một hộp xác nhận khi chưa có công việc chưa lưu.

## Chạy và kiểm duyệt

```powershell
git switch phase-3-copy-feedback
dotnet run --project PhotoFileFilter.csproj -c Release
# Bộ kiểm tra dùng fixture tự tạo, không cần ảnh thật:
dotnet run --project Tests/PhotoFileFilter.Tests.csproj -c Release
```

Phase 1 so với main; phase 2 so với phase 1; phase 3 so với phase 2.
Kiểm tra ảnh RAW thật, codec máy bạn, ổ USB và thư mục lớn vẫn cần bạn duyệt trước khi gộp.
Không có benchmark thực tế cho 30–80 GB hoặc 8.000 ảnh trong đợt này.

## Phase 4 — phase-4-virtualized-grid

Đã triển khai grid pixel-scrolling với virtualized fixed-size tiles và filmstrip VirtualizingStackPanel.
Thanh chỉnh thumbnail 100–400 px, lưu qua lần mở app; điều hướng lên/xuống dùng số cột thực tế.
Thumbnail tải theo ô được hiện, không còn giới hạn 120 ảnh đầu; tối đa 2 decode thumbnail đồng thời và giữ 512 thumbnail từ hàng đợi.
Kiểm tra danh sách tổng hợp 8.000 mục: số container dưới 60 ở viewport 800×500, cuộn cuối, ScrollIntoView, selection, resize và thay collection.
Đây là kiểm tra container và fixture JPG, chưa phải benchmark codec 8.000 RAW thật.
Duyệt: bấm G, kéo thanh Cỡ ảnh dưới grid, cuộn và chọn nhiều ảnh; thử thư mục hơn 120 ảnh.

## Phase 5 — đã hủy: Compare

Nút C / So sánh mở hai ảnh cạnh nhau; chọn hai ảnh ở Grid để dùng đúng cặp đã chọn, hoặc ghim ảnh hiện tại.
Trái là ảnh ghim, phải là ảnh đang chấm; ←/→ chỉ đổi ảnh bên phải, bỏ qua ảnh ghim.
Con lăn và Space+kéo đồng bộ zoom/pan; G/E hoặc Esc thoát Compare. Có Ghim ảnh bên phải và Đổi hai bên.
Giữ zoom khi đổi ứng viên; giải phóng reference khi rời Compare; serialize RAW decode và hủy tác vụ cũ.
200 checks đạt, gồm race decode, pin/swap/navigation, thiếu ảnh, EN/VI và render Compare.
Zoom là tỷ lệ so với Fit của từng pane, không phải đối chiếu pixel 1:1 giữa hai ảnh khác kích thước. Cặp Compare chưa lưu qua lần đóng app (mở lại ở Loupe).

## Phase 6 — phase-6-quick-preview

Quick Preview của TXT Filter mở không modal, tái sử dụng một cửa sổ và nhận snapshot danh sách kết quả đang hiện.
←/→ duyệt ảnh, con lăn hoặc +/- zoom, kéo chuột pan, R xoay tạm thời, Ctrl+0 Fit, Esc đóng.
Zoom trên Fit nạp ảnh gốc; navigation hủy decode cũ và reset transform. Xoay không ghi vào ảnh gốc hay catalog Review.
Kiểm tra navigation, giới hạn snapshot theo search, reset zoom/rotation, race decode, đóng trong lúc tải và render ở 620×420.

## Bản kiểm duyệt 1.23.0

Nhánh hiện tại: phase 4 từ phase 3; phase 6 từ phase 4. Phase 5 đã hủy.
Bản máy và phase 6 chứa tất cả thay đổi. Main chưa gộp.
Các hạng mục còn lại: semantic theme / localization không restart, inspector tabs, navigator viewport, clipping overlay, filmstrip resize và Pause/Resume copy có checkpoint.

Kiểm chứng bản 1.23.0: 209 checks đạt ở Release, gồm cửa sổ preview modeless và tái sử dụng cửa sổ. Bộ screenshot nằm trong artifacts/screenshots.

## Điều chỉnh kiểm duyệt — phase-6-quick-preview — 1.23.1

Theo yêu cầu người dùng: bỏ Compare hoàn toàn và khôi phục Space chuyển Grid ↔ Loupe.
Phase 5 (Compare) đã hủy theo yêu cầu; không giữ nhánh riêng cho tính năng đã bỏ.
Space không đổi ảnh đang chọn, không lặp chuyển mode khi giữ phím, không chặn nhập Space trong TextBox.
Giữ các cải tiến phase 4 và Quick Preview phase 6. Các chỉnh sửa cập nhật trực tiếp trên phase-6-quick-preview, chưa gộp main.

## Quy tắc nhánh sau kiểm duyệt

Mỗi phase/nghiệp vụ một nhánh, không dùng tiền tố codex/. Sửa tiếp trong cùng nhánh, không tạo nhánh fix riêng. Nhánh bản kiểm duyệt mới nhất: phase-6-quick-preview. Main chỉ gộp sau khi người dùng duyệt.
