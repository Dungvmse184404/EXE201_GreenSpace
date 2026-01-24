using EXE201_GreenSpaceDb,
 
DROP TABLE IF EXISTS "ratings",
"promotions", 
"payments", 
"order_items", 
"orders", 
"product_attribute_values", 
"attributes", 
"product_variants", 
"products", 
"categories", 
"user_address", 
"users" 
CASCADE;

-- 1. USERS
CREATE TABLE "users" (
    "user_id" SERIAL PRIMARY KEY,
    "email" VARCHAR(255) NOT NULL UNIQUE,
    "password_hash" VARCHAR(255) NOT NULL,
    "full_name" VARCHAR(255),
    "phone" VARCHAR(20),
    "role" INT DEFAULT '0', -- '0-Guest, 1-Customer','2-Staff' '3-Admin'
    "date_of_birth" TIMESTAMP,
    "gender" VARCHAR(10),
    "is_active" BOOLEAN DEFAULT TRUE,
    "create_at" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "update_at" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 2. USER_ADDRESS (Xóa User -> Mất Address)
CREATE TABLE "user_address" (
    "address_id" SERIAL PRIMARY KEY,
    "user_id" INT NOT NULL,
    "address" VARCHAR(255) NOT NULL,
    CONSTRAINT fk_user_address_users 
        FOREIGN KEY ("user_id") REFERENCES "users" ("user_id") ON DELETE CASCADE
);

-- 3. CATEGORIES (Xóa Cha -> Con mất Cha (Set Null))
CREATE TABLE "categories" (
    "category_id" SERIAL PRIMARY KEY,
    "parent_id" INT,
    "name" VARCHAR(255) NOT NULL,
    "slug" VARCHAR(255),
    CONSTRAINT fk_category_parent 
        FOREIGN KEY ("parent_id") REFERENCES "categories" ("category_id") ON DELETE SET NULL
);

-- 4. PRODUCTS (Xóa Category -> Sản phẩm còn (Set Null))
CREATE TABLE "products" (
    "product_id" SERIAL PRIMARY KEY,
    "brand_id" INT, 
    "category_id" INT,
    "name" VARCHAR(255) NOT NULL,
    "description" TEXT,
    "base_price" DECIMAL(10,2) NOT NULL,
    "thumbnail_url" VARCHAR(255),
    "is_active" BOOLEAN DEFAULT TRUE,
    "created_at" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_products_category 
        FOREIGN KEY ("category_id") REFERENCES "categories" ("category_id") ON DELETE SET NULL
);

-- 5. PRODUCT_VARIANTS (Xóa Product -> Mất Variants)
CREATE TABLE "product_variants" (
    "variant_id" SERIAL PRIMARY KEY,
    "product_id" INT NOT NULL,
    "sku" VARCHAR(100) UNIQUE,
    "price" DECIMAL(10,2) NOT NULL,
    "stock_quantity" INT DEFAULT 0,
    "image_url" VARCHAR(255),
    "color" VARCHAR(50),
    "size_or_model" VARCHAR(50),
    "is_active" BOOLEAN DEFAULT TRUE,
    "created_at" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_variants_product 
        FOREIGN KEY ("product_id") REFERENCES "products" ("product_id") ON DELETE CASCADE
);

-- 6. ATTRIBUTES
CREATE TABLE "attributes" (
    "attribute_id" SERIAL PRIMARY KEY,
    "name" VARCHAR(255) NOT NULL,
    "data_type" VARCHAR(50)
);

-- 7. PRODUCT_ATTRIBUTE_VALUES (Xóa Product/Attribute -> Mất Value)
CREATE TABLE "product_attribute_values" (
    "value_id" SERIAL PRIMARY KEY,
    "attribute_id" INT NOT NULL,
    "product_id" INT NOT NULL,
    "value" VARCHAR(255) NOT NULL,
    CONSTRAINT fk_pav_attribute 
        FOREIGN KEY ("attribute_id") REFERENCES "attributes" ("attribute_id") ON DELETE CASCADE,
    CONSTRAINT fk_pav_product 
        FOREIGN KEY ("product_id") REFERENCES "products" ("product_id") ON DELETE CASCADE
);

-- 8. ORDERS (Xóa User -> Mất Order)
CREATE TABLE "orders" (
    "order_id" SERIAL PRIMARY KEY,
    "user_id" INT NOT NULL,
    "total_amount" DECIMAL(10,2) NOT NULL,
    "status" VARCHAR(50) DEFAULT 'Pending', -- Pending, Shipping, Completed, Cancelled
    "shipping_address" VARCHAR(255),
    "created_at" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_orders_user 
        FOREIGN KEY ("user_id") REFERENCES "users" ("user_id") ON DELETE CASCADE
);

-- 9. ORDER_ITEMS (Xóa Order -> Mất Items; Xóa Variant -> Giữ Item (Set Null))
CREATE TABLE "order_items" (
    "item_id" SERIAL PRIMARY KEY,
    "order_id" INT NOT NULL,
    "variant_id" INT, 
    "quantity" INT NOT NULL,
    "price_at_purchase" DECIMAL(10,2) NOT NULL,
    "custom_data" VARCHAR(255),
    CONSTRAINT fk_order_items_order 
        FOREIGN KEY ("order_id") REFERENCES "orders" ("order_id") ON DELETE CASCADE,
    CONSTRAINT fk_order_items_variant 
        FOREIGN KEY ("variant_id") REFERENCES "product_variants" ("variant_id") ON DELETE SET NULL
);

-- 10. PAYMENTS (Xóa Order -> Mất Payment)
CREATE TABLE "payments" (
    "payment_id" SERIAL PRIMARY KEY,
    "order_id" INT NOT NULL,
    "payment_method" VARCHAR(50),
    "amount" DECIMAL(10,2) NOT NULL,
    "transaction_code" VARCHAR(100),
    "status" VARCHAR(50), -- Pending, Success, Failed
    "paid_at" TIMESTAMP,
    CONSTRAINT fk_payments_order 
        FOREIGN KEY ("order_id") REFERENCES "orders" ("order_id") ON DELETE CASCADE
);

-- 11. PROMOTIONS (Xóa Order -> Mất Promotion gắn kèm)
CREATE TABLE "promotions" (
    "promotion_id" SERIAL PRIMARY KEY,
    "order_id" INT,
    "start_date" TIMESTAMP,
    "end_date" TIMESTAMP,
    "discount_type" VARCHAR(50), -- Percentage, Fixed
    "discount_amount" DECIMAL(10,2),
    "is_active" BOOLEAN DEFAULT TRUE,
    "create_at" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "update_at" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_promotions_order 
        FOREIGN KEY ("order_id") REFERENCES "orders" ("order_id") ON DELETE CASCADE
);

-- 12. RATINGS (Xóa User/Product -> Mất Rating)
CREATE TABLE "ratings" (
    "rating_id" SERIAL PRIMARY KEY,
    "product_id" INT NOT NULL,
    "user_id" INT NOT NULL,
    "comment" TEXT,
    "stars" DECIMAL(2,1) CHECK (stars >= 0 AND stars <= 5),
    "create_date" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_ratings_product 
        FOREIGN KEY ("product_id") REFERENCES "products" ("product_id") ON DELETE CASCADE,
    CONSTRAINT fk_ratings_user 
        FOREIGN KEY ("user_id") REFERENCES "users" ("user_id") ON DELETE CASCADE
);


--=====================================================--
-- Insert dữ liệu
--=====================================================--

-- 1. Insert USERS
INSERT INTO "users" (email, password_hash, full_name, phone, role, is_active) VALUES
('admin@greenspace.com', 'hash_admin_123', 'Quản Trị Viên', '0909000111', 'Admin', TRUE),
('nguyenvana@gmail.com', 'hash_user_123', 'Nguyễn Văn A', '0912345678', 'Customer', TRUE),
('tranb@gmail.com', 'hash_user_456', 'Trần Thị B', '0987654321', 'Customer', TRUE),
('user_block@test.com', 'hash_block_789', 'Lê Văn C', '0999888777', 'Customer', FALSE); -- User bị khóa

-- 2. Insert USER_ADDRESS
INSERT INTO "user_address" (user_id, address) VALUES
(2, '123 Đường Láng, Hà Nội'),
(2, '456 Cầu Giấy, Hà Nội'), -- User A có 2 địa chỉ
(3, '789 Nguyễn Huệ, TP.HCM');

-- 3. Insert CATEGORIES
INSERT INTO "categories" (name, slug, parent_id) VALUES
('Cây Cảnh', 'cay-canh', NULL),   -- ID 1
('Chậu Cây', 'chau-cay', NULL),   -- ID 2
('Dụng Cụ', 'dung-cu', NULL);     -- ID 3

INSERT INTO "categories" (name, slug, parent_id) VALUES
('Cây Trong Nhà', 'cay-trong-nha', 1), -- ID 4 (Con của 1)
('Sen Đá', 'sen-da', 1),               -- ID 5 (Con của 1)
('Chậu Đất Nung', 'chau-dat-nung', 2); -- ID 6 (Con của 2)

-- 4. Insert PRODUCTS
INSERT INTO "products" (brand_id, category_id, name, description, base_price, thumbnail_url) VALUES
(1, 4, 'Cây Kim Tiền', 'Mang lại tài lộc', 150000, 'url_kimtien.jpg'),
(1, 4, 'Cây Lưỡi Hổ', 'Lọc không khí tốt', 120000, 'url_luoiho.jpg'),
(2, 5, 'Sen Đá Nâu', 'Dễ chăm sóc', 50000, 'url_senda.jpg'),
(3, 6, 'Chậu Đất Nung Size L', 'Thoát nước tốt', 30000, 'url_chau.jpg');

-- 5. Insert PRODUCT_VARIANTS (Quan trọng: Test case nhiều biến thể)
INSERT INTO "product_variants" (product_id, sku, price, stock_quantity, size_or_model, color) VALUES
(1, 'KT-S', 150000, 100, 'Small', 'Xanh'),       -- Kim Tiền nhỏ
(1, 'KT-L', 250000, 50, 'Large', 'Xanh'),        -- Kim Tiền lớn
(2, 'LH-M', 120000, 0, 'Medium', 'Vàng Xanh'),   -- Lưỡi Hổ (Hết hàng - Test case stock=0)
(3, 'SD-N', 50000, 200, 'Standard', 'Nâu'),      -- Sen đá
(4, 'CDN-L', 30000, 500, 'L', 'Đỏ Đất');         -- Chậu

-- 6. Insert ATTRIBUTES & VALUES
INSERT INTO "attributes" (name, data_type) VALUES ('Yêu cầu ánh sáng', 'String'), ('Tưới nước', 'String');

INSERT INTO "product_attribute_values" (attribute_id, product_id, value) VALUES
(1, 1, 'Bóng râm'),       -- Kim tiền ưa bóng râm
(2, 1, '1 lần/tuần'),
(1, 3, 'Nắng trực tiếp'); -- Sen đá ưa nắng

-- 7. Insert ORDERS
INSERT INTO "orders" (user_id, total_amount, status, shipping_address) VALUES
(2, 400000, 'Completed', '123 Đường Láng, Hà Nội'), -- Đơn 1 của User A (Thành công)
(2, 150000, 'Shipping', '456 Cầu Giấy, Hà Nội'),    -- Đơn 2 của User A (Đang giao)
(3, 50000, 'Pending', '789 Nguyễn Huệ, TP.HCM'),    -- Đơn 3 của User B (Mới đặt)
(2, 0, 'Cancelled', '123 Đường Láng, Hà Nội');      -- Đơn 4 (Đã hủy)

-- 8. Insert ORDER_ITEMS
INSERT INTO "order_items" (order_id, variant_id, quantity, price_at_purchase) VALUES
(1, 1, 1, 150000), -- Đơn 1 mua 1 Kim Tiền nhỏ
(1, 2, 1, 250000), -- Đơn 1 mua 1 Kim Tiền lớn (Tổng 400k)
(2, 1, 1, 150000), -- Đơn 2 mua 1 Kim Tiền nhỏ
(3, 3, 1, 50000);  -- Đơn 3 mua 1 Sen đá

-- 9. Insert PAYMENTS
INSERT INTO "payments" (order_id, payment_method, amount, status, transaction_code, paid_at) VALUES
(1, 'Momo', 400000, 'Success', 'MOMO123456', NOW()),
(2, 'COD', 150000, 'Pending', NULL, NULL);

-- 10. Insert RATINGS
INSERT INTO "ratings" (product_id, user_id, stars, comment) VALUES
(1, 2, 5, 'Cây rất đẹp, đóng gói kỹ'),
(3, 3, 4, 'Sen đá hơi nhỏ nhưng khỏe');

-- 11. Insert PROMOTIONS
INSERT INTO "promotions" (start_date, end_date, discount_type, discount_amount, is_active) VALUES
(NOW(), NOW() + INTERVAL '7 days', 'Percentage', 10.00, TRUE), -- Giảm 10% tuần lễ vàng
(NOW() - INTERVAL '30 days', NOW() - INTERVAL '1 day', 'Fixed', 50000, FALSE); -- Voucher cũ đã hết hạn