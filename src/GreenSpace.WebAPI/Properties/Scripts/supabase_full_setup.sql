-- ============================================
-- GreenSpace Full Setup Script for Supabase
-- Run this on Supabase SQL Editor
-- ============================================
-- ORDER:
-- 1. Run migration_user_address_v2.sql (if upgrading)
-- 2. Run this seed_data script (for fresh data)
-- ============================================

-- ============================================
-- CLEAR EXISTING DATA (except users - optional)
-- ============================================
TRUNCATE TABLE cart_items, carts, order_items, orders, payments, ratings,
    product_attribute_values, product_variants, products,
    categories, attributes, promotions, user_address, refresh_tokens
    CASCADE;

-- ============================================
-- 1. USERS (Password: "Password123")
-- ============================================
INSERT INTO users (user_id, username, email, password_hash, first_name, last_name, phone, role, gender, is_active, create_at, update_at)
VALUES
    ('94c69894-6840-42f6-953b-014529329709', 'staff01', 'staff01@greenspace.vn', '$2a$11$rBNxPfv.YhBRFDHxXjGYeO5FgVrXXmrCqPVn0YQwVJOZGGWcOFRMu', 'Nguyen', 'Staff', '0902234567', 'Staff', 'Female', true, NOW(), NOW()),
    ('a8677c73-4560-466d-9c3f-42e557169623', 'customer01', 'customer01@gmail.com', '$2a$11$rBNxPfv.YhBRFDHxXjGYeO5FgVrXXmrCqPVn0YQwVJOZGGWcOFRMu', 'Tran', 'Minh', '0903334567', 'Customer', 'Male', true, NOW(), NOW()),
    ('c5695029-7987-4355-9b22-485764d9d832', 'customer02', 'customer02@gmail.com', '$2a$11$rBNxPfv.YhBRFDHxXjGYeO5FgVrXXmrCqPVn0YQwVJOZGGWcOFRMu', 'Le', 'Hoa', '0904445678', 'Customer', 'Female', true, NOW(), NOW()),
    ('d2891390-8488-4444-9b20-67512760f351', 'customer03', 'customer03@gmail.com', '$2a$11$rBNxPfv.YhBRFDHxXjGYeO5FgVrXXmrCqPVn0YQwVJOZGGWcOFRMu', 'Pham', 'Anh', '0905556789', 'Customer', 'Male', true, NOW(), NOW())
ON CONFLICT (user_id) DO NOTHING;

-- ============================================
-- 2. USER ADDRESSES (New schema)
-- ============================================
INSERT INTO user_address (address_id, user_id, province, district, ward, street_address, label, is_default, created_at)
VALUES
    -- Customer 01: 2 addresses
    ('10a34b22-0546-4c44-8039-4b8c9d0e1f2a', 'a8677c73-4560-466d-9c3f-42e557169623',
     'TP. Ho Chi Minh', 'Quan 1', 'Phuong Ben Nghe', '123 Nguyen Hue', 'Nha', true, NOW()),
    ('2b9c8d33-1657-5d55-914a-5c9d0e1f2a3b', 'a8677c73-4560-466d-9c3f-42e557169623',
     'TP. Ho Chi Minh', 'Quan 3', 'Phuong 7', '456 Le Loi', 'Cong ty', false, NOW()),

    -- Customer 02: 2 addresses
    ('3c8d7e44-2768-6e66-a25b-6d0e1f2a3b4c', 'c5695029-7987-4355-9b22-485764d9d832',
     'TP. Ho Chi Minh', 'Quan 5', 'Phuong 11', '789 Tran Hung Dao', 'Nha', true, NOW()),
    ('5e9f0a11-4980-8088-c47d-8f0a1b2c3d4e', 'c5695029-7987-4355-9b22-485764d9d832',
     'TP. Ho Chi Minh', 'Quan 7', 'Phuong Tan Phu', '100 Nguyen Van Linh, Sunrise City', 'Nha ban', false, NOW()),

    -- Customer 03: 2 addresses
    ('4d7e6f55-3879-7f77-b36c-7e1f2a3b4c5d', 'd2891390-8488-4444-9b20-67512760f351',
     'TP. Ho Chi Minh', 'Quan 1', 'Phuong Pham Ngu Lao', '321 Pham Ngu Lao', 'Nha tro', true, NOW()),
    ('6f0a1b22-5091-9199-d58e-9a1b2c3d4e5f', 'd2891390-8488-4444-9b20-67512760f351',
     'Ha Noi', 'Quan Hoan Kiem', 'Phuong Hang Bai', '50 Trang Tien', 'Nha Ha Noi', false, NOW())
