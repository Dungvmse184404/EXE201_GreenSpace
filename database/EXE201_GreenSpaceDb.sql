-- ============================================
-- DROP ALL TABLES IN CORRECT ORDER
-- ============================================

-- Drop tables with foreign keys first
DROP TABLE IF EXISTS cart_items CASCADE;
DROP TABLE IF EXISTS carts CASCADE;
DROP TABLE IF EXISTS order_items CASCADE;
DROP TABLE IF EXISTS payments CASCADE;
DROP TABLE IF EXISTS promotions CASCADE;
DROP TABLE IF EXISTS orders CASCADE;
DROP TABLE IF EXISTS ratings CASCADE;
DROP TABLE IF EXISTS product_attribute_values CASCADE;
DROP TABLE IF EXISTS product_variants CASCADE;
DROP TABLE IF EXISTS products CASCADE;
DROP TABLE IF EXISTS categories CASCADE;
DROP TABLE IF EXISTS attributes CASCADE;
DROP TABLE IF EXISTS user_address CASCADE;
DROP TABLE IF EXISTS refresh_tokens CASCADE;
DROP TABLE IF EXISTS users CASCADE;

-- Drop extension
DROP EXTENSION IF EXISTS pgcrypto CASCADE;

-- Verify all tables are dropped
SELECT tablename FROM pg_tables WHERE schemaname = 'public';

-- ============================================
-- NUCLEAR OPTION: Drop and Recreate Schema (Do not touch)
-- ============================================

DROP SCHEMA public CASCADE;
CREATE SCHEMA public;
GRANT ALL ON SCHEMA public TO postgres;
GRANT ALL ON SCHEMA public TO public;

-- Recreate pgcrypto extension
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- ============================================
-- MAIN ENTITIES SCRIPT
-- ============================================
-- dùng migrationdid 


-- ============================================
-- GREENSPACE SAMPLE DATA SCRIPT
-- ============================================

-- Enable UUID generation
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- ============================================
-- 1. USERS (5 users: 1 admin, 1 staff, 3 customers)
-- ============================================

INSERT INTO users (user_id, username, email, password_hash, first_name, last_name, phone, role, date_of_birth, gender, is_active, create_at, update_at)
VALUES
-- Admin
(
    gen_random_uuid(),
    'admin',
    'admin@greenspace.com',
    -- Password: Admin@123a
    'AQAAAAIAAYagAAAAELK8xZFJ3qYWvH4kNJL2K3cY5T8vVZwqK2H6xP9mR7nS1tB4uC5wD6eE7fF8gG9h',
    'System',
    'Admin',
    '0901234567',
    'ADMIN',
    '1990-01-01',
    'Male',
    true,
    NOW(),
    NOW()
),
-- Staff
(
    gen_random_uuid(),
    'staff01',
    'staff@greenspace.com',
    -- Password: Staff@123
    'AQAAAAIAAYagAAAAELK8xZFJ3qYWvH4kNJL2K3cY5T8vVZwqK2H6xP9mR7nS1tB4uC5wD6eE7fF8gG9h',
    'Nguyen',
    'Staff',
    '0902234567',
    'STAFF',
    '1995-05-15',
    'Female',
    true,
    NOW(),
    NOW()
),
-- Customer 1
(
    gen_random_uuid(),
    'customer01',
    'customer1@gmail.com',
    -- Password: Customer@123
    'AQAAAAIAAYagAAAAELK8xZFJ3qYWvH4kNJL2K3cY5T8vVZwqK2H6xP9mR7nS1tB4uC5wD6eE7fF8gG9h',
    'Tran',
    'Van A',
    '0903234567',
    'CUSTOMER',
    '1998-03-20',
    'Male',
    true,
    NOW(),
    NOW()
),
-- Customer 2
(
    gen_random_uuid(),
    'customer02',
    'customer2@gmail.com',
    'AQAAAAIAAYagAAAAELK8xZFJ3qYWvH4kNJL2K3cY5T8vVZwqK2H6xP9mR7nS1tB4uC5wD6eE7fF8gG9h',
    'Le',
    'Thi B',
    '0904234567',
    'CUSTOMER',
    '1997-07-10',
    'Female',
    true,
    NOW(),
    NOW()
),
-- Customer 3
(
    gen_random_uuid(),
    'customer03',
    'customer3@gmail.com',
    'AQAAAAIAAYagAAAAELK8xZFJ3qYWvH4kNJL2K3cY5T8vVZwqK2H6xP9mR7nS1tB4uC5wD6eE7fF8gG9h',
    'Pham',
    'Van C',
    '0905234567',
    'CUSTOMER',
    '1999-12-25',
    'Male',
    true,
    NOW(),
    NOW()
);

