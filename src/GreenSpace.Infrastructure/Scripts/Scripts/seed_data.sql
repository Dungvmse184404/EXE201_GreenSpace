-- ============================================
-- GreenSpace Full Seed Data Script (Fixed UUIDs)
-- Run this on Supabase SQL Editor
-- ============================================

-- ============================================
-- CLEAR EXISTING DATA
-- ============================================
TRUNCATE TABLE cart_items, carts, order_items, orders, payments, ratings,
    product_attribute_values, product_variants, products,
    categories, attributes, promotions, user_address, refresh_tokens
    CASCADE;

-- ============================================
-- 1. USERS
-- ============================================
INSERT INTO users (user_id, username, email, password_hash, first_name, last_name, phone, role, gender, is_active, create_at, update_at)
VALUES
    ('94c69894-6840-42f6-953b-014529329709', 'staff01', 'staff01@greenspace.vn', '$2a$11$rBNxPfv.YhBRFDHxXjGYeO5FgVrXXmrCqPVn0YQwVJOZGGWcOFRMu', 'Nguyen', 'Staff', '0902234567', 'Staff', 'Female', true, NOW(), NOW()),
    ('a8677c73-4560-466d-9c3f-42e557169623', 'customer01', 'customer01@gmail.com', '$2a$11$rBNxPfv.YhBRFDHxXjGYeO5FgVrXXmrCqPVn0YQwVJOZGGWcOFRMu', 'Tran', 'Minh', '0903334567', 'Customer', 'Male', true, NOW(), NOW()),
    ('c5695029-7987-4355-9b22-485764d9d832', 'customer02', 'customer02@gmail.com', '$2a$11$rBNxPfv.YhBRFDHxXjGYeO5FgVrXXmrCqPVn0YQwVJOZGGWcOFRMu', 'Le', 'Hoa', '0904445678', 'Customer', 'Female', true, NOW(), NOW()),
    ('d2891390-8488-4444-9b20-67512760f351', 'customer03', 'customer03@gmail.com', '$2a$11$rBNxPfv.YhBRFDHxXjGYeO5FgVrXXmrCqPVn0YQwVJOZGGWcOFRMu', 'Pham', 'Anh', '0905556789', 'Customer', 'Male', true, NOW(), NOW())
ON CONFLICT (user_id) DO NOTHING;

-- ============================================
-- 2. USER ADDRESSES (New schema with province, district, ward)
-- ============================================
INSERT INTO user_address (address_id, user_id, province, district, ward, street_address, label, is_default, created_at)
VALUES
    -- Customer 01 (a8677c73...): 2 addresses
    ('10a34b22-0546-4c44-8039-4b8c9d0e1f2a', 'a8677c73-4560-466d-9c3f-42e557169623',
     'TP. Ho Chi Minh', 'Quan 1', 'Phuong Ben Nghe', '123 Nguyen Hue', 'Nha', true, NOW()),
    ('2b9c8d33-1657-5d55-914a-5c9d0e1f2a3b', 'a8677c73-4560-466d-9c3f-42e557169623',
     'TP. Ho Chi Minh', 'Quan 3', 'Phuong 7', '456 Le Loi', 'Cong ty', false, NOW()),

    -- Customer 02 (c5695029...): 2 addresses
    ('3c8d7e44-2768-6e66-a25b-6d0e1f2a3b4c', 'c5695029-7987-4355-9b22-485764d9d832',
     'TP. Ho Chi Minh', 'Quan 5', 'Phuong 11', '789 Tran Hung Dao', 'Nha', true, NOW()),
    ('5e9f0a11-4980-8088-c47d-8f0a1b2c3d4e', 'c5695029-7987-4355-9b22-485764d9d832',
     'TP. Ho Chi Minh', 'Quan 7', 'Phuong Tan Phu', '100 Nguyen Van Linh, Sunrise City', 'Nha ban', false, NOW()),

    -- Customer 03 (d2891390...): 2 addresses
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
    -- Parent Categories (Cleaned UUIDs)
    ('50b1f7d1-9b1a-4f5c-8d1e-2a3b4c5d6e7f', NULL, 'Cay Canh', 'cay-canh'),
    ('61c208e2-0c2b-506d-9e2f-3b4c5d6e7f8a', NULL, 'Chau Cay', 'chau-cay'),
    ('72d319f3-1d3c-617e-af30-4c5d6e7f8a9b', NULL, 'Phan Bon & Dat', 'phan-bon-dat'),
    ('83e42aa4-2e4d-728f-ba41-5d6e7f8a9b0c', NULL, 'Dung Cu Lam Vuon', 'dung-cu-lam-vuon'), -- Fixed 'g'

    -- Children - Cay Canh (Fixed 'h','i','j','k')
    ('94f53bb5-3f5e-8390-cc52-6e7f8a9b0c1d', '50b1f7d1-9b1a-4f5c-8d1e-2a3b4c5d6e7f', 'Cay Trong Nha', 'cay-trong-nha'),
    ('a5064cc6-406f-94a1-dd63-7f8a9b0c1d2e', '50b1f7d1-9b1a-4f5c-8d1e-2a3b4c5d6e7f', 'Cay Ngoai Troi', 'cay-ngoai-troi'),
    ('b6175dd7-5170-a5b2-ee74-8a9b0c1d2e3f', '50b1f7d1-9b1a-4f5c-8d1e-2a3b4c5d6e7f', 'Cay Sen Da', 'cay-sen-da'),
    ('c7286ee8-6281-b6c3-ff85-9b0c1d2e3f40', '50b1f7d1-9b1a-4f5c-8d1e-2a3b4c5d6e7f', 'Cay Phong Thuy', 'cay-phong-thuy'),

    -- Children - Chau Cay (Fixed 'l','m','n')
    ('d8397ff9-7392-c7d4-aa96-0c1d2e3f4051', '61c208e2-0c2b-506d-9e2f-3b4c5d6e7f8a', 'Chau Su', 'chau-su'),
    ('e94a8aa0-84a3-d8e5-bb07-1d2e3f405162', '61c208e2-0c2b-506d-9e2f-3b4c5d6e7f8a', 'Chau Nhua', 'chau-nhua'),
    ('fa5b9bb1-95b4-e9f6-cc18-2e3f40516273', '61c208e2-0c2b-506d-9e2f-3b4c5d6e7f8a', 'Chau Xi Mang', 'chau-xi-mang'),

    -- Children - Phan Bon & Dat (Fixed 'o','p')
    ('0b6cacc2-a6c5-f007-dd29-3f4051627384', '72d319f3-1d3c-617e-af30-4c5d6e7f8a9b', 'Phan Bon Huu Co', 'phan-bon-huu-co'),
    ('1c7dbdd3-b7d6-0118-ee30-405162738495', '72d319f3-1d3c-617e-af30-4c5d6e7f8a9b', 'Dat Trong', 'dat-trong'),

    -- Children - Dung Cu (Fixed 'q','r')
    ('2d8ecee4-c8e7-1229-ff41-5162738495a6', '83e42aa4-2e4d-728f-ba41-5d6e7f8a9b0c', 'Keo Cat Tinh', 'keo-cat-tinh'),
    ('3e9fdff5-d9f8-2330-aa52-62738495a6b7', '83e42aa4-2e4d-728f-ba41-5d6e7f8a9b0c', 'Binh Tuoi Cay', 'binh-tuoi-cay')
