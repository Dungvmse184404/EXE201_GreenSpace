-- ============================================
-- FULL MIGRATION FOR SUPABASE
-- Run this in Supabase SQL Editor
-- ============================================

-- ============================================
-- 1. ORDERS TABLE - Simplify Pricing
-- ============================================

-- Add new columns for simplified pricing
ALTER TABLE orders ADD COLUMN IF NOT EXISTS sub_total NUMERIC(10, 2) DEFAULT 0;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS discount NUMERIC(10, 2) DEFAULT 0;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS voucher_code VARCHAR(50);
ALTER TABLE orders ADD COLUMN IF NOT EXISTS shipping_fee NUMERIC(10, 2) DEFAULT 0;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS final_amount NUMERIC(10, 2) DEFAULT 0;
ALTER TABLE orders ADD COLUMN IF NOT EXISTS note VARCHAR(500);
ALTER TABLE orders ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP;

-- Migrate data: combine old discounts into single 'discount' column (if old columns exist)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'orders' AND column_name = 'product_discount') THEN
        UPDATE orders SET
            discount = COALESCE(product_discount, 0) + COALESCE(voucher_discount, 0) + COALESCE(points_used, 0)
        WHERE discount = 0 OR discount IS NULL;
    END IF;
END $$;

-- Recalculate final_amount
UPDATE orders SET
    final_amount = GREATEST(0, COALESCE(sub_total, 0) - COALESCE(discount, 0) + COALESCE(shipping_fee, 0)),
    total_amount = GREATEST(0, COALESCE(sub_total, 0) - COALESCE(discount, 0) + COALESCE(shipping_fee, 0))
WHERE final_amount = 0 OR final_amount IS NULL;

-- Drop old columns (optional - uncomment if you want to clean up)
-- ALTER TABLE orders DROP COLUMN IF EXISTS shipping_discount;
-- ALTER TABLE orders DROP COLUMN IF EXISTS product_discount;
-- ALTER TABLE orders DROP COLUMN IF EXISTS voucher_discount;
-- ALTER TABLE orders DROP COLUMN IF EXISTS points_used;

-- ============================================
-- 2. PROMOTIONS TABLE - Enhance for Voucher System
-- ============================================

-- Add new columns for voucher system
ALTER TABLE promotions ADD COLUMN IF NOT EXISTS code VARCHAR(50);
ALTER TABLE promotions ADD COLUMN IF NOT EXISTS name VARCHAR(100);
ALTER TABLE promotions ADD COLUMN IF NOT EXISTS description VARCHAR(500);
ALTER TABLE promotions ADD COLUMN IF NOT EXISTS discount_value NUMERIC(10, 2) DEFAULT 0;
ALTER TABLE promotions ADD COLUMN IF NOT EXISTS max_discount NUMERIC(10, 2);
ALTER TABLE promotions ADD COLUMN IF NOT EXISTS min_order_value NUMERIC(10, 2);
ALTER TABLE promotions ADD COLUMN IF NOT EXISTS max_usage INTEGER;
ALTER TABLE promotions ADD COLUMN IF NOT EXISTS used_count INTEGER DEFAULT 0;
ALTER TABLE promotions ADD COLUMN IF NOT EXISTS created_at TIMESTAMP;
ALTER TABLE promotions ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP;

-- Make is_active non-nullable with default
ALTER TABLE promotions ALTER COLUMN is_active SET DEFAULT true;
UPDATE promotions SET is_active = true WHERE is_active IS NULL;
ALTER TABLE promotions ALTER COLUMN is_active SET NOT NULL;

-- Migrate old discount_amount to discount_value (if not already migrated)
UPDATE promotions SET
    discount_value = COALESCE(discount_amount, 0)
WHERE discount_value = 0 OR discount_value IS NULL;

-- Create unique index on code (for voucher lookup)
CREATE UNIQUE INDEX IF NOT EXISTS idx_promotions_code ON promotions(code) WHERE code IS NOT NULL;

-- ============================================
-- 3. INSERT SAMPLE VOUCHERS
-- ============================================

-- Only insert if no vouchers exist
INSERT INTO promotions (
    promotion_id, code, name, description,
    discount_type, discount_value, max_discount, min_order_value,
    max_usage, used_count, is_active,
    start_date, end_date, created_at, updated_at,
    order_id, discount_amount, create_at, update_at
)
SELECT
    gen_random_uuid(), 'SALE20', 'Giảm 20% tối đa 50k', 'Áp dụng cho đơn hàng từ 200,000đ',
    'Percentage', 20, 50000, 200000,
    100, 0, true,
    NOW(), NOW() + INTERVAL '30 days', NOW(), NOW(),
    NULL, 20, NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM promotions WHERE code = 'SALE20');

INSERT INTO promotions (
    promotion_id, code, name, description,
    discount_type, discount_value, max_discount, min_order_value,
    max_usage, used_count, is_active,
    start_date, end_date, created_at, updated_at,
    order_id, discount_amount, create_at, update_at
)
SELECT
    gen_random_uuid(), 'FLAT50K', 'Giảm ngay 50,000đ', 'Áp dụng cho đơn hàng từ 300,000đ',
    'Fixed', 50000, NULL, 300000,
    50, 0, true,
    NOW(), NOW() + INTERVAL '14 days', NOW(), NOW(),
    NULL, 50000, NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM promotions WHERE code = 'FLAT50K');

INSERT INTO promotions (
    promotion_id, code, name, description,
    discount_type, discount_value, max_discount, min_order_value,
    max_usage, used_count, is_active,
    start_date, end_date, created_at, updated_at,
    order_id, discount_amount, create_at, update_at
)
SELECT
    gen_random_uuid(), 'NEWUSER', 'Ưu đãi khách mới', 'Giảm 15% cho khách hàng mới, tối đa 100k',
    'Percentage', 15, 100000, 100000,
    NULL, 0, true,
    NOW() - INTERVAL '10 days', NOW() + INTERVAL '60 days', NOW(), NOW(),
    NULL, 15, NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM promotions WHERE code = 'NEWUSER');

-- ============================================
-- 4. VERIFICATION
-- ============================================

-- Check orders table structure
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'orders'
AND column_name IN ('sub_total', 'discount', 'voucher_code', 'shipping_fee', 'final_amount', 'note')
ORDER BY ordinal_position;

-- Check promotions table structure
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'promotions'
AND column_name IN ('code', 'name', 'discount_value', 'max_discount', 'min_order_value', 'max_usage', 'used_count')
ORDER BY ordinal_position;

-- List all vouchers
SELECT code, name, discount_type, discount_value, max_discount, min_order_value, max_usage, used_count, is_active
FROM promotions
WHERE code IS NOT NULL;