-- ============================================
-- 2. USER ADDRESSES
-- ============================================

INSERT INTO user_address (address_id, user_id, address)
SELECT 
    gen_random_uuid(),
    u.user_id,
    CASE u.email
        WHEN 'customer1@gmail.com' THEN '123 Nguyen Van Linh, Q7, HCMC'
        WHEN 'customer2@gmail.com' THEN '456 Le Van Viet, Q9, HCMC'
        WHEN 'customer3@gmail.com' THEN '789 Vo Van Ngan, Thu Duc, HCMC'
    END
FROM users u
WHERE u.role = 'CUSTOMER';

-- ============================================
-- 3. CATEGORIES (Hierarchical structure)
-- ============================================

WITH cat_indoor AS (
    INSERT INTO categories (category_id, name, slug, parent_id)
    VALUES (gen_random_uuid(), 'Cây Cảnh Trong Nhà', 'cay-canh-trong-nha', NULL)
    RETURNING category_id
),
cat_outdoor AS (
    INSERT INTO categories (category_id, name, slug, parent_id)
    VALUES (gen_random_uuid(), 'Cây Cảnh Ngoài Trời', 'cay-canh-ngoai-troi', NULL)
    RETURNING category_id
),
cat_tools AS (
    INSERT INTO categories (category_id, name, slug, parent_id)
    VALUES (gen_random_uuid(), 'Dụng Cụ Làm Vườn', 'dung-cu-lam-vuon', NULL)
    RETURNING category_id
),
cat_pots AS (
    INSERT INTO categories (category_id, name, slug, parent_id)
    VALUES (gen_random_uuid(), 'Chậu & Giá Trồng', 'chau-gia-trong', NULL)
    RETURNING category_id
)
-- Sub-categories for Indoor Plants
INSERT INTO categories (category_id, name, slug, parent_id)
SELECT gen_random_uuid(), 'Cây Lọc Không Khí', 'cay-loc-khong-khi', category_id FROM cat_indoor
UNION ALL
SELECT gen_random_uuid(), 'Cây Để Bàn', 'cay-de-ban', category_id FROM cat_indoor
UNION ALL
SELECT gen_random_uuid(), 'Cây Sen Đá', 'cay-sen-da', category_id FROM cat_indoor
UNION ALL
-- Sub-categories for Outdoor Plants
SELECT gen_random_uuid(), 'Cây Ăn Quả', 'cay-an-qua', category_id FROM cat_outdoor
UNION ALL
SELECT gen_random_uuid(), 'Cây Hoa', 'cay-hoa', category_id FROM cat_outdoor
UNION ALL
-- Sub-categories for Tools
SELECT gen_random_uuid(), 'Dụng Cụ Tưới', 'dung-cu-tuoi', category_id FROM cat_tools
UNION ALL
SELECT gen_random_uuid(), 'Dụng Cụ Cắt Tỉa', 'dung-cu-cat-tia', category_id FROM cat_tools;

-- ============================================
-- 4. PRODUCTS (20 products)
-- ============================================