ON CONFLICT (category_id) DO NOTHING;

-- ============================================
-- 4. ATTRIBUTES
-- ============================================
INSERT INTO attributes (attribute_id, name, data_type)
VALUES
    ('9f8e7d6c-5b4a-3f2e-1d0c-9b8a7c6d5e4f', 'Kich Thuoc', 'string'),
    ('8e7d6c5b-4a3f-2e1d-0c9b-8a7c6d5e4f3a', 'Chat Lieu', 'string'),
    ('7d6c5b4a-3f2e-1d0c-9b8a-7c6d5e4f3a2b', 'Xuat Xu', 'string'),
    ('6c5b4a3f-2e1d-0c9b-8a7c-6d5e4f3a2b1c', 'Do Kho Cham Soc', 'string'),
    ('5b4a3f2e-1d0c-9b8a-7c6d-5e4f3a2b1c0d', 'Anh Sang', 'string'),
    ('4a3f2e1d-0c9b-8a7c-6d5e-4f3a2b1c0d9e', 'Trong Luong', 'string')
ON CONFLICT (attribute_id) DO NOTHING;

-- ============================================
-- 5. PRODUCTS
-- ============================================
INSERT INTO products (product_id, category_id, name, description, base_price, thumbnail_url, is_active, created_at, updated_at)
VALUES
    -- Cay Trong Nha (Category: 94f53bb5...)
    ('a1b2c3d4-e5f6-4071-8082-111111111111', '94f53bb5-3f5e-8390-cc52-6e7f8a9b0c1d', 'Cay Kim Tien', 'Cay kim tien de trong, mang lai tai loc', 150000, 'https://placehold.co/400x400?text=Kim+Tien', true, NOW(), NOW()),
    ('b2c3d4e5-f6a7-4182-9193-222222222222', '94f53bb5-3f5e-8390-cc52-6e7f8a9b0c1d', 'Cay Luoi Ho', 'Cay luoi ho loc khong khi', 120000, 'https://placehold.co/400x400?text=Luoi+Ho', true, NOW(), NOW()),
    ('c3d4e5f6-a7b8-4293-a2a4-333333333333', '94f53bb5-3f5e-8390-cc52-6e7f8a9b0c1d', 'Cay Trau Ba', 'Cay trau ba de cham soc', 80000, 'https://placehold.co/400x400?text=Trau+Ba', true, NOW(), NOW()),
    ('d4e5f6a7-b8c9-43a4-b3b5-444444444444', '94f53bb5-3f5e-8390-cc52-6e7f8a9b0c1d', 'Cay Van Nien Thanh', 'Xanh quanh nam', 200000, 'https://placehold.co/400x400?text=Van+Nien+Thanh', true, NOW(), NOW()),
    ('e5f6a7b8-c9d0-44b5-c4c6-555555555555', '94f53bb5-3f5e-8390-cc52-6e7f8a9b0c1d', 'Cay Monstera', 'Phong cach nhiet doi', 350000, 'https://placehold.co/400x400?text=Monstera', true, NOW(), NOW()),

    -- Cay Sen Da (Category: b6175dd7...)
    ('f6a7b8c9-d0e1-45c6-d5d7-666666666666', 'b6175dd7-5170-a5b2-ee74-8a9b0c1d2e3f', 'Sen Da Hong', 'Sen da hong xinh xan', 45000, 'https://placehold.co/400x400?text=Sen+Da+Hong', true, NOW(), NOW()),
    ('07b8c9d0-e1f2-46d7-e6e8-777777777777', 'b6175dd7-5170-a5b2-ee74-8a9b0c1d2e3f', 'Sen Da Tim', 'Sen da tim quyen ru', 50000, 'https://placehold.co/400x400?text=Sen+Da+Tim', true, NOW(), NOW()),
    ('18c9d0e1-f2a3-47e8-f7f9-888888888888', 'b6175dd7-5170-a5b2-ee74-8a9b0c1d2e3f', 'Xuong Rong Mini', 'Xuong rong mini nhieu mau', 35000, 'https://placehold.co/400x400?text=Xuong+Rong', true, NOW(), NOW()),

    -- Cay Phong Thuy (Category: c7286ee8...)
    ('29d0e1f2-a3b4-48f9-080a-999999999999', 'c7286ee8-6281-b6c3-ff85-9b0c1d2e3f40', 'Cay Phat Tai', 'Mang lai may man', 450000, 'https://placehold.co/400x400?text=Phat+Tai', true, NOW(), NOW()),
    ('3ae1f2a3-b4c5-490a-191b-000000000000', 'c7286ee8-6281-b6c3-ff85-9b0c1d2e3f40', 'Cay Hanh Phuc', 'Bieu tuong am no', 280000, 'https://placehold.co/400x400?text=Hanh+Phuc', true, NOW(), NOW()),

    -- Chau Su (Category: d8397ff9...)
    ('4bf2a3b4-c5d6-501b-2a2c-aaaa1111bbbb', 'd8397ff9-7392-c7d4-aa96-0c1d2e3f4051', 'Chau Su Trang Tron', 'Chau su trang co dien', 85000, 'https://placehold.co/400x400?text=Chau+Su+Trang', true, NOW(), NOW()),
    ('5c03b4c5-d6e7-512c-3b3d-cccc2222dddd', 'd8397ff9-7392-c7d4-aa96-0c1d2e3f4051', 'Chau Su Hoa Van', 'Chau su hoa van doc dao', 120000, 'https://placehold.co/400x400?text=Chau+Su+Hoa+Van', true, NOW(), NOW()),
    ('6d14c5d6-e7f8-523d-4c4e-eeee3333ffff', 'd8397ff9-7392-c7d4-aa96-0c1d2e3f4051', 'Chau Su Men Xanh', 'Chau su men xanh ngoc', 150000, 'https://placehold.co/400x400?text=Chau+Men+Xanh', true, NOW(), NOW()),

    -- Chau Nhua (Category: e94a8aa0...)
    ('7e25d6e7-f8a9-534e-5d5f-123412341234', 'e94a8aa0-84a3-d8e5-bb07-1d2e3f405162', 'Chau Nhua Tu Tuoi', 'Chau nhua thong minh', 180000, 'https://placehold.co/400x400?text=Chau+Tu+Tuoi', true, NOW(), NOW()),
    ('8f36e7f8-a9b0-545f-6e60-567856785678', 'e94a8aa0-84a3-d8e5-bb07-1d2e3f405162', 'Chau Nhua Treo', 'Tiet kiem khong gian', 75000, 'https://placehold.co/400x400?text=Chau+Treo', true, NOW(), NOW()),

    -- Phan Bon & Dat (Category: 0b6cacc2...)
    ('9047f8a9-b0c1-5560-7f71-901290129012', '0b6cacc2-a6c5-f007-dd29-3f4051627384', 'Phan Bon NPK 20-20-20', 'Phan bon da nang', 65000, 'https://placehold.co/400x400?text=NPK', true, NOW(), NOW()),
    ('a158a9b0-c1d2-5671-8082-345634563456', '0b6cacc2-a6c5-f007-dd29-3f4051627384', 'Phan Trun Que', 'Huu co 100%', 45000, 'https://placehold.co/400x400?text=Trun+Que', true, NOW(), NOW()),
    ('b269b0c1-d2e3-5782-9193-789078907890', '1c7dbdd3-b7d6-0118-ee30-405162738495', 'Dat Trong Sen Da', 'Chuyen dung cho sen da', 35000, 'https://placehold.co/400x400?text=Dat+Sen+Da', true, NOW(), NOW()),

    -- Dung Cu (Category: 2d8ecee4...)
    ('c37ac1d2-e3f4-5893-a2a4-abcdabcdef01', '2d8ecee4-c8e7-1229-ff41-5162738495a6', 'Keo Cat Tinh Nhat', 'Thep khong gi', 250000, 'https://placehold.co/400x400?text=Keo+Cat', true, NOW(), NOW()),
    ('d48bd2e3-f4a5-59a4-b3b5-bcdef0123456', '3e9fdff5-d9f8-2330-aa52-62738495a6b7', 'Binh Tuoi Cay 2L', 'Dau phun suong', 95000, 'https://placehold.co/400x400?text=Binh+Tuoi', true, NOW(), NOW())
