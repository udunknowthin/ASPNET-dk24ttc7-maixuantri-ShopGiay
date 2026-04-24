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
3. **Database:** (Tùy chọn, nếu muốn dùng LocalDB mặc định của Visual Studio ở bước 3 thì không cần cài SQL Express và SSMS)
   - **SQL Express:** Hệ quản trị cơ sở dữ liệu nhẹ cho môi trường học tập.
     - [Tải SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
   - **SQL Server Management Studio (SSMS):** Công cụ quản lý và truy vấn giao diện DB.
     - [Tải SSMS](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
---

## Hướng dẫn chạy dự án

### Bước 1: Mở dự án
Mở file có đuôi `.slnx` nằm trong folder `./src/ShopGiay` trực tiếp bằng **Visual Studio**.

### Bước 2: Phục hồi các gói thư viện (NuGet)
Trong cửa sổ **Solution Explorer**, nhấp chuột phải vào **Solution** (dòng trên cùng) và chọn **Restore NuGet Packages**.

### Bước 3: Tạo cơ sở dữ liệu (Database)
Chọn **1 trong 2 cách** sau tùy môi trường bạn sử dụng:

**Trường hợp 1: Dùng SQL Server Express (SQLEXPRESS)**
- Mở **SSMS** và kết nối tới `SQLEXPRESS`.
- Chạy lệnh sau để tạo Database:
```
CREATE DATABASE ShopGiay;
GO
```

**Trường hợp 2: Dùng LocalDB (mặc định của Visual Studio)**
- Mở **SQL Server Object Explorer**
  - Vào menu **View** → chọn **SQL Server Object Explorer** (hoặc nhấn `Ctrl + \ , Ctrl + S`)
- Kết nối LocalDB
  - Trong cửa sổ **SQL Server Object Explorer**, mở rộng mục **SQL Server**
  - Tìm dòng **(localdb)\MSSQLLocalDB** → nhấp vào để kết nối
  - Nếu chưa thấy, nhấn **Add SQL Server** (biểu tượng ổ cắm) → nhập `(LocalDB)\MSSQLLocalDB` → nhấn **Connect**
- Tạo Database
  - Chuột phải vào **Databases** → chọn **Add New Database**
  - Đặt tên database là **ShopGiay** → nhấn **OK**

### Bước 4: Cấu hình Connection String
Mở file `web.config` tại thư mục `./src/ShopGiay`. Chỉnh sửa thẻ `<connectionStrings>` khớp với máy của bạn:
```xml
<connectionStrings>
  <add name="DefaultConnection" connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=<Tên DB vừa tạo>;Integrated Security=True;MultipleActiveResultSets=True" providerName="System.Data.SqlClient" />
</connectionStrings>
```

Ví dụ DB vừa tạo trên là ShopGiay:

- Trường hợp 1, sử dụng SQLEXPRESS:
```xml
<connectionStrings>
  <add name="DefaultConnection" connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=ShopGiay;Integrated Security=True;MultipleActiveResultSets=True" providerName="System.Data.SqlClient" />
</connectionStrings>
```

- Trường hợp 2, dùng LocalDB của Visual Studio:
```xml
<connectionStrings>
  <add name="DefaultConnection"
       connectionString="Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=ShopGiay;Integrated Security=True;MultipleActiveResultSets=True"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

### Bước 5: Mở bảng điều khiển quản lý gói
Trên thanh menu, chọn `Tools` -> `NuGet Package Manager` -> `Package Manager Console`.

### Bước 6: Khởi tạo dữ liệu (Migration)
Tại cửa sổ **Package Manager Console**, nhập lệnh sau và nhấn Enter:
```powershell
Update-Database
```

### Bước 7: Build lại dự án
Nhấp chuột phải vào **Solution** -> chọn **Clean Solution**. Sau đó nhấp chuột phải lần nữa -> chọn **Rebuild Solution**.

### Bước 8: Chạy dự án
- Nhấn tổ hợp phím `Ctrl + F5` để chạy dự án trên trình duyệt.
- Lần đầu chạy, Visual Studio sẽ yêu cầu tạo và tin cậy (trust) chứng chỉ SSL cho HTTPS.
- Chọn Yes để hệ thống tự động tạo chứng chỉ cho IIS Express.
- Nếu trình duyệt cảnh báo bảo mật:
  - Trên Google Chrome, Microsoft Edge:
    Khi hiện trang “Your connection is not private” -> Nhấn Advanced (Nâng cao) -> Chọn Proceed to localhost (unsafe) để tiếp tục
  - Trên Mozilla Firefox:
    Nhấn Advanced (Nâng cao) -> Chọn Accept the Risk and Continue để truy cập
- Sau khi hoàn tất, ứng dụng sẽ chạy với địa chỉ dạng https://localhost:xxxx

---

## 🛠 Khắc phục sự cố (Troubleshooting)

**Bước 9: Nếu gặp lỗi không build được dự án**
Mở **Package Manager Console** và chạy lệnh sau để cài đặt bổ sung trình biên dịch:
```powershell
Install-Package Microsoft.CodeDom.Providers.DotNetCompilerPlatform