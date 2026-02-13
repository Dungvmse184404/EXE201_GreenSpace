# GreenSpace API Documentation

> **Base URL:** `/api`
> **Authentication:** Bearer Token (JWT)

---

## 📋 Table of Contents

1. [Authentication (Auth)](#1-authentication-auth)
2. [Users](#2-users)
3. [User Addresses](#3-user-addresses)
4. [Products](#4-products)
5. [Product Variants](#5-product-variants)
6. [Categories](#6-categories)
7. [Cart](#7-cart)
8. [Orders](#8-orders)
9. [Payments](#9-payments)
10. [Ratings](#10-ratings)
11. [Promotions](#11-promotions)
12. [Reports (Admin)](#12-reports-admin)

---

## 1. Authentication (Auth)

### 1.1 Register Flow

#### Step 1: Initiate Registration
```
POST /api/auth/register/initiate
```
**Auth:** None
**Request Body:**
```json
{
  "email": "user@example.com"  // Required, valid email format
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | OTP sent successfully |
| 400 | Invalid email format / Email already registered |

---

#### Step 2: Verify OTP
```
POST /api/auth/register/verify
```
**Auth:** None
**Request Body:**
```json
{
  "email": "user@example.com",  // Required, valid email
  "otp": "123456"               // Required, 6 digits
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | OTP verified successfully |
| 400 | Invalid/expired OTP |

---

#### Step 3: Finalize Registration
```
POST /api/auth/register/finalize
```
**Auth:** None
**Request Body:**
```json
{
  "email": "user@example.com",      // Required, must be verified
  "password": "password123",        // Required, min 6 characters
  "firstName": "Nguyen",            // Optional
  "lastName": "Van A",              // Optional
  "phoneNumber": "0901234567",      // Optional, valid phone format, max 11 chars
  "address": "123 Nguyen Hue, Q1"   // Optional, max 256 chars
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Registration successful |
| 400 | Email not verified / Invalid data |

---

#### Resend OTP
```
POST /api/auth/register/resend
```
Same as `register/initiate`

---

### 1.2 Login
```
POST /api/auth/login
```
**Auth:** None
**Request Body:**
```json
{
  "email": "user@example.com",  // Required, valid email
  "password": "password123"     // Required, min 6 characters
}
```
**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "userId": "guid",
    "email": "user@example.com",
    "fullName": "Nguyen Van A",
    "role": "CUSTOMER",
    "accessToken": "eyJ...",
    "refreshToken": "abc123...",
    "expiresAt": "2024-01-15T12:00:00Z"
  }
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Login successful |
| 401 | Invalid credentials |

---

### 1.3 Refresh Token
```
POST /api/auth/refresh
```
**Auth:** None
**Request Body:**
```json
{
  "refreshToken": "abc123...",
  "accessToken": "eyJ..."
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | New tokens issued |
| 401 | Invalid/expired tokens |

---

### 1.4 Revoke Token (Logout)
```
POST /api/auth/revoke
```
**Auth:** Required
**Request Body:**
```json
{
  "refreshToken": "abc123..."
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Token revoked |
| 400 | Invalid token |

---

### 1.5 Forgot Password Flow

#### Step 1: Request Reset
```
POST /api/auth/password/forgot
```
**Request Body:**
```json
{
  "email": "user@example.com"  // Required, must exist in system
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | OTP sent |
| 400 | Email not found |

---

#### Step 2: Verify Reset OTP
```
POST /api/auth/password/verify
```
**Request Body:**
```json
{
  "email": "user@example.com",
  "otp": "123456"  // 6 digits exactly
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | OTP verified (valid for 15 mins) |
| 400 | Invalid/expired OTP |

---

#### Step 3: Reset Password
```
POST /api/auth/password/reset
```
**Request Body:**
```json
{
  "email": "user@example.com",
  "newPassword": "newpass123",      // Min 6 characters
  "confirmPassword": "newpass123"   // Must match newPassword
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Password reset successful |
| 400 | OTP not verified / Passwords don't match |

---

### 1.6 Create Internal User (Admin Only)
```
POST /api/auth/internal-user
```
**Auth:** ADMIN
**Request Body:**
```json
{
  "email": "staff@greenspace.com",  // Required, valid email
  "password": "password123",        // Required, min 6 chars
  "firstName": "Tran",              // Required, max 128 chars
  "lastName": "Van B",              // Required, max 128 chars
  "phoneNumber": "0909876543",      // Optional, valid phone, max 32 chars
  "address": "456 Le Loi, Q3",      // Optional, max 256 chars
  "birthday": "1990-05-15",         // Optional, format: YYYY-MM-DD
  "role": "STAFF"                   // Required: "STAFF", "MANAGER", "ADMIN"
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | User created |
| 400 | Invalid data / Email exists |

---

## 2. Users

### 2.1 Get All Users (Admin)
```
GET /api/users
```
**Auth:** ADMIN
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | List of users |
| 400 | Error |

---

### 2.2 Get Current User
```
GET /api/users/me
```
**Auth:** Required
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | User profile |
| 404 | User not found |

---

### 2.3 Get User by ID (Admin)
```
GET /api/users/{id}
```
**Auth:** ADMIN
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | User data |
| 404 | Not found |

---

### 2.4 Update User
```
PUT /api/users/{id}
```
**Auth:** Owner or ADMIN
**Request Body:**
```json
{
  "email": "newemail@example.com",  // Optional, valid email
  "fullName": "New Name",           // Optional
  "phoneNumber": "0901234567",      // Optional
  "editAddress": "New address",     // Optional
  "additionalAddress": "Extra",     // Optional
  "status": "Active",               // Optional
  "isActive": true                  // Default: true
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Updated |
| 400 | Invalid data |
| 403 | Forbidden (not owner/admin) |

---

### 2.5 Deactivate User (Admin)
```
DELETE /api/users/{id}
```
**Auth:** ADMIN
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Deactivated |
| 400 | Error |

---

## 3. User Addresses

### 3.1 Get My Addresses
```
GET /api/users/me/addresses
```
**Auth:** Required
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | List of addresses |
| 400 | Error |

---

### 3.2 Get Default Address
```
GET /api/users/me/addresses/default
```
**Auth:** Required
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Default address |
| 404 | No default address |

---

### 3.3 Get Address by ID
```
GET /api/users/me/addresses/{addressId}
```
**Auth:** Required
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Address data |
| 404 | Not found |

---

### 3.4 Create Address
```
POST /api/users/me/addresses
```
**Auth:** Required
**Request Body:**
```json
{
  "province": "TP. Hồ Chí Minh",           // Required, max 100 chars
                                            // Hint: Tỉnh/Thành phố (VD: "Hà Nội", "TP. Hồ Chí Minh")

  "district": "Quận 1",                     // Required, max 100 chars
                                            // Hint: Quận/Huyện (VD: "Quận 1", "Quận Bình Thạnh")

  "ward": "Phường Bến Nghé",                // Required, max 100 chars
                                            // Hint: Phường/Xã (VD: "Phường Bến Nghé", "Phường 25")

  "streetAddress": "123 Nguyễn Huệ, Tòa ABC, Tầng 5",  // Required, max 255 chars
                                            // Hint: Số nhà, tên đường, tòa nhà, tầng...

  "label": "Công ty",                       // Optional, max 50 chars
                                            // Hint: Nhãn phân biệt (VD: "Nhà", "Công ty", "Nhà bố mẹ")

  "isDefault": true                         // Optional, default: false
                                            // Hint: Đặt làm địa chỉ mặc định
}
```
**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "addressId": "guid",
    "province": "TP. Hồ Chí Minh",
    "district": "Quận 1",
    "ward": "Phường Bến Nghé",
    "streetAddress": "123 Nguyễn Huệ, Tòa ABC, Tầng 5",
    "fullAddress": "123 Nguyễn Huệ, Tòa ABC, Tầng 5, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh",
    "label": "Công ty",
    "isDefault": true,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": null
  }
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 201 | Created |
| 400 | Invalid data |

---

### 3.5 Update Address
```
PUT /api/users/me/addresses/{addressId}
```
**Auth:** Required
**Request Body:** Same as Create
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Updated |
| 400 | Invalid data / Not found |

---

### 3.6 Delete Address
```
DELETE /api/users/me/addresses/{addressId}
```
**Auth:** Required
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Deleted |
| 400 | Not found |

---

### 3.7 Set Default Address
```
PUT /api/users/me/addresses/{addressId}/default
```
**Auth:** Required
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Set as default |
| 400 | Not found |

---

## 4. Products

### 4.1 Get All Products
```
GET /api/products
```
**Auth:** None
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | List of products |
| 400 | Error |

---

### 4.2 Get Product by ID
```
GET /api/products/{id}
```
**Auth:** None
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Product data |
| 404 | Not found |

---

### 4.3 Create Product (Admin/Staff)
```
POST /api/products
```
**Auth:** ADMIN, STAFF
**Request Body:**
```json
{
  "name": "Cây xanh mini",           // Required, max 255 chars
                                      // Hint: Tên sản phẩm

  "description": "Cây xanh để bàn",  // Optional
                                      // Hint: Mô tả chi tiết sản phẩm

  "basePrice": 150000,               // Required, >= 0
                                      // Hint: Giá gốc (VND)

  "thumbnailUrl": "https://...",     // Optional
                                      // Hint: URL ảnh đại diện

  "categoryId": 1,                   // Required
                                      // Hint: ID danh mục sản phẩm

  "brandId": 1                       // Optional
                                      // Hint: ID thương hiệu
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 201 | Created |
| 400 | Invalid data |

---

### 4.4 Update Product (Admin/Staff)
```
PUT /api/products/{id}
```
**Auth:** ADMIN, STAFF
**Request Body:** Same as Create
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Updated |
| 400 | Invalid data |

---

### 4.5 Delete Product (Admin)
```
DELETE /api/products/{id}
```
**Auth:** ADMIN
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Deleted |
| 400 | Error |

---

## 5. Product Variants

**Base URL:** `/api/products/{productId}/variants`

### 5.1 Get Variants by Product
```
GET /api/products/{productId}/variants
```
**Auth:** None
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | List of variants |
| 400 | Error |

---

### 5.2 Get Variant by ID
```
GET /api/products/{productId}/variants/{variantId}
```
**Auth:** None
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Variant data |
| 404 | Not found |

---

### 5.3 Create Variant (Admin/Staff)
```
POST /api/products/{productId}/variants
```
**Auth:** ADMIN, STAFF
**Request Body:**
```json
{
  "productId": "guid",           // Required, must match URL
                                  // Hint: ID sản phẩm

  "sku": "GREEN-001-S",          // Required, max 100 chars
                                  // Hint: Mã SKU unique (VD: "PLANT-001-GREEN")

  "price": 180000,               // Required, >= 0
                                  // Hint: Giá bán (VND)

  "stockQuantity": 50,           // Required, >= 0
                                  // Hint: Số lượng tồn kho

  "imageUrl": "https://...",     // Optional, max 255 chars
                                  // Hint: URL ảnh variant

  "color": "Green",              // Optional, max 50 chars
                                  // Hint: Màu sắc (VD: "Xanh lá", "Đỏ")

  "sizeOrModel": "Small"         // Optional, max 50 chars
                                  // Hint: Size/Model (VD: "S", "M", "L", "XL")
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 201 | Created |
| 400 | Invalid data / Product ID mismatch |

---

### 5.4 Update Variant (Admin/Staff)
```
PUT /api/products/{productId}/variants/{variantId}
```
**Auth:** ADMIN, STAFF
**Request Body:** Same fields (except productId)
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Updated |
| 400 | Invalid data |

---

### 5.5 Update Stock (Admin/Staff)
```
PATCH /api/products/{productId}/variants/{variantId}/stock
```
**Auth:** ADMIN, STAFF
**Request Body:**
```json
{
  "quantity": 100  // New stock quantity
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Stock updated |
| 400 | Error |

---

### 5.6 Delete Variant (Admin)
```
DELETE /api/products/{productId}/variants/{variantId}
```
**Auth:** ADMIN
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Deleted |
| 400 | Error |

---

## 6. Categories

### 6.1 Get All Categories
```
GET /api/categories
```
**Auth:** None
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | List of categories |
| 400 | Error |

---

### 6.2 Get Category by ID
```
GET /api/categories/{id}
```
**Auth:** None
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Category data |
| 404 | Not found |

---

### 6.3 Create Category (Admin/Staff)
```
POST /api/categories
```
**Auth:** ADMIN, STAFF
**Request Body:**
```json
{
  "name": "Cây cảnh",           // Required, max 255 chars
                                 // Hint: Tên danh mục

  "slug": "cay-canh",           // Optional, max 255 chars
                                 // Hint: URL slug (tự động tạo nếu bỏ trống)

  "parentId": "guid"            // Optional
                                 // Hint: ID danh mục cha (cho danh mục con)
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 201 | Created |
| 400 | Invalid data |

---

### 6.4 Update Category (Admin/Staff)
```
PUT /api/categories/{id}
```
**Auth:** ADMIN, STAFF
**Request Body:** Same as Create
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Updated |
| 400 | Invalid data |

---

### 6.5 Delete Category (Admin)
```
DELETE /api/categories/{id}
```
**Auth:** ADMIN
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Deleted |
| 400 | Error |

---

## 7. Cart

### 7.1 Get My Cart
```
GET /api/carts
```
**Auth:** Required
**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "cartId": "guid",
    "items": [
      {
        "cartItemId": "guid",
        "variantId": "guid",
        "productName": "Cây xanh mini",
        "quantity": 2,
        "price": 150000,
        "subTotal": 300000
      }
    ],
    "totalAmount": 300000
  }
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Cart data |
| 400 | Error |

---

### 7.2 Add Item to Cart
```
POST /api/carts/items
```
**Auth:** Required
**Request Body:**
```json
{
  "variantId": "guid",  // Required
                         // Hint: ID của product variant muốn thêm

  "quantity": 2         // Required, >= 1
                         // Hint: Số lượng (mặc định: 1)
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Item added |
| 400 | Invalid variant / Out of stock |

---

### 7.3 Remove Item from Cart
```
DELETE /api/carts/items
```
**Auth:** Required
**Request Body:**
```json
{
  "variantId": "guid",  // Required
  "quantity": 1         // Number to remove
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Item removed |
| 400 | Error |

---

### 7.4 Clear Cart
```
DELETE /api/carts
```
**Auth:** Required
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Cart cleared |
| 400 | Error |

---

## 8. Orders

### 8.1 Get My Orders
```
GET /api/orders/my-orders
```
**Auth:** Required
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | List of orders |
| 401 | Unauthorized |

---

### 8.2 Get Order by ID
```
GET /api/orders/{id}
```
**Auth:** Required
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Order data |
| 404 | Not found |

---

### 8.3 Create Order
```
POST /api/orders
```
**Auth:** Required
**Request Body:**
```json
{
  "addressId": "guid",              // Optional (Default: nếu có sẵn is_default trong userAddress)
                                     // Hint: ID địa chỉ đã lưu (nếu dùng địa chỉ có sẵn)

  "shippingAddress": "123 ABC...",  // Optional (bắt buộc nếu không có addressId)
                                     // Hint: Địa chỉ giao hàng nhập tay

  "recipientName": "Nguyen Van A",  // Optional, max 100 chars (Default: người đang đăng nhập)
                                     // Hint: Tên người nhận hàng

  "recipientPhone": "0901234567",   // Optional, max 20 chars (Default:  người đang đăng nhập)
                                     // Hint: SĐT người nhận hàng

  "paymentMethod": "COD",           // Optional (tự điền sau khi thanh toán)
                                     // Hint: Phương thức thanh toán: "COD", "VNPAY", "PAYOS" ()

  "voucherCode": "SALE20",          // Optional, max 50 chars
                                     // Hint: Mã giảm giá (nếu có)

  "note": "Giao giờ hành chính",    // Optional, max 500 chars
                                     // Hint: Ghi chú cho đơn hàng

  "items": [                         // Required
    {
      "variantId": "guid",           // Required - ID variant sản phẩm
      "quantity": 2                  // Required - Số lượng mua
    },{
      "variantId": "guid",           // Required - ID variant sản phẩm
      "quantity": 2                  // Required - Số lượng mua
    }
  ]
}
```
**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "orderId": "guid",
    "createdAt": "2024-01-15T10:30:00Z",
    "status": "Pending",
    "subTotal": 300000,
    "discount": 60000,
    "voucherCode": "SALE20",
    "shippingFee": 30000,
    "finalAmount": 270000,
    "totalAmount": 270000,
    "shippingAddressId": "guid",
    "shippingAddress": "123 Nguyễn Huệ, P.Bến Nghé, Q1, TP.HCM",
    "recipientName": "Nguyen Van A",
    "recipientPhone": "0901234567",
    "note": "Giao giờ hành chính",
    "paymentMethod": "COD",
    "paymentStatus": "Pending",
    "items": [...]
  }
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 201 | Order created |
| 400 | Invalid data / Out of stock / Invalid voucher |

---

### 8.4 Update Order Status (Admin/Staff)
```
PATCH /api/orders/{id}/status
```
**Auth:** ADMIN, STAFF
**Request Body:**
```json
{
  "status": "Shipped"  // "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled"
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Status updated |
| 400 | Invalid status transition |
| 403 | Forbidden |

---

## 9. Payments

### 9.1 Create VNPay Payment
```
POST /api/payments/vnpay/create
```
**Auth:** Required
**Request Body:**
```json
{
  "orderId": "guid",                     // Required
                                          // Hint: ID đơn hàng cần thanh toán

  "amount": 270000,                      // Required, >= 1000
                                          // Hint: Số tiền (VND, tối thiểu 1,000đ)

  "orderDescription": "Thanh toan GreenSpace",  // Optional, max 255 chars
                                          // Hint: Mô tả đơn hàng

  "bankCode": "NCB"                      // Optional, max 50 chars
                                          // Hint: Mã ngân hàng (để trống = chọn ở VNPay)
}
```
**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "paymentUrl": "https://sandbox.vnpayment.vn/..."
  }
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Payment URL generated |
| 400 | Invalid data |

---

### 9.2 Create PayOS Payment
```
POST /api/payments/payos/create
```
**Auth:** Required
**Request Body:**
```json
{
  "orderId": "guid",                    // Required
                                         // Hint: ID đơn hàng cần thanh toán

  "description": "Thanh toan GreenSpace"  // Optional, max 255 chars
                                         // Hint: Mô tả thanh toán
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Payment link created |
| 400 | Invalid data |

---

### 9.3 Get Payment by ID
```
GET /api/payments/{id}
```
**Auth:** Required
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Payment data |
| 404 | Not found |

---

### 9.4 Get Payments by Order
```
GET /api/payments/order/{orderId}
```
**Auth:** Required
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | List of payments |
| 400 | Error |

---

## 10. Ratings

### 10.1 Get Ratings by Product
```
GET /api/ratings/product/{productId}
```
**Auth:** None
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | List of ratings |
| 400 | Error |

---

### 10.2 Get Average Rating
```
GET /api/ratings/product/{productId}/average
```
**Auth:** None
**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "averageRating": 4.5,
    "totalRatings": 25
  }
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Average data |
| 400 | Error |

---

### 10.3 Get My Ratings
```
GET /api/ratings/my-ratings
```
**Auth:** Required
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | List of user's ratings |
| 400 | Error |

---

### 10.4 Get Rating by ID
```
GET /api/ratings/{id}
```
**Auth:** None
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Rating data |
| 404 | Not found |

---

### 10.5 Create Rating
```
POST /api/ratings
```
**Auth:** Required
**Request Body:**
```json
{
  "productId": "guid",         // Required
                                // Hint: ID sản phẩm cần đánh giá

  "stars": 5,                  // Required, range 1-5
                                // Hint: Số sao (1-5)

  "comment": "Sản phẩm tốt!"   // Optional, max 1000 chars
                                // Hint: Nhận xét (tùy chọn)
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 201 | Rating created |
| 400 | Invalid data / Already rated |

---

### 10.6 Update Rating
```
PUT /api/ratings/{id}
```
**Auth:** Required (owner only)
**Request Body:**
```json
{
  "stars": 4,                    // Required, range 1-5
  "comment": "Updated comment"   // Optional, max 1000 chars
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Updated |
| 400 | Invalid data / Not owner |

---

### 10.7 Delete Rating
```
DELETE /api/ratings/{id}
```
**Auth:** Required (owner only)
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Deleted |
| 400 | Not owner |

---

## 11. Promotions

### 11.1 Get Active Promotions
```
GET /api/promotions/active
```
**Auth:** None
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | List of active promotions |
| 400 | Error |

---

### 11.2 Create Promotion (Admin)
```
POST /api/promotions
```
**Auth:** ADMIN
**Request Body:**
```json
{
  "code": "SALE20",              // Required, max 50 chars, unique
                                  // Hint: Mã voucher (VD: "SALE20", "NEWUSER")

  "name": "Giảm 20%",            // Optional, max 100 chars
                                  // Hint: Tên/Mô tả ngắn

  "description": "Giảm 20% tối đa 50k cho đơn từ 100k",  // Optional, max 500 chars
                                  // Hint: Mô tả chi tiết

  "discountType": "Percentage",  // Required: "Percentage" hoặc "Fixed"
                                  // Hint: Loại giảm giá

  "discountValue": 20,           // Required, >= 0
                                  // Hint: Giá trị giảm (20 = 20% hoặc 20,000đ tùy type)

  "maxDiscount": 50000,          // Optional (dùng cho Percentage)
                                  // Hint: Giảm tối đa (VND)

  "minOrderValue": 100000,       // Optional
                                  // Hint: Giá trị đơn tối thiểu (VND)

  "maxUsage": 100,               // Optional (null = không giới hạn)
                                  // Hint: Số lần sử dụng tối đa

  "startDate": "2024-01-01",     // Optional
                                  // Hint: Ngày bắt đầu

  "endDate": "2024-12-31",       // Optional
                                  // Hint: Ngày kết thúc

  "discountAmount": 0            // Backward compatible, ignore
}
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Created |
| 400 | Invalid data / Code exists |

---

## 12. Reports (Admin)

**Auth:** ADMIN for all endpoints

### Common Query Parameters
| Parameter | Type | Description | Default |
|-----------|------|-------------|---------|
| fromDate | DateTime | Ngày bắt đầu | 1 tháng trước |
| toDate | DateTime | Ngày kết thúc | Hiện tại |
| groupBy | Enum | Day, Week, Month, Year | Day |

---

### 12.1 Revenue Report (Full)
```
GET /api/reports/revenue?fromDate=2024-01-01&toDate=2024-01-31&groupBy=Day
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Full revenue report |
| 400 | Error |

---

### 12.2 Revenue Summary
```
GET /api/reports/revenue/summary
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Summary only |
| 400 | Error |

---

### 12.3 Revenue by Time (Chart data)
```
GET /api/reports/revenue/by-time?groupBy=Month
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Time series data |
| 400 | Error |

---

### 12.4 Revenue by Category
```
GET /api/reports/revenue/by-category
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Category breakdown |
| 400 | Error |

---

### 12.5 Revenue by Payment Method
```
GET /api/reports/revenue/by-payment-method
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Payment method breakdown |
| 400 | Error |

---

### 12.6 Top Selling Products
```
GET /api/reports/products/top-selling?top=10
```
| Parameter | Type | Description | Default |
|-----------|------|-------------|---------|
| top | int | Số lượng top | 10 |

**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Top products list |
| 400 | Error |

---

### 12.7 Promotion Usage
```
GET /api/reports/promotions/usage
```
**Status Codes:**
| Code | Description |
|------|-------------|
| 200 | Promotion statistics |
| 400 | Error |

---

## 📝 Common Response Format

### Success Response
```json
{
  "isSuccess": true,
  "message": "Operation successful",
  "data": { ... }
}
```

### Error Response
```json
{
  "isSuccess": false,
  "message": "Error description",
  "errors": ["Detailed error 1", "Detailed error 2"]
}
```

---

## 🔐 Authentication Header

```
Authorization: Bearer <access_token>
```

---

## 📅 Date Formats

- **Request:** `YYYY-MM-DD` hoặc `YYYY-MM-DDTHH:mm:ss`
- **Response:** ISO 8601 format `2024-01-15T10:30:00Z`

---

## 💰 Currency

All monetary values are in **VND (Vietnamese Dong)** as integers or decimals.

---

*Generated: 2024*