ON CONFLICT (product_id) DO NOTHING;

-- ============================================
-- 6. PRODUCT VARIANTS
-- ============================================
INSERT INTO product_variants (variant_id, product_id, sku, price, stock_quantity, image_url, color, size_or_model, is_active, created_at, updated_at)
VALUES
    -- Kim Tien (S, M, L)
    ('50a41708-4688-46c5-8f43-a6b12c85e2b4', 'a1b2c3d4-e5f6-4071-8082-111111111111', 'KT-S-001', 150000, 50, NULL, NULL, 'S (20-30cm)', true, NOW(), NOW()),
    ('526e84d7-e07b-4009-8580-534cdd367010', 'a1b2c3d4-e5f6-4071-8082-111111111111', 'KT-M-001', 250000, 30, NULL, NULL, 'M (40-50cm)', true, NOW(), NOW()),
    ('d9c15033-902e-49b0-aa75-680455c1b5a2', 'a1b2c3d4-e5f6-4071-8082-111111111111', 'KT-L-001', 450000, 15, NULL, NULL, 'L (60-80cm)', true, NOW(), NOW()),

    -- Luoi Ho
    ('c4573197-8c44-46c5-a681-705856758410', 'b2c3d4e5-f6a7-4182-9193-222222222222', 'LH-S-001', 120000, 40, NULL, NULL, 'S (15-20cm)', true, NOW(), NOW()),
    ('8e4f3b2a-5a6d-7e9f-0b1c-2d3e4f5a6b7c', 'b2c3d4e5-f6a7-4182-9193-222222222222', 'LH-M-001', 200000, 25, NULL, NULL, 'M (30-40cm)', true, NOW(), NOW()),

    -- Trau Ba
    ('98b4c730-1092-4919-8646-9d2266856012', 'c3d4e5f6-a7b8-4293-a2a4-333333333333', 'TB-S-001', 80000, 60, NULL, NULL, 'S (10-15cm)', true, NOW(), NOW()),
    ('aa234059-3286-4441-9492-f04655462823', 'c3d4e5f6-a7b8-4293-a2a4-333333333333', 'TB-M-001', 150000, 35, NULL, NULL, 'M (20-30cm)', true, NOW(), NOW()),

    -- Van Nien Thanh
    ('bb653198-5407-4452-9593-015766573934', 'd4e5f6a7-b8c9-43a4-b3b5-444444444444', 'VNT-M-001', 200000, 20, NULL, NULL, 'M (30-40cm)', true, NOW(), NOW()),
    ('cc764209-6518-4463-9694-126877684045', 'd4e5f6a7-b8c9-43a4-b3b5-444444444444', 'VNT-L-001', 350000, 10, NULL, NULL, 'L (50-60cm)', true, NOW(), NOW()),

    -- Monstera
    ('d9907923-3809-4074-b49d-541297059781', 'e5f6a7b8-c9d0-44b5-c4c6-555555555555', 'MON-M-001', 350000, 15, NULL, NULL, 'M (40-50cm)', true, NOW(), NOW()),
    ('d9f5a4c3-6b7e-8f0a-1c2d-3e4f5a6b7c8d', 'e5f6a7b8-c9d0-44b5-c4c6-555555555555', 'MON-L-001', 550000, 8, NULL, NULL, 'L (60-80cm)', true, NOW(), NOW()),

    -- Sen Da Hong
    ('e0a6b5d4-7c8f-9a1b-2d3e-4f5a6b7c8d9e', 'f6a7b8c9-d0e1-45c6-d5d7-666666666666', 'SDH-S-001', 45000, 100, NULL, 'Hong', 'S (5-7cm)', true, NOW(), NOW()),

    -- Sen Da Tim
    ('4b01625e-3987-4d6b-bd85-6e0332822456', '07b8c9d0-e1f2-46d7-e6e8-777777777777', 'SDT-S-001', 50000, 80, NULL, 'Tim', 'S (5-7cm)', true, NOW(), NOW()),

    -- Xuong Rong Mini
    ('1d986421-8730-4585-9816-348099806267', '18c9d0e1-f2a3-47e8-f7f9-888888888888', 'XR-S-001', 35000, 150, NULL, NULL, 'Mini (3-5cm)', true, NOW(), NOW()),
    ('2e097532-9841-4696-9927-459100917378', '18c9d0e1-f2a3-47e8-f7f9-888888888888', 'XR-S-002', 55000, 80, NULL, NULL, 'S (5-8cm)', true, NOW(), NOW()),

    -- Phat Tai
    ('3f108643-0952-4707-0038-560211028489', '29d0e1f2-a3b4-48f9-080a-999999999999', 'PT-M-001', 450000, 12, NULL, NULL, 'M (50-60cm)', true, NOW(), NOW()),
    ('b3713060-e8f0-4107-b286-318465565369', '29d0e1f2-a3b4-48f9-080a-999999999999', 'PT-L-001', 750000, 5, NULL, NULL, 'L (80-100cm)', true, NOW(), NOW()),

    -- Hanh Phuc
    ('2a590895-35c2-4809-9b98-508077553556', '3ae1f2a3-b4c5-490a-191b-000000000000', 'HP-M-001', 280000, 18, NULL, NULL, 'M (40-50cm)', true, NOW(), NOW()),

    -- Chau Su Trang Tron
    ('6c239754-1063-4818-1149-671322139590', '4bf2a3b4-c5d6-501b-2a2c-aaaa1111bbbb', 'CST-S-001', 85000, 40, NULL, 'Trang', 'S (12cm)', true, NOW(), NOW()),
    ('f1a1b1c1-d1e1-f1a1-b1c1-d1e1f1a1b1c1', '4bf2a3b4-c5d6-501b-2a2c-aaaa1111bbbb', 'CST-M-001', 120000, 30, NULL, 'Trang', 'M (18cm)', true, NOW(), NOW()),
    ('7d340865-2174-4929-2250-782433240601', '4bf2a3b4-c5d6-501b-2a2c-aaaa1111bbbb', 'CST-L-001', 180000, 20, NULL, 'Trang', 'L (25cm)', true, NOW(), NOW()),

    -- Chau Su Hoa Van
    ('8e451976-3285-4030-3361-893544351712', '5c03b4c5-d6e7-512c-3b3d-cccc2222dddd', 'CSHV-M-001', 120000, 25, NULL, NULL, 'M (15cm)', true, NOW(), NOW()),
    ('9f562087-4396-4141-4472-904655462823', '5c03b4c5-d6e7-512c-3b3d-cccc2222dddd', 'CSHV-L-001', 180000, 15, NULL, NULL, 'L (22cm)', true, NOW(), NOW()),

    -- Chau Su Men Xanh (Fixed '0g' to '0a')
    ('0a673198-5407-4252-5583-015766573934', '6d14c5d6-e7f8-523d-4c4e-eeee3333ffff', 'CSMX-M-001', 150000, 20, NULL, 'Xanh Ngoc', 'M (18cm)', true, NOW(), NOW()),

    -- Chau Nhua Tu Tuoi (Fixed '1h','2i' to '1b','2c')
    ('1b784209-6518-4363-6694-126877684045', '7e25d6e7-f8a9-534e-5d5f-123412341234', 'CNTT-M-001', 180000, 35, NULL, 'Trang', 'M (20cm)', true, NOW(), NOW()),
    ('2c895310-7629-4474-7705-237988795156', '7e25d6e7-f8a9-534e-5d5f-123412341234', 'CNTT-L-001', 280000, 20, NULL, 'Trang', 'L (30cm)', true, NOW(), NOW()),

    -- Chau Nhua Treo (Fixed '3j','4k' to '3d','4e')
    ('3d906421-8730-4585-8816-348099806267', '8f36e7f8-a9b0-545f-6e60-567856785678', 'CNT-S-001', 75000, 50, NULL, 'Trang', 'S (15cm)', true, NOW(), NOW()),
    ('4e017532-9841-4696-9927-459100917378', '8f36e7f8-a9b0-545f-6e60-567856785678', 'CNT-S-002', 75000, 40, NULL, 'Den', 'S (15cm)', true, NOW(), NOW()),

    -- Phan Bon NPK (Fixed '5l','6m' to '5f','6a')
    ('5f128643-0952-4707-0038-560211028489', '9047f8a9-b0c1-5560-7f71-901290129012', 'NPK-500G', 65000, 100, NULL, NULL, '500g', true, NOW(), NOW()),
    ('6a239754-1063-4818-1149-671322139590', '9047f8a9-b0c1-5560-7f71-901290129012', 'NPK-1KG', 110000, 60, NULL, NULL, '1kg', true, NOW(), NOW()),

    -- Phan Trun Que (Fixed '7n','8o' to '7b','8c')
    ('7b340865-2174-4929-2250-782433240601', 'a158a9b0-c1d2-5671-8082-345634563456', 'PTQ-1KG', 45000, 80, NULL, NULL, '1kg', true, NOW(), NOW()),
    ('8c451976-3285-4030-3361-893544351712', 'a158a9b0-c1d2-5671-8082-345634563456', 'PTQ-5KG', 180000, 30, NULL, NULL, '5kg', true, NOW(), NOW()),

    -- Dat Trong Sen Da (Fixed '9p' to '9d')
    ('12345678-90ab-cdef-1234-567890abcdef', 'b269b0c1-d2e3-5782-9193-789078907890', 'DTSD-2KG', 35000, 100, NULL, NULL, '2kg', true, NOW(), NOW()),
    ('9d562087-4396-4141-4472-904655462823', 'b269b0c1-d2e3-5782-9193-789078907890', 'DTSD-5KG', 75000, 50, NULL, NULL, '5kg', true, NOW(), NOW()),

    -- Keo Cat Tinh (Fixed '0q','1r' to '0e','1f')
    ('0e673198-5407-4252-5583-015766573934', 'c37ac1d2-e3f4-5893-a2a4-abcdabcdef01', 'KCT-180MM', 250000, 25, NULL, NULL, '180mm', true, NOW(), NOW()),
    ('1f784209-6518-4363-6694-126877684045', 'c37ac1d2-e3f4-5893-a2a4-abcdabcdef01', 'KCT-200MM', 320000, 15, NULL, NULL, '200mm', true, NOW(), NOW()),

    -- Binh Tuoi (Fixed '2s' to '2a')
    ('23456789-0abc-def1-2345-67890abcdef1', 'd48bd2e3-f4a5-59a4-b3b5-bcdef0123456', 'BTC-2L-001', 95000, 45, NULL, 'Xanh', '2L', true, NOW(), NOW()),
    ('2a895310-7629-4474-7705-237988795156', 'd48bd2e3-f4a5-59a4-b3b5-bcdef0123456', 'BTC-2L-002', 95000, 40, NULL, 'Hong', '2L', true, NOW(), NOW())
