-- Menja samo login; password_hash se ne dira.
-- EF/Npgsql koristi tabelu "AspNetUsers" i kolone u snake_case (email, user_name, ...).

UPDATE "AspNetUsers"
SET
  user_name = 'itocs@oneclick.rs',
  normalized_user_name = UPPER('itocs@oneclick.rs'),
  email = 'itocs@oneclick.rs',
  normalized_email = UPPER('itocs@oneclick.rs')
WHERE lower(email) LIKE '%@blackbird.rs'
   OR lower(user_name) LIKE '%@blackbird.rs';

-- Provera:
-- SELECT user_name, email, LEFT(password_hash, 20) AS hash_prefix FROM "AspNetUsers";