WITH cat_ids AS (
    SELECT category_id, name FROM categories WHERE parent_id IS NOT NULL
)
INSERT INTO products (product_id, name, description, base_price, thumbnail_url, category_id, is_active, created_at, updated_at)
VALUES
-- Indoor Plants
(
    gen_random_uuid(),
    'Cây Kim Tiền',
    'Cây phong thủy mang lại may mắn, dễ chăm sóc, lọc không khí tốt',
    150000,
    'https://cayxanh.vn/images/kim-tien.jpg',
    (SELECT category_id FROM cat_ids WHERE name = 'Cây Lọc Không Khí' LIMIT 1),
    true,
    NOW(),
    NOW()
),
(
    gen_random_uuid(),
    'Cây Trầu Bà',
    'Cây lọc không khí hiệu quả, phù hợp văn phòng',
    80000,
    'https://cayxanh.vn/images/trau-ba.jpg',
    (SELECT category_id FROM cat_ids WHERE name = 'Cây Lọc Không Khí' LIMIT 1),
    true,
    NOW(),
    NOW()
),
(
    gen_random_uuid(),
    'Cây Lưỡi Hổ',
    'Cây cực kỳ dễ sống, lọc không khí ban đêm',
    120000,
    'https://cayxanh.vn/images/luoi-ho.jpg',
    (SELECT category_id FROM cat_ids WHERE name = 'Cây Lọc Không Khí' LIMIT 1),
    true,
    NOW(),
    NOW()
),
(
    gen_random_uuid(),
    'Xương Rồng Mini',
    'Cây nhỏ xinh, dễ thương, không cần tưới nhiều nước',
    50000,
    'https://cayxanh.vn/images/xuong-rong.jpg',
    (SELECT category_id FROM cat_ids WHERE name = 'Cây Để Bàn' LIMIT 1),
    true,
    NOW(),
    NOW()
),
(
    gen_random_uuid(),
    'Sen Đá Đa Màu',
    'Bộ 5 chậu sen đá nhiều màu sắc',
    200000,
    'https://cayxanh.vn/images/sen-da.jpg',
    (SELECT category_id FROM cat_ids WHERE name = 'Cây Sen Đá' LIMIT 1),
    true,
    NOW(),
    NOW()
),
-- Outdoor Plants
(
    gen_random_uuid(),
    'Cây Hoa Hồng',
    'Hoa hồng Đà Lạt, nhiều màu sắc',
    180000,
    'https://cayxanh.vn/images/hoa-hong.jpg',
    (SELECT category_id FROM cat_ids WHERE name = 'Cây Hoa' LIMIT 1),
    true,
    NOW(),
    NOW()
),
(
    gen_random_uuid(),
    'Cây Dâu Tây',
    'Dâu tây sạch, trồng sân thượng',
    150000,
    'https://cayxanh.vn/images/dau-tay.jpg',
    (SELECT category_id FROM cat_ids WHERE name = 'Cây Ăn Quả' LIMIT 1),
    true,
    NOW(),
    NOW()
),
(
    gen_random_uuid(),
    'Cây Cà Chua Cherry',
    'Cà chua bi, dễ trồng, năng suất cao',
    100000,
    'https://cayxanh.vn/images/ca-chua.jpg',
    (SELECT category_id FROM cat_ids WHERE name = 'Cây Ăn Quả' LIMIT 1),
    true,
    NOW(),
    NOW()
),
(
    gen_random_uuid(),
    'Cây Hoa Giấy',
    'Hoa giấy nhiều màu, chịu nắng tốt',
    250000,
    'https://cayxanh.vn/images/hoa-giay.jpg',
    (SELECT category_id FROM cat_ids WHERE name = 'Cây Hoa' LIMIT 1),
    true,
    NOW(),
    NOW()
),
(
    gen_random_uuid(),
    'Cây Hoa Phong Lan',
    'Phong lan Hồ Điệp, sang trọng',
    300000,
    'https://cayxanh.vn/images/phong-lan.jpg',
    (SELECT category_id FROM cat_ids WHERE name = 'Cây Hoa' LIMIT 1),
    true,
    NOW(),
    NOW()
),
-- Tools
(
    gen_random_uuid(),
    'Bình Tưới Cây 1L',
    'Bình tưới nhựa cao cấp, vòi phun mịn',
    45000,
    'https://cayxanh.vn/images/binh-tuoi.jpg',
    (SELECT category_id FROM cat_ids WHERE name = 'Dụng Cụ Tưới' LIMIT 1),
    true,
    NOW(),
    NOW()
),
(
    gen_random_uuid(),
    'Bộ 3 Dụng Cụ Làm Vườn',
    'Cuốc nhỏ, xẻng, xới đất mini',
    120000,
    'https://cayxanh.vn/images/bo-dung-cu.jpg',
    (SELECT category_id FROM cat_ids WHERE name = 'Dụng Cụ Cắt Tỉa' LIMIT 1),
    true,
    NOW(),
    NOW()
),
(
    gen_random_uuid(),
    'Kéo Cắt Cành Chuyên Dụng',
    'Kéo sắc bén, tay cầm chống trượt',
    150000,
    'https://cayxanh.vn/images/keo-cat.jpg',
    (SELECT category_id FROM cat_ids WHERE name = 'Dụng Cụ Cắt Tỉa' LIMIT 1),
    true,
    NOW(),
    NOW()
),
(
    gen_random_uuid(),
    'Vòi Xịt Nước Đa Năng',
    'Vòi xịt 7 chế độ, tiết kiệm nước',
    80000,
    'https://cayxanh.vn/images/voi-xit.jpg',
    (SELECT category_id FROM cat_ids WHERE name = 'Dụng Cụ Tưới' LIMIT 1),
    true,
    NOW(),
    NOW()
),
-- Pots
(
    gen_random_uuid(),
    'Chậu Sứ Cao Cấp 20cm',
    'Chậu sứ trắng, có lỗ thoát nước',
    80000,
    'https://cayxanh.vn/images/chau-su.jpg',
    (SELECT category_id FROM categories WHERE name = 'Chậu & Giá Trồng' LIMIT 1),
    true,
    NOW(),
    NOW()
),
(
    gen_random_uuid(),
    'Chậu Nhựa Composite 30cm',
    'Chậu composite giả xi măng, nhẹ bền',
    150000,
    'https://cayxanh.vn/images/chau-composite.jpg',
    (SELECT category_id FROM categories WHERE name = 'Chậu & Giá Trồng' LIMIT 1),
    true,
    NOW(),
    NOW()
),
(
    gen_random_uuid(),
    'Giá Treo Cây 3 Tầng',
    'Giá sắt sơn tĩnh điện, chịu nặng tốt',
    450000,
    'https://cayxanh.vn/images/gia-treo.jpg',
    (SELECT category_id FROM categories WHERE name = 'Chậu & Giá Trồng' LIMIT 1),
    true,
    NOW(),
    NOW()
),
(
    gen_random_uuid(),
    'Chậu Treo Macrame',
    'Chậu treo dây bện thủ công',
    120000,
    'https://cayxanh.vn/images/chau-macrame.jpg',
    (SELECT category_id FROM categories WHERE name = 'Chậu & Giá Trồng' LIMIT 1),
    true,
    NOW(),
    NOW()
),
-- Combo Sets
(
    gen_random_uuid(),
    'Combo Cây Để Bàn 5 Loại',
    'Bộ 5 cây nhỏ phù hợp bàn làm việc',
    350000,
    'https://cayxanh.vn/images/combo-de-ban.jpg',
    (SELECT category_id FROM cat_ids WHERE name = 'Cây Để Bàn' LIMIT 1),
    true,
    NOW(),
    NOW()
),
(
    gen_random_uuid(),
    'Combo Starter Kit',
    'Bộ dụng cụ + phân bón + đất trồng cho người mới',
    280000,
    'https://cayxanh.vn/images/starter-kit.jpg',
    (SELECT category_id FROM cat_ids WHERE name = 'Dụng Cụ Tưới' LIMIT 1),
    true,
    NOW(),
    NOW()
);