ON CONFLICT (variant_id) DO NOTHING;

-- ============================================
-- 7. PRODUCT ATTRIBUTE VALUES
-- ============================================
INSERT INTO product_attribute_values (value_id, product_id, attribute_id, value)
VALUES
    -- Kim Tien attributes
    ('4a1b5c2d-9e3f-608a-1b4d-5c7e9f0a2b3c', 'a1b2c3d4-e5f6-4071-8082-111111111111', '6c5b4a3f-2e1d-0c9b-8a7c-6d5e4f3a2b1c', 'De'),
    ('5b2c6d3e-0f4a-719b-2c5e-6d8f0a1b3c4d', 'a1b2c3d4-e5f6-4071-8082-111111111111', '5b4a3f2e-1d0c-9b8a-7c6d-5e4f3a2b1c0d', 'Ban bong'),
    ('6c3d7e4f-1a5b-82ac-3d6f-7e9a1b2c4d5e', 'a1b2c3d4-e5f6-4071-8082-111111111111', '7d6c5b4a-3f2e-1d0c-9b8a-7c6d5e4f3a2b', 'Viet Nam'),

    -- Monstera attributes
    ('7d4e8f5a-2b6c-93bd-4e7a-8f0b2c3d5e6f', 'e5f6a7b8-c9d0-44b5-c4c6-555555555555', '6c5b4a3f-2e1d-0c9b-8a7c-6d5e4f3a2b1c', 'Trung binh'),
    ('8e5f9a6b-3c7d-04ce-5f8b-9a1c3d4e6f7a', 'e5f6a7b8-c9d0-44b5-c4c6-555555555555', '5b4a3f2e-1d0c-9b8a-7c6d-5e4f3a2b1c0d', 'Anh sang tan xa'),

    -- Chau Su Trang attributes (Fixed IDs)
    ('9f6a0b7c-4d8e-15df-6a9c-0b2d4e5f7a8b', '4bf2a3b4-c5d6-501b-2a2c-aaaa1111bbbb', '8e7d6c5b-4a3f-2e1d-0c9b-8a7c6d5e4f3a', 'Su cao cap'),
    ('0a7b1c8d-5e9f-26ea-7b0d-1c3e5f6a8b9c', '4bf2a3b4-c5d6-501b-2a2c-aaaa1111bbbb', '7d6c5b4a-3f2e-1d0c-9b8a-7c6d5e4f3a2b', 'Bat Trang'),

    -- Keo Cat attributes (Fixed IDs)
    ('1b8c2d9e-6f0a-37fb-8c1e-2d4f6a7b9c0d', 'c37ac1d2-e3f4-5893-a2a4-abcdabcdef01', '8e7d6c5b-4a3f-2e1d-0c9b-8a7c6d5e4f3a', 'Thep khong gi'),
    ('2c9d3e0f-7a1b-48ac-9d2f-3e5a7b8c0d1e', 'c37ac1d2-e3f4-5893-a2a4-abcdabcdef01', '7d6c5b4a-3f2e-1d0c-9b8a-7c6d5e4f3a2b', 'Nhat Ban')
