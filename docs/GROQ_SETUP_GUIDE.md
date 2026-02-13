# Groq API Setup Guide

## Giới thiệu
Groq cung cấp API miễn phí với tốc độ inference cực nhanh cho các model AI như Llama 3.2 Vision.

## Các bước lấy API Key

### Bước 1: Tạo tài khoản Groq
1. Truy cập https://console.groq.com
2. Đăng ký tài khoản (có thể dùng Google/GitHub)

### Bước 2: Tạo API Key
1. Sau khi đăng nhập, vào **API Keys** từ menu bên trái
2. Hoặc truy cập trực tiếp: https://console.groq.com/keys
3. Click **Create API Key**
4. Đặt tên cho key (ví dụ: "GreenSpace-Diagnosis")
5. Copy API key (chỉ hiển thị 1 lần!)

### Bước 3: Cấu hình trong appsettings.json
```json
"Groq": {
    "ApiKey": "gsk_xxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "Model": "llama-3.2-11b-vision-preview",
    "BaseUrl": "https://api.groq.com/openai/v1",
    "MaxTokens": 2048,
    "Temperature": 0.4,
    "IsEnabled": true
}
```

## Các Model Vision có sẵn

| Model | Mô tả | Tốc độ |
|-------|-------|--------|
| `llama-3.2-11b-vision-preview` | Llama 3.2 11B Vision (khuyến nghị) | Nhanh |
| `llama-3.2-90b-vision-preview` | Llama 3.2 90B Vision (chính xác hơn) | Chậm hơn |

## Rate Limits (Free Tier)
- **Requests per minute:** 30
- **Requests per day:** 14,400
- **Tokens per minute:** 18,000
- **Tokens per day:** 500,000

## So sánh với Gemini

| Tiêu chí | Groq | Gemini |
|----------|------|--------|
| Miễn phí | ✅ Có giới hạn | ✅ Có giới hạn |
| Tốc độ | ⚡ Rất nhanh | Trung bình |
| Vision | ✅ Llama 3.2 Vision | ✅ Gemini Vision |
| Rate limit | 30 req/min | 15 req/min |

## Test API
```bash
curl -X POST http://localhost:5020/api/diagnosis \
  -H "Content-Type: application/json" \
  -d '{
    "description": "Cay trau ba bi vang la",
    "language": "vi"
  }'
```

## Chuyển đổi giữa Groq và Gemini

Trong `DependencyInjection.cs`, thay đổi:

**Dùng Groq:**
```csharp
services.AddHttpClient<IAIVisionService, GroqService>();
```

**Dùng Gemini:**
```csharp
services.AddHttpClient<IAIVisionService, GeminiService>();
```

## Troubleshooting

### Lỗi 401 Unauthorized
- Kiểm tra API key đã đúng chưa
- API key phải bắt đầu bằng `gsk_`

### Lỗi 429 Rate Limit
- Đã vượt quá giới hạn request
- Chờ 1 phút hoặc nâng cấp plan

### Lỗi 400 Bad Request
- Kiểm tra format request
- Đảm bảo image base64 hợp lệ