-- ============================================
-- 5. PRODUCT VARIANTS (Multiple variants per product)
-- ============================================

WITH prod AS (
    SELECT product_id, name FROM products ORDER BY name
)
INSERT INTO product_variants (variant_id, product_id, sku, price, stock_quantity, image_url, color, size_or_model, is_active, created_at, updated_at)
SELECT 
    gen_random_uuid(),
    p.product_id,
    'SKU-' || LPAD((ROW_NUMBER() OVER (ORDER BY p.name))::text, 4, '0') || '-' || size_variant.suffix,
    CASE 
        WHEN size_variant.size = 'S' THEN p.base_price * 0.8
        WHEN size_variant.size = 'M' THEN p.base_price
        WHEN size_variant.size = 'L' THEN p.base_price * 1.3
    END,
    CASE 
        WHEN size_variant.size = 'S' THEN 50
        WHEN size_variant.size = 'M' THEN 30
        WHEN size_variant.size = 'L' THEN 20
    END,
    'https://cayxanh.vn/images/' || LOWER(REPLACE(p.name, ' ', '-')) || '-' || size_variant.size || '.jpg',
    NULL,
    size_variant.size,
    true,
    NOW(),
    NOW()
FROM prod p
CROSS JOIN (
    VALUES ('S', 'SMALL'), ('M', 'MEDIUM'), ('L', 'LARGE')
) AS size_variant(size, suffix)
WHERE p.name NOT LIKE '%Combo%'
LIMIT 45;

