-- Benchmark: isti agregat kao CompanyRepository.GetAnalyticsResultsAsync
-- (GET /api/Company/{id}/Analytics).
--
-- 1) U WITH params zameniti company_id i opseg datuma (istim vrednostima kao u dashboardu / API pozivu).
-- 2) U psql: \timing on   pa pokrenuti ceo fajl (ili samo blok SELECT ispod).
-- 3) Ispod: jedan ispravan SQL — EXPLAIN (ANALYZE) izvršava upit i na kraju plana piše "Execution Time: ... ms".
--    Za samo rezultat (bez plana), zameni prvi red sa:  WITH params AS (

EXPLAIN (ANALYZE, BUFFERS, FORMAT TEXT)
WITH params AS (
    SELECT
        '00000000-0000-0000-0000-000000000001'::uuid AS company_id,
        '2026-04-01 00:00:00+00'::timestamptz AS start_date,
        '2026-04-30 23:59:59.999+00'::timestamptz AS end_date
)
SELECT
    COALESCE(SUM(CASE WHEN l.viber_status NOT IN (0, 1) THEN 1 ELSE 0 END), 0) AS viber_total_sent,
    COALESCE(SUM(CASE WHEN l.viber_status IN (3, 4, 7) THEN 1 ELSE 0 END), 0) AS viber_delivered,
    COALESCE(SUM(CASE WHEN l.viber_status = 5 THEN 1 ELSE 0 END), 0) AS viber_undelivered,
    COALESCE(SUM(CASE WHEN l.viber_status = 6 THEN 1 ELSE 0 END), 0) AS viber_expired,
    COALESCE(SUM(CASE WHEN l.viber_status IN (4, 7) THEN 1 ELSE 0 END), 0) AS viber_seen,
    COALESCE(SUM(CASE WHEN l.viber_status = 7 THEN 1 ELSE 0 END), 0) AS viber_clicked,
    COALESCE(SUM(CASE WHEN l.sms_status != 0 THEN 1 ELSE 0 END), 0) AS sms_total_sent,
    COALESCE(SUM(CASE WHEN l.sms_status = 1 THEN 1 ELSE 0 END), 0) AS sms_delivered,
    COALESCE(SUM(CASE WHEN l.sms_status = 2 THEN 1 ELSE 0 END), 0) AS sms_failed,
    COALESCE(COUNT(DISTINCT le.phone_number), 0) AS unique_phone_numbers,
    COALESCE((
        SELECT COUNT(DISTINCT le1.phone_number)
        FROM campaign_leads l1
        JOIN leads le1 ON le1.id = l1.lead_id
        JOIN campaigns c1 ON c1.id = l1.campaign_id
        WHERE c1.company_id = (SELECT company_id FROM params)
          AND c1.sending_datetime >= (SELECT start_date FROM params)
          AND c1.sending_datetime <= (SELECT end_date FROM params)
          AND c1.status = 4
          AND le1.is_unsubscribed = true
    ), 0) AS total_unsubscribed,
    COALESCE((
        SELECT COUNT(*)
        FROM test_messages t1
        WHERE t1.company_id = (SELECT company_id FROM params)
          AND t1.created_at >= (SELECT start_date FROM params)
          AND t1.created_at <= (SELECT end_date FROM params)
          AND t1.is_delivered = true
          AND t1.is_viber = true
    ), 0) AS number_of_tests_viber,
    COALESCE((
        SELECT COUNT(*)
        FROM test_messages t1
        WHERE t1.company_id = (SELECT company_id FROM params)
          AND t1.created_at >= (SELECT start_date FROM params)
          AND t1.created_at <= (SELECT end_date FROM params)
          AND t1.is_delivered = true
          AND t1.is_viber = true
          AND t1.is_clicked = true
    ), 0) AS number_of_tests_viber_clicked,
    COALESCE((
        SELECT COUNT(*)
        FROM test_messages t1
        WHERE t1.company_id = (SELECT company_id FROM params)
          AND t1.created_at >= (SELECT start_date FROM params)
          AND t1.created_at <= (SELECT end_date FROM params)
          AND t1.is_delivered = true
          AND t1.is_viber = false
    ), 0) AS number_of_tests_sms,
    COALESCE((
        SELECT COUNT(*)
        FROM api_messages api_1
        WHERE api_1.company_id = (SELECT company_id FROM params)
          AND api_1.created_at >= (SELECT start_date FROM params)
          AND api_1.created_at <= (SELECT end_date FROM params)
          AND api_1.message_type = 1
          AND api_1.viber_status IN (3, 4, 7)
    ), 0) AS number_of_api_viber,
    COALESCE((
        SELECT COUNT(*)
        FROM api_messages api_1
        WHERE api_1.company_id = (SELECT company_id FROM params)
          AND api_1.created_at >= (SELECT start_date FROM params)
          AND api_1.created_at <= (SELECT end_date FROM params)
          AND api_1.message_type = 1
          AND api_1.viber_status = 7
    ), 0) AS number_of_api_viber_clicked,
    COALESCE((
        SELECT COUNT(*)
        FROM api_messages api_1
        WHERE api_1.company_id = (SELECT company_id FROM params)
          AND api_1.created_at >= (SELECT start_date FROM params)
          AND api_1.created_at <= (SELECT end_date FROM params)
          AND api_1.message_type = 2
          AND api_1.sms_status = 1
    ), 0) AS number_of_api_sms,
    COALESCE(COUNT(DISTINCT c.id), 0) AS number_of_campaigns,
    COALESCE(COUNT(*), 0) AS viber_total_leads,
    COALESCE(SUM(CASE WHEN l.viber_status = 0 THEN 1 ELSE 0 END), 0) AS viber_not_sent,
    COALESCE(SUM(CASE WHEN l.viber_status = 1 THEN 1 ELSE 0 END), 0) AS viber_received,
    COALESCE(SUM(CASE WHEN l.viber_status = 3 THEN 1 ELSE 0 END), 0) AS viber_delivered_only
FROM campaign_leads l
JOIN leads le ON le.id = l.lead_id
JOIN campaigns c ON c.id = l.campaign_id
WHERE c.company_id = (SELECT company_id FROM params)
  AND c.sending_datetime >= (SELECT start_date FROM params)
  AND c.sending_datetime <= (SELECT end_date FROM params)
  AND c.status = 4;
