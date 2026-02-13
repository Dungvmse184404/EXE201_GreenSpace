-- ============================================
-- Migration: Update user_address and orders table
-- Version: 2.0
-- Run this FIRST on Supabase SQL Editor
-- ============================================

-- ============================================
-- 1. UPDATE user_address TABLE
-- ============================================

-- Add new columns to user_address
ALTER TABLE user_address
ADD COLUMN IF NOT EXISTS province VARCHAR(100),
ADD COLUMN IF NOT EXISTS district VARCHAR(100),
ADD COLUMN IF NOT EXISTS ward VARCHAR(100),
ADD COLUMN IF NOT EXISTS street_address VARCHAR(255),
ADD COLUMN IF NOT EXISTS label VARCHAR(50),
ADD COLUMN IF NOT EXISTS is_default BOOLEAN DEFAULT false,
ADD COLUMN IF NOT EXISTS created_at TIMESTAMP WITHOUT TIME ZONE DEFAULT NOW(),
ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP WITHOUT TIME ZONE;

-- Migrate old address data to street_address and DROP old column
DO $$
BEGIN
    -- Check if old 'address' column exists
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'user_address' AND column_name = 'address') THEN
        -- Step 1: Migrate data from old 'address' to new 'street_address'
        UPDATE user_address
        SET street_address = address,
            province = 'TP. Ho Chi Minh',
            district = 'Unknown',
            ward = 'Unknown',
            is_default = true,
            created_at = NOW()
        WHERE street_address IS NULL AND address IS NOT NULL;

        -- Step 2: DROP the old 'address' column (no longer needed)
        ALTER TABLE user_address DROP COLUMN address;
        RAISE NOTICE 'Old "address" column migrated and dropped successfully.';
    END IF;
END $$;

-- ============================================
-- 2. UPDATE orders TABLE
-- ============================================

-- Add new columns to orders
ALTER TABLE orders
ADD COLUMN IF NOT EXISTS shipping_address_id UUID,
ADD COLUMN IF NOT EXISTS recipient_name VARCHAR(100),
ADD COLUMN IF NOT EXISTS recipient_phone VARCHAR(20);

-- Increase shipping_address length
ALTER TABLE orders
ALTER COLUMN shipping_address TYPE VARCHAR(500);

-- Add index for shipping_address_id
CREATE INDEX IF NOT EXISTS IX_orders_shipping_address_id ON orders(shipping_address_id);

-- ============================================
-- VERIFICATION
-- ============================================
SELECT
    'Migration completed!' as status,
    -- Check new columns added
    (SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'user_address' AND column_name = 'province') as province_added,
    (SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'user_address' AND column_name = 'district') as district_added,
    (SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'user_address' AND column_name = 'ward') as ward_added,
    (SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'user_address' AND column_name = 'street_address') as street_address_added,
    -- Check old 'address' column removed (should be 0)
    (SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'user_address' AND column_name = 'address') as old_address_removed,
    -- Check orders table updated
    (SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'orders' AND column_name = 'recipient_name') as recipient_name_added,
    (SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'orders' AND column_name = 'recipient_phone') as recipient_phone_added;