-- ============================================
-- 6. ORDERS (10 sample orders)
-- ============================================

WITH customer_ids AS (
    SELECT user_id FROM users WHERE role = 'CUSTOMER' LIMIT 3
)
INSERT INTO orders (order_id, user_id, total_amount, status, shipping_address, created_at)
SELECT 
    gen_random_uuid(),
    (SELECT user_id FROM customer_ids ORDER BY RANDOM() LIMIT 1),
    (RANDOM() * 500000 + 100000)::numeric(10,2),
    CASE (RANDOM() * 4)::int
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Processing'
        WHEN 2 THEN 'Paid'
        ELSE 'Completed'
    END,
    '123 Test Street, District ' || (ROW_NUMBER() OVER ()) || ', HCMC',
    NOW() - (RANDOM() * INTERVAL '30 days')
FROM generate_series(1, 10);

-- ============================================
-- 7. ORDER ITEMS
-- ============================================

WITH order_list AS (
    SELECT order_id, ROW_NUMBER() OVER () as rn FROM orders
),
variant_list AS (
    SELECT variant_id, price, ROW_NUMBER() OVER () as rn FROM product_variants WHERE is_active = true
)
INSERT INTO order_items (item_id, order_id, variant_id, quantity, price_at_purchase)
SELECT 
    gen_random_uuid(),
    o.order_id,
    v.variant_id,
    (RANDOM() * 3 + 1)::int,
    v.price
FROM order_list o
CROSS JOIN LATERAL (
    SELECT variant_id, price 
    FROM variant_list 
    WHERE rn = (o.rn % 20) + 1
    LIMIT 2
) v;

-- ============================================
-- 8. PAYMENTS
-- ============================================

INSERT INTO payments (payment_id, 
	order_id, 
	payment_method, 
    gateway, 
    transaction_ref,
    amount, 
    transaction_code, 
    bank_code, 
    status, 
    response_code, 
    response_message, 
    created_at,       
    paid_at, 
    expired_at        
)
WITH SeedData AS (
    SELECT 
        o.order_id,
        o.total_amount,
        o.status as order_status,
        o.created_at as order_created_at,
        (floor(random() * 3))::int as method_seed -- 0: VNPay, 1: COD, 2: BankTransfer
    FROM orders o
    -- Chỉ tạo payment cho các đơn hàng chưa có payment (tránh duplicate nếu chạy nhiều lần)
    WHERE NOT EXISTS (SELECT 1 FROM payments p WHERE p.order_id = o.order_id)
)
SELECT 
    gen_random_uuid(), -- payment_id
    order_id,
    
    -- 1. Payment Method & Gateway Logic
    CASE method_seed
        WHEN 0 THEN 'VNPay'
        WHEN 1 THEN 'COD'
        ELSE 'BankTransfer'
    END as payment_method,

    CASE method_seed
        WHEN 0 THEN 'VNPay' -- Gateway khớp với Method
        WHEN 1 THEN 'None'  -- COD không qua Gateway
        ELSE 'Manual'       -- Chuyển khoản thủ công
    END as gateway,

    -- 2. Transaction Ref (Giả lập mã gửi sang cổng thanh toán)
    -- Format: ORDERID_TIMESTAMP
    CONCAT(SUBSTRING(order_id::text, 1, 8), '_', to_char(order_created_at, 'YYYYMMDDHHMISS')) as transaction_ref,

    total_amount,

    -- 3. Transaction Code (Mã giao dịch từ ngân hàng trả về)
    CASE 
        WHEN order_status IN ('Paid', 'Completed') THEN 'TXN' || LPAD((ROW_NUMBER() OVER ())::text, 8, '0')
        ELSE NULL -- Chưa thanh toán xong thì chưa có mã giao dịch
    END as transaction_code,

    -- 4. Bank Code (Giả lập)
    CASE method_seed
        WHEN 0 THEN 'NCB' -- VNPay hay test bằng NCB
        WHEN 2 THEN 'VCB' -- BankTransfer ví dụ VCB
        ELSE NULL 
    END as bank_code,

    -- 5. Status Mapping
    CASE order_status
        WHEN 'Paid' THEN 'Success'
        WHEN 'Completed' THEN 'Success'
        WHEN 'Cancelled' THEN 'Failed'
        ELSE 'Pending'
    END as status,

    -- 6. Response Info (Giả lập dữ liệu trả về từ cổng)
    CASE 
        WHEN order_status IN ('Paid', 'Completed') THEN '00' 
        ELSE NULL 
    END as response_code,

    CASE 
        WHEN order_status IN ('Paid', 'Completed') THEN 'Transaction Successful'
        ELSE NULL 
    END as response_message,

    -- 7. Timestamps
    order_created_at as created_at,
    
    CASE 
        WHEN order_status IN ('Paid', 'Completed') THEN order_created_at + INTERVAL '5 minutes'
        ELSE NULL
    END as paid_at,

    order_created_at + INTERVAL '15 minutes' as expired_at 