ON CONFLICT (address_id) DO NOTHING;

-- ============================================
-- 3. CATEGORIES
-- ============================================
INSERT INTO categories (category_id, parent_id, name, slug)
VALUES
    -- Parent Categories
    ('50b1f7d1-9b1a-4f5c-8d1e-2a3b4c5d6e7f', NULL, 'Cay Canh', 'cay-canh'),
    ('61c208e2-0c2b-506d-9e2f-3b4c5d6e7f8a', NULL, 'Chau Cay', 'chau-cay'),
    ('72d319f3-1d3c-617e-af30-4c5d6e7f8a9b', NULL, 'Phan Bon & Dat', 'phan-bon-dat'),
    ('83e42aa4-2e4d-728f-ba41-5d6e7f8a9b0c', NULL, 'Dung Cu Lam Vuon', 'dung-cu-lam-vuon'),

    -- Children - Cay Canh
    ('94f53bb5-3f5e-8390-cc52-6e7f8a9b0c1d', '50b1f7d1-9b1a-4f5c-8d1e-2a3b4c5d6e7f', 'Cay Trong Nha', 'cay-trong-nha'),
    ('a5064cc6-406f-94a1-dd63-7f8a9b0c1d2e', '50b1f7d1-9b1a-4f5c-8d1e-2a3b4c5d6e7f', 'Cay Ngoai Troi', 'cay-ngoai-troi'),
    ('b6175dd7-5170-a5b2-ee74-8a9b0c1d2e3f', '50b1f7d1-9b1a-4f5c-8d1e-2a3b4c5d6e7f', 'Cay Sen Da', 'cay-sen-da'),
    ('c7286ee8-6281-b6c3-ff85-9b0c1d2e3f40', '50b1f7d1-9b1a-4f5c-8d1e-2a3b4c5d6e7f', 'Cay Phong Thuy', 'cay-phong-thuy'),

    -- Children - Chau Cay
    ('d8397ff9-7392-c7d4-aa96-0c1d2e3f4051', '61c208e2-0c2b-506d-9e2f-3b4c5d6e7f8a', 'Chau Su', 'chau-su'),
    ('e94a8aa0-84a3-d8e5-bb07-1d2e3f405162', '61c208e2-0c2b-506d-9e2f-3b4c5d6e7f8a', 'Chau Nhua', 'chau-nhua'),
    ('fa5b9bb1-95b4-e9f6-cc18-2e3f40516273', '61c208e2-0c2b-506d-9e2f-3b4c5d6e7f8a', 'Chau Xi Mang', 'chau-xi-mang'),

    -- Children - Phan Bon & Dat
    ('0b6cacc2-a6c5-f007-dd29-3f4051627384', '72d319f3-1d3c-617e-af30-4c5d6e7f8a9b', 'Phan Bon Huu Co', 'phan-bon-huu-co'),
    ('1c7dbdd3-b7d6-0118-ee30-405162738495', '72d319f3-1d3c-617e-af30-4c5d6e7f8a9b', 'Dat Trong', 'dat-trong'),

    -- Children - Dung Cu
    ('2d8ecee4-c8e7-1229-ff41-5162738495a6', '83e42aa4-2e4d-728f-ba41-5d6e7f8a9b0c', 'Keo Cat Tinh', 'keo-cat-tinh'),
    ('3e9fdff5-d9f8-2330-aa52-62738495a6b7', '83e42aa4-2e4d-728f-ba41-5d6e7f8a9b0c', 'Binh Tuoi Cay', 'binh-tuoi-cay')
ON CONFLICT (category_id) DO NOTHING;

