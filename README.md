# ASPNET-dk24ttc7-maixuantri-ShopGiay

Dự án Website bán giày được xây dựng trên nền tảng ASP.NET. Tài liệu này hướng dẫn chi tiết các bước để thiết lập và khởi chạy dự án trên môi trường local.

## Thông tin tác giả
- **Họ và tên:** Mai Xuân Trí
- **Email:** xuantri2000@gmail.com
- **Số điện thoại:** 0835847062
- **Lớp:** DK24TTC7

---

## Yêu cầu hệ thống & Công cụ
1. **Framework:** .NET Framework 4.7.2
2. **IDE:** Visual Studio Community 2022/2026 (Đảm bảo đã cài đặt thành phần **NuGet Package Manager**).
   - [Tải Visual Studio](https://visualstudio.microsoft.com/downloads/)
3. **Database:** - **SQL Express:** Hệ quản trị cơ sở dữ liệu nhẹ cho môi trường học tập.
     - [Tải SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
   - **SQL Server Management Studio (SSMS):** Công cụ quản lý và truy vấn giao diện DB.
     - [Tải SSMS](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)

---

## Hướng dẫn chạy dự án
### 1. Mở dự án
Mở tệp tin có đuôi `.slnx` trực tiếp bằng **Visual Studio Community**.

### 2. Phục hồi các gói thư viện (NuGet)
Trong cửa sổ **Solution Explorer**, nhấp chuột phải vào **Solution** (dòng đầu tiên) và chọn **Restore NuGet Packages**. Việc này sẽ tự động tải về các gói thư viện cần thiết.

### 3. Mở bảng điều khiển quản lý gói
Truy cập thử NuGet: `Tools` -> `NuGet Package Manager` -> `Package Manager Console`.

### 4. Cấu hình Connection String
Mở tệp `web.config`. Tìm đến phần `<connectionStrings>` và chỉnh sửa lại đường dẫn kết nối phù hợp với instance SQL Express trên máy của bạn.
*Lưu ý:* Thường là `Data Source=.\SQLEXPRESS;Initial Catalog=Ten_DB;Integrated Security=True`.

### 5. Khởi tạo dữ liệu (Migration)
Tại cửa sổ **Package Manager Console** đã mở tại bước 3, nhập dòng lệnh sau và nhấn Enter:
```powershell
Update-Database