ON CONFLICT (value_id) DO NOTHING;

-- ============================================
-- 8. PROMOTIONS
-- ============================================
INSERT INTO promotions (promotion_id, code, name, description, discount_type, discount_value, max_discount, min_order_value, max_usage, used_count, is_active, start_date, end_date, created_at, updated_at)
VALUES
    ('4a51e607-e832-4e4c-9f6b-76b4a3328512', 'NEWUSER', 'Giam 50K cho khach moi', 'Ap dung cho don hang dau tien', 'Fixed', 50000, NULL, 100000, 1000, 0, true, NOW() - INTERVAL '1 day', NOW() + INTERVAL '365 days', NOW(), NOW()),
    ('7c18a932-5d41-47fb-8926-15e8d91c7a23', 'SALE20', 'Giam 20% toi da 100K', 'Ap dung cho don tu 200K', 'Percentage', 20, 100000, 200000, 500, 0, true, NOW() - INTERVAL '1 day', NOW() + INTERVAL '30 days', NOW(), NOW()),
    ('9d20c541-6e52-48a0-9a37-26f9e02d8b34', 'VIP10', 'Giam 10% khong gioi han', 'Danh cho khach VIP', 'Percentage', 10, NULL, 500000, 100, 0, true, NOW() - INTERVAL '1 day', NOW() + INTERVAL '90 days', NOW(), NOW()),
    ('be31d652-7f63-49b1-ab48-37a0f13e9c45', 'FREESHIP', 'Mien phi ship 30K', 'Ap dung cho don tu 150K', 'Fixed', 30000, NULL, 150000, NULL, 0, true, NOW() - INTERVAL '1 day', NOW() + INTERVAL '60 days', NOW(), NOW()),
    ('df42e763-8074-4ac2-bc59-48b1024fad56', 'FLASH50', 'Flash Sale giam 50K', 'Khuyen mai co thoi han', 'Fixed', 50000, NULL, 200000, 50, 10, true, NOW() - INTERVAL '1 day', NOW() + INTERVAL '3 days', NOW(), NOW()),
    ('0153f874-9185-4bd3-cd60-59c2135abe67', 'LIMITED', 'Voucher gioi han 5 luot', 'Chi con vai luot', 'Percentage', 30, 150000, 300000, 5, 4, true, NOW() - INTERVAL '1 day', NOW() + INTERVAL '30 days', NOW(), NOW()),
    ('12640985-a296-4ce4-de71-6ad3246bcf78', 'UPCOMING', 'Sap co khuyen mai', 'Voucher chua bat dau', 'Percentage', 25, 200000, 400000, 200, 0, true, NOW() + INTERVAL '7 days', NOW() + INTERVAL '14 days', NOW(), NOW()),
    ('23751a96-b307-4df5-ef82-7be4357cda89', 'EXPIRED', 'Da het han', 'Voucher het han', 'Fixed', 100000, NULL, 100000, 100, 50, true, NOW() - INTERVAL '30 days', NOW() - INTERVAL '1 day', NOW(), NOW()),
    ('34862ba7-c418-4ea6-fa93-8cf5468beb90', 'DISABLED', 'Voucher tam ngung', 'Da bi vo hieu hoa', 'Percentage', 15, 50000, 100000, 100, 0, false, NOW() - INTERVAL '1 day', NOW() + INTERVAL '30 days', NOW(), NOW())