-- ============================================
-- 4. PRODUCTS (Sample - 10 products)
-- ============================================
INSERT INTO products (product_id, category_id, name, description, base_price, thumbnail_url, is_active, created_at, updated_at)
VALUES
    ('a1b2c3d4-e5f6-4071-8082-111111111111', '94f53bb5-3f5e-8390-cc52-6e7f8a9b0c1d', 'Cay Kim Tien', 'Cay kim tien de trong, mang lai tai loc', 150000, 'https://placehold.co/400x400?text=Kim+Tien', true, NOW(), NOW()),
    ('b2c3d4e5-f6a7-4182-9193-222222222222', '94f53bb5-3f5e-8390-cc52-6e7f8a9b0c1d', 'Cay Luoi Ho', 'Cay luoi ho loc khong khi', 120000, 'https://placehold.co/400x400?text=Luoi+Ho', true, NOW(), NOW()),
    ('c3d4e5f6-a7b8-4293-a2a4-333333333333', '94f53bb5-3f5e-8390-cc52-6e7f8a9b0c1d', 'Cay Trau Ba', 'Cay trau ba de cham soc', 80000, 'https://placehold.co/400x400?text=Trau+Ba', true, NOW(), NOW()),
    ('e5f6a7b8-c9d0-44b5-c4c6-555555555555', '94f53bb5-3f5e-8390-cc52-6e7f8a9b0c1d', 'Cay Monstera', 'Phong cach nhiet doi', 350000, 'https://placehold.co/400x400?text=Monstera', true, NOW(), NOW()),
    ('f6a7b8c9-d0e1-45c6-d5d7-666666666666', 'b6175dd7-5170-a5b2-ee74-8a9b0c1d2e3f', 'Sen Da Hong', 'Sen da hong xinh xan', 45000, 'https://placehold.co/400x400?text=Sen+Da+Hong', true, NOW(), NOW()),
    ('29d0e1f2-a3b4-48f9-080a-999999999999', 'c7286ee8-6281-b6c3-ff85-9b0c1d2e3f40', 'Cay Phat Tai', 'Mang lai may man', 450000, 'https://placehold.co/400x400?text=Phat+Tai', true, NOW(), NOW()),
    ('4bf2a3b4-c5d6-501b-2a2c-aaaa1111bbbb', 'd8397ff9-7392-c7d4-aa96-0c1d2e3f4051', 'Chau Su Trang Tron', 'Chau su trang co dien', 85000, 'https://placehold.co/400x400?text=Chau+Su+Trang', true, NOW(), NOW()),
    ('9047f8a9-b0c1-5560-7f71-901290129012', '0b6cacc2-a6c5-f007-dd29-3f4051627384', 'Phan Bon NPK 20-20-20', 'Phan bon da nang', 65000, 'https://placehold.co/400x400?text=NPK', true, NOW(), NOW()),
    ('c37ac1d2-e3f4-5893-a2a4-abcdabcdef01', '2d8ecee4-c8e7-1229-ff41-5162738495a6', 'Keo Cat Tinh Nhat', 'Thep khong gi', 250000, 'https://placehold.co/400x400?text=Keo+Cat', true, NOW(), NOW()),
    ('d48bd2e3-f4a5-59a4-b3b5-bcdef0123456', '3e9fdff5-d9f8-2330-aa52-62738495a6b7', 'Binh Tuoi Cay 2L', 'Dau phun suong', 95000, 'https://placehold.co/400x400?text=Binh+Tuoi', true, NOW(), NOW())
ON CONFLICT (product_id) DO NOTHING;