FROM SeedData;

-- ============================================
-- 9. CARTS (for 3 customers)
-- ============================================

INSERT INTO carts (cart_id, user_id, created_at, updated_at)
SELECT 
    gen_random_uuid(),
    user_id,
    NOW(),
    NOW()
FROM users 
WHERE role = 'CUSTOMER';

-- ============================================
-- 10. CART ITEMS
-- ============================================

WITH cart_list AS (
    SELECT cart_id, ROW_NUMBER() OVER () as rn FROM carts
),
variant_sample AS (
    SELECT variant_id, ROW_NUMBER() OVER () as rn FROM product_variants WHERE is_active = true LIMIT 10
)
INSERT INTO cart_items (cart_item_id, cart_id, variant_id, quantity, created_at)
SELECT 
    gen_random_uuid(),
    c.cart_id,
    v.variant_id,
    (RANDOM() * 3 + 1)::int,
    NOW()
FROM cart_list c
CROSS JOIN LATERAL (
    SELECT variant_id 
    FROM variant_sample 
    WHERE rn = (c.rn % 5) + 1
    LIMIT 2
) v;

-- ============================================
-- 11. RATINGS (Random ratings for products)
-- ============================================

WITH prod_sample AS (
    SELECT product_id FROM products WHERE is_active = true LIMIT 10
),
customer_list AS (
    SELECT user_id FROM users WHERE role = 'CUSTOMER'
)
INSERT INTO ratings (rating_id, product_id, user_id, stars, comment, create_date)
SELECT 
    gen_random_uuid(),
    p.product_id,
    c.user_id,
    (RANDOM() * 4 + 1)::numeric(2,1),
    CASE (RANDOM() * 5)::int
        WHEN 0 THEN 'Cây đẹp, giao hàng nhanh!'
        WHEN 1 THEN 'Chất lượng tốt, đóng gói cẩn thận'
        WHEN 2 THEN 'Sản phẩm như mô tả'
        WHEN 3 THEN 'Rất hài lòng với cây'
        ELSE 'Shop nhiệt tình, cây xanh tốt'
    END,
    NOW() - (RANDOM() * INTERVAL '60 days')
FROM prod_sample p
CROSS JOIN customer_list c
WHERE RANDOM() > 0.5
LIMIT 20;

-- ============================================
-- 12. PROMOTIONS
-- ============================================

INSERT INTO promotions (promotion_id, start_date, end_date, discount_type, discount_amount, is_active, create_at, update_at)
VALUES
(
    gen_random_uuid(),
    NOW(),
    NOW() + INTERVAL '30 days',
    'Percentage',
    15,
    true,
    NOW(),
    NOW()
),
(
    gen_random_uuid(),
    NOW() + INTERVAL '7 days',
    NOW() + INTERVAL '14 days',
    'Fixed',
    50000,
    true,
    NOW(),
    NOW()
),
(
    gen_random_uuid(),
    NOW() - INTERVAL '10 days',
    NOW() + INTERVAL '20 days',
    'Percentage',
    20,
    true,
    NOW(),
    NOW()
);