ON CONFLICT (promotion_id) DO NOTHING;

-- ============================================
-- 9. CARTS
-- ============================================
INSERT INTO carts (cart_id, user_id, session_id, created_at, updated_at)
VALUES
    ('38400030-3101-4436-963d-425514605551', 'a8677c73-4560-466d-9c3f-42e557169623', NULL, NOW(), NOW()),
    ('99222471-1256-4293-8686-368725845922', 'c5695029-7987-4355-9b22-485764d9d832', NULL, NOW(), NOW()),
    ('b5585526-7241-4328-8746-814234057883', 'd2891390-8488-4444-9b20-67512760f351', NULL, NOW(), NOW()),
    ('66311651-6151-4682-9653-535562769914', NULL, 'guest-session-12345', NOW(), NOW())
ON CONFLICT (cart_id) DO NOTHING;

-- ============================================
-- 10. CART ITEMS
-- ============================================
INSERT INTO cart_items (cart_item_id, cart_id, variant_id, quantity, created_at)
VALUES
    -- Customer 1 cart (Kim Tien M + Chau Su M)
    ('a9761234-5678-4321-8765-123456789abc', '38400030-3101-4436-963d-425514605551', '526e84d7-e07b-4009-8580-534cdd367010', 1, NOW()),
    ('b0872345-6789-5432-9876-234567890bcd', '38400030-3101-4436-963d-425514605551', 'f1a1b1c1-d1e1-f1a1-b1c1-d1e1f1a1b1c1', 1, NOW()),

    -- Customer 2 cart (Sen Da Hong + Dat Trong)
    ('c1983456-7890-6543-0987-345678901cde', '99222471-1256-4293-8686-368725845922', 'e0a6b5d4-7c8f-9a1b-2d3e-4f5a6b7c8d9e', 3, NOW()),
    ('d2094567-8901-7654-1098-456789012def', '99222471-1256-4293-8686-368725845922', '12345678-90ab-cdef-1234-567890abcdef', 1, NOW()),

    -- Guest cart (Binh Tuoi)
    ('e31a5678-9012-8765-2109-567890123ef0', '66311651-6151-4682-9653-535562769914', '23456789-0abc-def1-2345-67890abcdef1', 2, NOW())
ON CONFLICT (cart_item_id) DO NOTHING;