-- ============================================
-- 5. PRODUCT VARIANTS
-- ============================================
INSERT INTO product_variants (variant_id, product_id, sku, price, stock_quantity, image_url, color, size_or_model, is_active, created_at, updated_at)
VALUES
    -- Kim Tien (S, M, L)
    ('50a41708-4688-46c5-8f43-a6b12c85e2b4', 'a1b2c3d4-e5f6-4071-8082-111111111111', 'KT-S-001', 150000, 50, NULL, NULL, 'S (20-30cm)', true, NOW(), NOW()),
    ('526e84d7-e07b-4009-8580-534cdd367010', 'a1b2c3d4-e5f6-4071-8082-111111111111', 'KT-M-001', 250000, 30, NULL, NULL, 'M (40-50cm)', true, NOW(), NOW()),
    ('d9c15033-902e-49b0-aa75-680455c1b5a2', 'a1b2c3d4-e5f6-4071-8082-111111111111', 'KT-L-001', 450000, 15, NULL, NULL, 'L (60-80cm)', true, NOW(), NOW()),

    -- Luoi Ho (S, M)
    ('c4573197-8c44-46c5-a681-705856758410', 'b2c3d4e5-f6a7-4182-9193-222222222222', 'LH-S-001', 120000, 40, NULL, NULL, 'S (15-20cm)', true, NOW(), NOW()),
    ('8e4f3b2a-5a6d-7e9f-0b1c-2d3e4f5a6b7c', 'b2c3d4e5-f6a7-4182-9193-222222222222', 'LH-M-001', 200000, 25, NULL, NULL, 'M (30-40cm)', true, NOW(), NOW()),

    -- Trau Ba (S, M)
    ('98b4c730-1092-4919-8646-9d2266856012', 'c3d4e5f6-a7b8-4293-a2a4-333333333333', 'TB-S-001', 80000, 60, NULL, NULL, 'S (10-15cm)', true, NOW(), NOW()),

    -- Monstera (M, L)
    ('d9907923-3809-4074-b49d-541297059781', 'e5f6a7b8-c9d0-44b5-c4c6-555555555555', 'MON-M-001', 350000, 15, NULL, NULL, 'M (40-50cm)', true, NOW(), NOW()),
    ('d9f5a4c3-6b7e-8f0a-1c2d-3e4f5a6b7c8d', 'e5f6a7b8-c9d0-44b5-c4c6-555555555555', 'MON-L-001', 550000, 8, NULL, NULL, 'L (60-80cm)', true, NOW(), NOW()),

    -- Sen Da Hong
    ('e0a6b5d4-7c8f-9a1b-2d3e-4f5a6b7c8d9e', 'f6a7b8c9-d0e1-45c6-d5d7-666666666666', 'SDH-S-001', 45000, 100, NULL, 'Hong', 'S (5-7cm)', true, NOW(), NOW()),

    -- Phat Tai (M, L)
    ('3f108643-0952-4707-0038-560211028489', '29d0e1f2-a3b4-48f9-080a-999999999999', 'PT-M-001', 450000, 12, NULL, NULL, 'M (50-60cm)', true, NOW(), NOW()),
    ('b3713060-e8f0-4107-b286-318465565369', '29d0e1f2-a3b4-48f9-080a-999999999999', 'PT-L-001', 750000, 5, NULL, NULL, 'L (80-100cm)', true, NOW(), NOW()),

    -- Chau Su Trang (S, M, L)
    ('6c239754-1063-4818-1149-671322139590', '4bf2a3b4-c5d6-501b-2a2c-aaaa1111bbbb', 'CST-S-001', 85000, 40, NULL, 'Trang', 'S (12cm)', true, NOW(), NOW()),
    ('f1a1b1c1-d1e1-f1a1-b1c1-d1e1f1a1b1c1', '4bf2a3b4-c5d6-501b-2a2c-aaaa1111bbbb', 'CST-M-001', 120000, 30, NULL, 'Trang', 'M (18cm)', true, NOW(), NOW()),

    -- Phan Bon NPK
    ('5f128643-0952-4707-0038-560211028489', '9047f8a9-b0c1-5560-7f71-901290129012', 'NPK-500G', 65000, 100, NULL, NULL, '500g', true, NOW(), NOW()),

    -- Keo Cat
    ('0e673198-5407-4252-5583-015766573934', 'c37ac1d2-e3f4-5893-a2a4-abcdabcdef01', 'KCT-180MM', 250000, 25, NULL, NULL, '180mm', true, NOW(), NOW()),

    -- Binh Tuoi
    ('23456789-0abc-def1-2345-67890abcdef1', 'd48bd2e3-f4a5-59a4-b3b5-bcdef0123456', 'BTC-2L-001', 95000, 45, NULL, 'Xanh', '2L', true, NOW(), NOW())
ON CONFLICT (variant_id) DO NOTHING;

