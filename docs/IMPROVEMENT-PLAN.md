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
Kiểm chứng: 172 checks; cache tối đa 5 ảnh / 192 MiB (ước tính bộ nhớ bitmap), index tối đa 16 thư mục, TTL 2 giây. Không cache kết quả lỗi. Không phải giới hạn tổng RAM của app hay benchmark thư mục 8.000 ảnh.
Duyệt tay: qua lại ảnh lớn, đổi chất lượng, sửa/xóa JPG đồng hành, thư mục trên ổ ngoài.

## Phase 3 — codex/phase-3-copy-feedback

Tiến độ theo byte, tốc độ truyền và thời gian còn lại; giữ copy tuần tự và commit file an toàn.
Cải thiện một số thao tác UI nhỏ, kiểm tra lại toàn bộ workflow và build Release.
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
