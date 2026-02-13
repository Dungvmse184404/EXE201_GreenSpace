# 🌱 Hướng dẫn cài đặt Google Gemini AI cho GreenSpace

## Tổng quan

GreenSpace sử dụng **Google Gemini AI** để chẩn đoán bệnh cây trồng từ hình ảnh. Hướng dẫn này sẽ giúp bạn lấy API key và cấu hình.

---

## 📝 Bước 1: Tạo tài khoản Google Cloud (nếu chưa có)

1. Truy cập: https://console.cloud.google.com/
2. Đăng nhập bằng tài khoản Google
3. Tạo project mới (nếu cần)

---

## 🔑 Bước 2: Lấy Gemini API Key

### Cách 1: Sử dụng Google AI Studio (Khuyến nghị - Dễ nhất)

1. Truy cập: **https://aistudio.google.com/app/apikey**

2. Đăng nhập bằng tài khoản Google

3. Click **"Create API Key"**

4. Chọn project hoặc tạo mới

5. Copy API key được tạo

> ⚠️ **Lưu ý:** API key này có **Free tier** với giới hạn:
> - 60 requests/phút
> - 1500 requests/ngày
> - Miễn phí hoàn toàn

### Cách 2: Sử dụng Google Cloud Console

1. Truy cập: https://console.cloud.google.com/apis/credentials

2. Click **"+ CREATE CREDENTIALS"** → **"API key"**

3. Copy API key

4. (Optional) Click **"Edit API key"** để giới hạn quyền truy cập

---

## ⚙️ Bước 3: Cấu hình trong GreenSpace

### Option A: Cấu hình trong appsettings.json

```json
{
  "Gemini": {
    "ApiKey": "YOUR_API_KEY_HERE",
    "Model": "gemini-1.5-flash",
    "BaseUrl": "https://generativelanguage.googleapis.com/v1beta",
    "MaxOutputTokens": 2048,
    "Temperature": 0.4,
    "IsEnabled": true
  }
}
```

### Option B: Sử dụng Environment Variable (Khuyến nghị cho Production)

**Windows (PowerShell):**
```powershell
$env:Gemini__ApiKey = "YOUR_API_KEY_HERE"
```

**Windows (CMD):**
```cmd
set Gemini__ApiKey=YOUR_API_KEY_HERE
```

**Linux/Mac:**
```bash
export Gemini__ApiKey="YOUR_API_KEY_HERE"
```

### Option C: Sử dụng User Secrets (Development)

```bash
cd src/GreenSpace.WebAPI
dotnet user-secrets set "Gemini:ApiKey" "YOUR_API_KEY_HERE"
```

---

## 🧪 Bước 4: Kiểm tra cấu hình

### Test API Status

```bash
curl http://localhost:5020/api/diagnosis/status
```

**Response khi thành công:**
```json
{
  "isAvailable": true,
  "message": "Dich vu chan doan AI dang hoat dong"
}
```

**Response khi chưa cấu hình:**
```json
{
  "isAvailable": false,
  "message": "Dich vu chan doan AI tam thoi khong kha dung"
}
```

### Test Diagnosis (với Postman hoặc cURL)

```bash
curl -X POST http://localhost:5020/api/diagnosis \
  -H "Content-Type: application/json" \
  -d '{
    "imageBase64": "data:image/jpeg;base64,/9j/4AAQ...",
    "description": "La cay bi vang",
    "language": "vi"
  }'
```

---

## 📊 Các Model có sẵn

| Model | Tốc độ | Chi phí | Ghi chú |
|-------|--------|---------|---------|
| `gemini-1.5-flash` | Nhanh nhất | Thấp nhất | **Khuyến nghị** |
| `gemini-1.5-pro` | Trung bình | Cao hơn | Chính xác hơn |
| `gemini-pro-vision` | Trung bình | Trung bình | Legacy |

---

## 💰 Chi phí ước tính

### Free Tier (Google AI Studio)
- **60 requests/phút**
- **1,500 requests/ngày**
- **Miễn phí hoàn toàn**

### Paid (sau khi vượt Free Tier)
- Input: $0.00025 / 1K tokens
- Output: $0.0005 / 1K tokens
- Trung bình: **~$0.002-0.005 / request**

---

## 🔒 Bảo mật

1. **KHÔNG commit API key vào Git**
   - Sử dụng `.gitignore` cho `appsettings.Development.json`
   - Sử dụng Environment Variables cho Production

2. **Giới hạn API key** (nếu dùng Google Cloud Console)
   - Restrict to specific APIs: Generative Language API
   - Restrict to specific IPs/Domains

3. **Monitor usage**
   - Theo dõi tại: https://console.cloud.google.com/apis/dashboard

---

## ❓ Xử lý lỗi thường gặp

### Lỗi: "API key not valid"
- Kiểm tra API key đã copy đúng chưa
- Đảm bảo API key không có khoảng trắng thừa

### Lỗi: "Quota exceeded"
- Đã vượt giới hạn free tier
- Chờ 24h hoặc upgrade plan

### Lỗi: "Model not found"
- Kiểm tra tên model trong config
- Sử dụng `gemini-1.5-flash` (mặc định)

### Service trả về "unavailable"
- Kiểm tra `IsEnabled` = `true` trong config
- Kiểm tra API key đã được set

---

## 📞 Hỗ trợ

- **Google AI Studio**: https://aistudio.google.com/
- **Gemini API Docs**: https://ai.google.dev/docs
- **Pricing**: https://ai.google.dev/pricing

---

*Cập nhật: 2024*