-- ============================================
-- 6. PROMOTIONS
-- ============================================
INSERT INTO promotions (promotion_id, code, name, description, discount_type, discount_value, max_discount, min_order_value, max_usage, used_count, is_active, start_date, end_date, created_at, updated_at)
VALUES
    ('4a51e607-e832-4e4c-9f6b-76b4a3328512', 'NEWUSER', 'Giam 50K cho khach moi', 'Ap dung cho don hang dau tien', 'Fixed', 50000, NULL, 100000, 1000, 0, true, NOW() - INTERVAL '1 day', NOW() + INTERVAL '365 days', NOW(), NOW()),
    ('7c18a932-5d41-47fb-8926-15e8d91c7a23', 'SALE20', 'Giam 20% toi da 100K', 'Ap dung cho don tu 200K', 'Percentage', 20, 100000, 200000, 500, 0, true, NOW() - INTERVAL '1 day', NOW() + INTERVAL '30 days', NOW(), NOW()),
    ('9d20c541-6e52-48a0-9a37-26f9e02d8b34', 'VIP10', 'Giam 10% khong gioi han', 'Danh cho khach VIP', 'Percentage', 10, NULL, 500000, 100, 0, true, NOW() - INTERVAL '1 day', NOW() + INTERVAL '90 days', NOW(), NOW()),
    ('be31d652-7f63-49b1-ab48-37a0f13e9c45', 'FREESHIP', 'Mien phi ship 30K', 'Ap dung cho don tu 150K', 'Fixed', 30000, NULL, 150000, NULL, 0, true, NOW() - INTERVAL '1 day', NOW() + INTERVAL '60 days', NOW(), NOW())
ON CONFLICT (promotion_id) DO NOTHING;

-- ============================================
-- 7. CARTS
-- ============================================
INSERT INTO carts (cart_id, user_id, session_id, created_at, updated_at)
VALUES
    ('38400030-3101-4436-963d-425514605551', 'a8677c73-4560-466d-9c3f-42e557169623', NULL, NOW(), NOW()),
    ('99222471-1256-4293-8686-368725845922', 'c5695029-7987-4355-9b22-485764d9d832', NULL, NOW(), NOW()),
    ('b5585526-7241-4328-8746-814234057883', 'd2891390-8488-4444-9b20-67512760f351', NULL, NOW(), NOW())
ON CONFLICT (cart_id) DO NOTHING;

-- ============================================
-- 8. CART ITEMS
-- ============================================
INSERT INTO cart_items (cart_item_id, cart_id, variant_id, quantity, created_at)
VALUES
    ('a9761234-5678-4321-8765-123456789abc', '38400030-3101-4436-963d-425514605551', '526e84d7-e07b-4009-8580-534cdd367010', 1, NOW()),
    ('b0872345-6789-5432-9876-234567890bcd', '38400030-3101-4436-963d-425514605551', 'f1a1b1c1-d1e1-f1a1-b1c1-d1e1f1a1b1c1', 1, NOW()),
    ('c1983456-7890-6543-0987-345678901cde', '99222471-1256-4293-8686-368725845922', 'e0a6b5d4-7c8f-9a1b-2d3e-4f5a6b7c8d9e', 3, NOW())
ON CONFLICT (cart_item_id) DO NOTHING;

-- ============================================
-- 9. ORDERS (with recipient info)
-- ============================================
INSERT INTO orders (order_id, user_id, sub_total, discount, voucher_code, shipping_fee, total_amount, final_amount, status, shipping_address_id, shipping_address, recipient_name, recipient_phone, note, created_at, updated_at, stock_reserved)
VALUES
    -- Completed order (Cus 1)
    ('b5722365-1153-4318-8742-706708687158', 'a8677c73-4560-466d-9c3f-42e557169623',
     400000, 50000, 'NEWUSER', 30000, 380000, 380000, 'Completed',
     '10a34b22-0546-4c44-8039-4b8c9d0e1f2a', '123 Nguyen Hue, Phuong Ben Nghe, Quan 1, TP. Ho Chi Minh',
     'Tran Minh', '0903334567', 'Giao gio hanh chinh',
     NOW() - INTERVAL '7 days', NOW() - INTERVAL '5 days', false),

    -- Processing order (Cus 2)
    ('25807952-1981-4325-9614-729906596131', 'c5695029-7987-4355-9b22-485764d9d832',
     280000, 56000, 'SALE20', 30000, 254000, 254000, 'Processing',
     '3c8d7e44-2768-6e66-a25b-6d0e1f2a3b4c', '789 Tran Hung Dao, Phuong 11, Quan 5, TP. Ho Chi Minh',
     'Le Hoa', '0904445678', 'Goi qua tang',
     NOW() - INTERVAL '1 day', NOW() - INTERVAL '1 day', true),

    -- Pending order (Cus 3)
    ('41595671-5582-4458-9419-455642231221', 'd2891390-8488-4444-9b20-67512760f351',
     350000, 0, NULL, 30000, 380000, 380000, 'Pending',
     '4d7e6f55-3879-7f77-b36c-7e1f2a3b4c5d', '321 Pham Ngu Lao, Phuong Pham Ngu Lao, Quan 1, TP. Ho Chi Minh',
     'Pham Anh', '0905556789', NULL,
     NOW() - INTERVAL '30 minutes', NOW() - INTERVAL '30 minutes', true)