-- ============================================
-- 11. ORDERS (with recipient info)
-- ============================================
INSERT INTO orders (order_id, user_id, sub_total, discount, voucher_code, shipping_fee, total_amount, final_amount, status, shipping_address_id, shipping_address, recipient_name, recipient_phone, note, created_at, updated_at, stock_reserved)
VALUES
    -- Completed order with voucher (Cus 1)
    ('b5722365-1153-4318-8742-706708687158', 'a8677c73-4560-466d-9c3f-42e557169623',
     400000, 50000, 'NEWUSER', 30000, 380000, 380000, 'Completed',
     '10a34b22-0546-4c44-8039-4b8c9d0e1f2a', '123 Nguyen Hue, Phuong Ben Nghe, Quan 1, TP. Ho Chi Minh',
     'Tran Minh', '0903334567', 'Giao gio hanh chinh',
     NOW() - INTERVAL '7 days', NOW() - INTERVAL '5 days', false),

    -- Completed order without voucher - different address (Cus 1)
    ('69411993-2775-4081-9b19-f52985873995', 'a8677c73-4560-466d-9c3f-42e557169623',
     550000, 0, NULL, 30000, 580000, 580000, 'Completed',
     '2b9c8d33-1657-5d55-914a-5c9d0e1f2a3b', '456 Le Loi, Phuong 7, Quan 3, TP. Ho Chi Minh',
     'Nguyen Van A', '0909123456', NULL,
     NOW() - INTERVAL '14 days', NOW() - INTERVAL '12 days', false),

    -- Processing order (Cus 2)
    ('25807952-1981-4325-9614-729906596131', 'c5695029-7987-4355-9b22-485764d9d832',
     280000, 56000, 'SALE20', 30000, 254000, 254000, 'Processing',
     '3c8d7e44-2768-6e66-a25b-6d0e1f2a3b4c', '789 Tran Hung Dao, Phuong 11, Quan 5, TP. Ho Chi Minh',
     'Le Hoa', '0904445678', 'Goi qua tang',
     NOW() - INTERVAL '1 day', NOW() - INTERVAL '1 day', true),

    -- Shipping order (Cus 2)
    ('97805175-6852-4017-9152-426868512252', 'c5695029-7987-4355-9b22-485764d9d832',
     750000, 75000, 'VIP10', 30000, 705000, 705000, 'Shipping',
     '3c8d7e44-2768-6e66-a25b-6d0e1f2a3b4c', '789 Tran Hung Dao, Phuong 11, Quan 5, TP. Ho Chi Minh',
     'Le Hoa', '0904445678', NULL,
     NOW() - INTERVAL '3 days', NOW() - INTERVAL '2 days', true),

    -- Pending payment order (Cus 3)
    ('41595671-5582-4458-9419-455642231221', 'd2891390-8488-4444-9b20-67512760f351',
     350000, 0, NULL, 30000, 380000, 380000, 'Pending',
     '4d7e6f55-3879-7f77-b36c-7e1f2a3b4c5d', '321 Pham Ngu Lao, Phuong Pham Ngu Lao, Quan 1, TP. Ho Chi Minh',
     'Pham Anh', '0905556789', NULL,
     NOW() - INTERVAL '30 minutes', NOW() - INTERVAL '30 minutes', true),

    -- Cancelled order (Cus 3) - no address_id (direct input)
    ('76518884-3325-4521-8854-325946112545', 'd2891390-8488-4444-9b20-67512760f351',
     200000, 0, NULL, 30000, 230000, 230000, 'Cancelled',
     NULL, '321 Pham Ngu Lao, Quan 1, TP.HCM',
     'Pham Anh', '0905556789', 'Khach huy',
     NOW() - INTERVAL '10 days', NOW() - INTERVAL '9 days', false)
ON CONFLICT (order_id) DO NOTHING;

-- ============================================
-- 12. ORDER ITEMS
-- ============================================
INSERT INTO order_items (item_id, order_id, variant_id, quantity, price_at_purchase, custom_data)
VALUES
    -- Order 1 items (Kim Tien M + Luoi Ho M) - Fixed invalid hex chars
    ('0a1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d', 'b5722365-1153-4318-8742-706708687158', '526e84d7-e07b-4009-8580-534cdd367010', 1, 250000, NULL),
    ('1b2c3d4e-5f6a-7b8c-9d0e-1f2a3b4c5d6e', 'b5722365-1153-4318-8742-706708687158', '8e4f3b2a-5a6d-7e9f-0b1c-2d3e4f5a6b7c', 1, 200000, NULL),

    -- Order 2 items (Monstera L)
    ('2c3d4e5f-6a7b-8c9d-0e1f-2a3b4c5d6e7f', '69411993-2775-4081-9b19-f52985873995', 'd9f5a4c3-6b7e-8f0a-1c2d-3e4f5a6b7c8d', 1, 550000, NULL),

    -- Order 3 items (Hanh Phuc M)
    ('3d4e5f6a-7b8c-9d0e-1f2a-3b4c5d6e7f8a', '25807952-1981-4325-9614-729906596131', '2a590895-35c2-4809-9b98-508077553556', 1, 280000, NULL),

    -- Order 4 items (Phat Tai L)
    ('4e5f6a7b-8c9d-0e1f-2a3b-4c5d6e7f8a9b', '97805175-6852-4017-9152-426868512252', 'b3713060-e8f0-4107-b286-318465565369', 1, 750000, NULL),

    -- Order 5 items (Monstera M)
    ('5f6a7b8c-9d0e-1f2a-3b4c-5d6e7f8a9b0c', '41595671-5582-4458-9419-455642231221', 'd9907923-3809-4074-b49d-541297059781', 1, 350000, NULL),

    -- Order 6 items (Luoi Ho S x2)
    ('6a7b8c9d-0e1f-2a3b-4c5d-6e7f8a9b0c1d', '76518884-3325-4521-8854-325946112545', 'c4573197-8c44-46c5-a681-705856758410', 2, 120000, NULL)
ON CONFLICT (item_id) DO NOTHING;

