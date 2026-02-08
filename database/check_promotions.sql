-- Check all promotions in database
SELECT
    code,
    name,
    is_active,
    start_date,
    end_date,
    NOW() as current_time,
    (start_date <= NOW()) as started,
    (end_date >= NOW()) as not_expired,
    max_usage,
    used_count,
    (max_usage IS NULL OR used_count < max_usage) as has_usage_left
FROM promotions
ORDER BY code;

-- Check which ones pass all filters
SELECT
    code,
    name
FROM promotions
WHERE is_active = true
  AND (start_date IS NULL OR start_date <= NOW())
  AND (end_date IS NULL OR end_date >= NOW())
  AND (max_usage IS NULL OR used_count < max_usage);