ON CONFLICT (order_id) DO NOTHING;

-- ============================================
-- 10. ORDER ITEMS
-- ============================================
INSERT INTO order_items (item_id, order_id, variant_id, quantity, price_at_purchase, custom_data)
VALUES
    ('0a1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d', 'b5722365-1153-4318-8742-706708687158', '526e84d7-e07b-4009-8580-534cdd367010', 1, 250000, NULL),
    ('1b2c3d4e-5f6a-7b8c-9d0e-1f2a3b4c5d6e', 'b5722365-1153-4318-8742-706708687158', '8e4f3b2a-5a6d-7e9f-0b1c-2d3e4f5a6b7c', 1, 200000, NULL),
    ('3d4e5f6a-7b8c-9d0e-1f2a-3b4c5d6e7f8a', '25807952-1981-4325-9614-729906596131', 'd9907923-3809-4074-b49d-541297059781', 1, 350000, NULL),
    ('5f6a7b8c-9d0e-1f2a-3b4c-5d6e7f8a9b0c', '41595671-5582-4458-9419-455642231221', 'd9907923-3809-4074-b49d-541297059781', 1, 350000, NULL)
ON CONFLICT (item_id) DO NOTHING;

-- ============================================
-- 11. PAYMENTS
-- ============================================
INSERT INTO payments (payment_id, order_id, payment_method, amount, transaction_code, status, paid_at, bank_code, gateway, transaction_ref, created_at, updated_at)
VALUES
    ('11235813-2134-5589-1442-333776109871', 'b5722365-1153-4318-8742-706708687158', 'VNPay', 380000, 'VNP14123456789', 'Success', NOW() - INTERVAL '7 days', 'NCB', 'VNPay', 'ORD-01234567-1', NOW() - INTERVAL '7 days', NOW() - INTERVAL '7 days'),
    ('33445566-7788-99aa-bbcc-ddeeff001122', '25807952-1981-4325-9614-729906596131', 'VNPay', 254000, 'VNP14234567890', 'Success', NOW() - INTERVAL '1 day', 'BIDV', 'VNPay', 'ORD-23456789-1', NOW() - INTERVAL '1 day', NOW() - INTERVAL '1 day'),
    ('55667788-99aa-bbcc-ddee-ff0011223344', '41595671-5582-4458-9419-455642231221', 'VNPay', 380000, NULL, 'Pending', NULL, NULL, 'VNPay', 'ORD-456789AB-1', NOW() - INTERVAL '30 minutes', NOW() - INTERVAL '30 minutes')
ON CONFLICT (payment_id) DO NOTHING;

-- ============================================
-- 12. RATINGS
-- ============================================
INSERT INTO ratings (rating_id, product_id, user_id, stars, comment, create_date)
VALUES
    ('71360548-1254-4785-9854-123654789541', 'a1b2c3d4-e5f6-4071-8082-111111111111', 'a8677c73-4560-466d-9c3f-42e557169623', 5, 'Cay rat dep, giao hang nhanh!', NOW() - INTERVAL '5 days'),
    ('82471659-2365-5896-0965-234765890652', 'a1b2c3d4-e5f6-4071-8082-111111111111', 'c5695029-7987-4355-9b22-485764d9d832', 4.5, 'Cay xanh tot, dong goi can than', NOW() - INTERVAL '10 days'),
    ('93582760-3476-6907-1076-345876901763', 'e5f6a7b8-c9d0-44b5-c4c6-555555555555', 'a8677c73-4560-466d-9c3f-42e557169623', 5, 'Monstera la to dep qua!', NOW() - INTERVAL '12 days')
ON CONFLICT (rating_id) DO NOTHING;

-- ============================================
-- VERIFICATION
-- ============================================
SELECT 'Seed data completed!' as result,
       (SELECT COUNT(*) FROM users) as users_count,
       (SELECT COUNT(*) FROM user_address) as addresses_count,
       (SELECT COUNT(*) FROM categories) as categories_count,
       (SELECT COUNT(*) FROM products) as products_count,
       (SELECT COUNT(*) FROM product_variants) as variants_count,
       (SELECT COUNT(*) FROM orders) as orders_count;
