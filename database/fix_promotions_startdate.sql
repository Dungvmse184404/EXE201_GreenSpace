-- ============================================
-- FIX: Update start_date for all promotions to start from past
-- Run this in Supabase SQL Editor
-- ============================================

-- Update all promotions to start from 1 day ago (so they are immediately active)
UPDATE promotions
SET start_date = NOW() - INTERVAL '1 day'
WHERE start_date > NOW() - INTERVAL '1 minute';

-- Verify the fix
SELECT
    code,
    name,
    is_active,
    start_date,
    end_date,
    NOW() as current_time,
    CASE WHEN start_date <= NOW() THEN 'YES' ELSE 'NO' END as started,
    CASE WHEN end_date >= NOW() THEN 'YES' ELSE 'NO' END as not_expired
FROM promotions
WHERE code IS NOT NULL
ORDER BY code;