-- ============================================
-- 13. PAYMENTS
-- ============================================
INSERT INTO payments (payment_id, order_id, payment_method, amount, transaction_code, status, paid_at, bank_code, gateway, transaction_ref, created_at, updated_at)
VALUES
    -- Completed payment - VNPay
    ('11235813-2134-5589-1442-333776109871', 'b5722365-1153-4318-8742-706708687158', 'VNPay', 380000, 'VNP14123456789', 'Success', NOW() - INTERVAL '7 days', 'NCB', 'VNPay', 'ORD-01234567-1', NOW() - INTERVAL '7 days', NOW() - INTERVAL '7 days'),

    -- Completed payment - PayOS
    ('22334455-6677-8899-aabb-ccddeeff0011', '69411993-2775-4081-9b19-f52985873995', 'PayOS', 580000, 'PAY987654321', 'Success', NOW() - INTERVAL '14 days', NULL, 'PayOS', 'ORD-12345678-1', NOW() - INTERVAL '14 days', NOW() - INTERVAL '14 days'),

    -- Processing payment
    ('33445566-7788-99aa-bbcc-ddeeff001122', '25807952-1981-4325-9614-729906596131', 'VNPay', 254000, 'VNP14234567890', 'Success', NOW() - INTERVAL '1 day', 'BIDV', 'VNPay', 'ORD-23456789-1', NOW() - INTERVAL '1 day', NOW() - INTERVAL '1 day'),

    -- Shipping order payment
    ('44556677-8899-aabb-ccdd-eeff00112233', '97805175-6852-4017-9152-426868512252', 'PayOS', 705000, 'PAY123456789', 'Success', NOW() - INTERVAL '3 days', NULL, 'PayOS', 'ORD-3456789A-1', NOW() - INTERVAL '3 days', NOW() - INTERVAL '3 days'),

    -- Pending payment
    ('55667788-99aa-bbcc-ddee-ff0011223344', '41595671-5582-4458-9419-455642231221', 'VNPay', 380000, NULL, 'Pending', NULL, NULL, 'VNPay', 'ORD-456789AB-1', NOW() - INTERVAL '30 minutes', NOW() - INTERVAL '30 minutes'),

    -- Failed payment (cancelled order)
    ('66778899-aabb-ccdd-eeff-001122334455', '76518884-3325-4521-8854-325946112545', 'PayOS', 230000, NULL, 'Failed', NULL, NULL, 'PayOS', 'ORD-56789ABC-1', NOW() - INTERVAL '10 days', NOW() - INTERVAL '9 days')
ON CONFLICT (payment_id) DO NOTHING;

-- ============================================
-- 14. RATINGS
-- ============================================
INSERT INTO ratings (rating_id, product_id, user_id, stars, comment, create_date)
VALUES
    -- Kim Tien ratings
    ('71360548-1254-4785-9854-123654789541', 'a1b2c3d4-e5f6-4071-8082-111111111111', 'a8677c73-4560-466d-9c3f-42e557169623', 5, 'Cay rat dep, giao hang nhanh!', NOW() - INTERVAL '5 days'),
    ('82471659-2365-5896-0965-234765890652', 'a1b2c3d4-e5f6-4071-8082-111111111111', 'c5695029-7987-4355-9b22-485764d9d832', 4.5, 'Cay xanh tot, dong goi can than', NOW() - INTERVAL '10 days'),

    -- Monstera ratings
    ('93582760-3476-6907-1076-345876901763', 'e5f6a7b8-c9d0-44b5-c4c6-555555555555', 'a8677c73-4560-466d-9c3f-42e557169623', 5, 'Monstera la to dep qua!', NOW() - INTERVAL '12 days'),

    -- Luoi Ho ratings
    ('a4693871-4587-7018-2187-456987012874', 'b2c3d4e5-f6a7-4182-9193-222222222222', 'd2891390-8488-4444-9b20-67512760f351', 4, 'Cay khoe, de cham soc', NOW() - INTERVAL '8 days'),

    -- Sen Da ratings
    ('b5704982-5698-8129-3298-567098123985', 'f6a7b8c9-d0e1-45c6-d5d7-666666666666', 'c5695029-7987-4355-9b22-485764d9d832', 5, 'Sen da hong xinh lam, mua them!', NOW() - INTERVAL '6 days'),
    ('c6815093-6709-9230-4309-678109234096', '07b8c9d0-e1f2-46d7-e6e8-777777777777', 'a8677c73-4560-466d-9c3f-42e557169623', 4.5, 'Mau tim rat dep', NOW() - INTERVAL '4 days'),

    -- Chau Su ratings
    ('d7926104-7810-0341-5410-789210345107', '4bf2a3b4-c5d6-501b-2a2c-aaaa1111bbbb', 'c5695029-7987-4355-9b22-485764d9d832', 5, 'Chau dep, chat luong tot', NOW() - INTERVAL '9 days'),
    ('e8037215-8921-1452-6521-890321456218', '5c03b4c5-d6e7-512c-3b3d-cccc2222dddd', 'd2891390-8488-4444-9b20-67512760f351', 4, 'Hoa van doc dao', NOW() - INTERVAL '11 days'),

    -- Phat Tai ratings
    ('f9148326-9032-2563-7632-901432567329', '29d0e1f2-a3b4-48f9-080a-999999999999', 'c5695029-7987-4355-9b22-485764d9d832', 5, 'Cay Phat Tai rat dep, hy vong mang lai tai loc!', NOW() - INTERVAL '2 days'),

    -- Binh Tuoi ratings
    ('0a259437-0143-3674-8743-012543678430', 'd48bd2e3-f4a5-59a4-b3b5-bcdef0123456', 'a8677c73-4560-466d-9c3f-42e557169623', 4.5, 'Binh tuoi tien loi, phun suong tot', NOW() - INTERVAL '7 days')
ON CONFLICT (rating_id) DO NOTHING;

SELECT 'Seed data inserted successfully !' as result;