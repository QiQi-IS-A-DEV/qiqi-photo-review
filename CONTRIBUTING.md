# Đóng góp cho QiQi Photo Review

Cảm ơn bạn đã quan tâm đến dự án. Hãy đọc [cấu trúc và quyền sở hữu mã nguồn](docs/PROJECT-STRUCTURE.md) trước khi sửa code.

## Thiết lập môi trường

- Windows 10/11 x64.
- .NET 8 SDK hoặc Visual Studio với workload **.NET desktop development**.

```powershell
git clone https://github.com/QiQi-IS-A-DEV/qiqi-photo-review.git
cd qiqi-photo-review
dotnet build PhotoFileFilter.sln -c Release
dotnet run --project tests/PhotoFileFilter.Tests/PhotoFileFilter.Tests.csproj -c Release
```

Chạy ứng dụng bằng:

```powershell
dotnet run --project src/PhotoFileFilter.App/PhotoFileFilter.App.csproj
```

## Quy ước nhánh

- Mỗi phase hoặc một nhóm thay đổi cùng nghiệp vụ dùng một nhánh.
- Đặt tên ngắn, mô tả đúng phạm vi, ví dụ `phase-7-project-structure`.
- Không tạo nhánh sửa lẻ khi thay đổi vẫn thuộc phase đang làm.
- Chỉ gộp vào `main` sau khi build, test và kiểm duyệt hành vi liên quan.

## Chọn đúng vị trí sửa

- Review ảnh: `src/PhotoFileFilter.App/Features/Review`.
- Lọc và sao chép theo TXT: `src/PhotoFileFilter.App/Features/TxtFilter`.
- Onboarding: `src/PhotoFileFilter.App/Features/Onboarding`.
- Thành phần thực sự dùng chung: `src/PhotoFileFilter.App/Shared`.
- Thuật toán file không phụ thuộc WPF: `src/PhotoFileFilter.Core`.
- Kiểm thử: `tests/PhotoFileFilter.Tests`.

Không đưa code của một feature vào `Shared` chỉ để tránh tạo namespace. `Shared` là API chung của ứng dụng, không phải nơi chứa file chưa biết đặt ở đâu.

## Trước khi gửi pull request

```powershell
dotnet build PhotoFileFilter.sln -c Release
dotnet run --project tests/PhotoFileFilter.Tests/PhotoFileFilter.Tests.csproj -c Release
```

Pull request cần mô tả vấn đề, hành vi sau thay đổi và cách đã kiểm chứng. Nếu đổi cấu trúc, phím tắt hoặc workflow người dùng, hãy cập nhật README và tài liệu liên quan.

Không commit `artifacts`, `bin`, `obj`, dữ liệu ảnh cá nhân hoặc đường dẫn máy cục bộ.

## Bản quyền và giấy phép

Copyright © 2026 QiQi Studio (QiQi-IS-A-DEV). Giấy phép sử dụng mã nguồn sẽ được công bố trong file `LICENSE` của repository.