-- ============================================
-- VERIFICATION QUERIES
-- ============================================

-- Count records
SELECT 'Users' as table_name, COUNT(*) as count FROM users
UNION ALL
SELECT 'Categories', COUNT(*) FROM categories
UNION ALL
SELECT 'Products', COUNT(*) FROM products
UNION ALL
SELECT 'Product Variants', COUNT(*) FROM product_variants
UNION ALL
SELECT 'Orders', COUNT(*) FROM orders
UNION ALL
SELECT 'Order Items', COUNT(*) FROM order_items
UNION ALL
SELECT 'Payments', COUNT(*) FROM payments
UNION ALL
SELECT 'Carts', COUNT(*) FROM carts
UNION ALL
SELECT 'Cart Items', COUNT(*) FROM cart_items
UNION ALL
SELECT 'Ratings', COUNT(*) FROM ratings
UNION ALL
SELECT 'Promotions', COUNT(*) FROM promotions
UNION ALL
SELECT 'User Addresses', COUNT(*) FROM user_address;




-- ============================================
-- CHECK DATA INTEGRITY
-- ============================================

-- 1. Products with variants
SELECT 
    p.name,
    COUNT(pv.variant_id) as variant_count,
    MIN(pv.price) as min_price,
    MAX(pv.price) as max_price
FROM products p
LEFT JOIN product_variants pv ON p.product_id = pv.product_id
GROUP BY p.product_id, p.name
ORDER BY p.name;

-- 2. Orders with details
SELECT 
    o.order_id,
    u.email,
    o.status,
    o.total_amount,
    COUNT(oi.item_id) as item_count,
    o.created_at
FROM orders o
JOIN users u ON o.user_id = u.user_id
LEFT JOIN order_items oi ON o.order_id = oi.order_id
GROUP BY o.order_id, u.email, o.status, o.total_amount, o.created_at
ORDER BY o.created_at DESC;

-- 3. Category tree
WITH RECURSIVE category_tree AS (
    SELECT 
        category_id,
        name,
        parent_id,
        name as path,
        0 as level
    FROM categories
    WHERE parent_id IS NULL
    
    UNION ALL
    
    SELECT 
        c.category_id,
        c.name,
        c.parent_id,
        ct.path || ' > ' || c.name,
        ct.level + 1
    FROM categories c
    JOIN category_tree ct ON c.parent_id = ct.category_id
)
SELECT 
    REPEAT('  ', level) || name as category_structure,
    level
FROM category_tree
ORDER BY path;

-- 4. Customer purchase history
SELECT 
    u.email,
    COUNT(DISTINCT o.order_id) as total_orders,
    SUM(o.total_amount) as total_spent,
    MAX(o.created_at) as last_order_date
FROM users u
LEFT JOIN orders o ON u.user_id = o.user_id
WHERE u.role = 'CUSTOMER'
GROUP BY u.user_id, u.email
ORDER BY total_spent DESC NULLS LAST;

-- 5. Product ratings summary
SELECT 
    p.name,
    COUNT(r.rating_id) as rating_count,
    ROUND(AVG(r.stars), 2) as avg_rating
FROM products p
LEFT JOIN ratings r ON p.product_id = r.product_id
GROUP BY p.product_id, p.name
HAVING COUNT(r.rating_id) > 0
ORDER BY avg_rating DESC, rating_count DESC;

-- 6. Cart contents
SELECT 
    u.email,
    p.name as product_name,
    pv.size_or_model,
    ci.quantity,
    pv.price,
    (ci.quantity * pv.price) as subtotal
FROM carts c
JOIN users u ON c.user_id = u.user_id
JOIN cart_items ci ON c.cart_id = ci.cart_id
JOIN product_variants pv ON ci.variant_id = pv.variant_id
JOIN products p ON pv.product_id = p.product_id
ORDER BY u.email, p.name;
