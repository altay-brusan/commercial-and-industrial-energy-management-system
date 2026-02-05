-- =====================================================
-- Reset Database Script
-- Truncates all application tables and resets sequences
-- =====================================================
-- WARNING: This script will DELETE ALL DATA from the database!
-- Only run this in development/testing environments.
-- This script is idempotent - safe to run multiple times.
-- =====================================================
-- Tables NOT affected:
--   - __EFMigrationsHistory (EF Core migration tracking)
--   - hangfire.* (Hangfire job queue tables)
-- =====================================================

BEGIN;

-- Disable triggers temporarily for faster truncation
SET session_replication_role = 'replica';

-- =====================================================
-- 1. Truncate Telemetry/History Data Tables
-- =====================================================
TRUNCATE TABLE "DeviceSolarData" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "DeviceBessData" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "DevicePcsData" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "DeviceSmartMeterData" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "DayAheadEnergyPriceHistories" RESTART IDENTITY CASCADE;

-- =====================================================
-- 2. Truncate Junction/Association Tables
-- =====================================================
TRUNCATE TABLE "UserFirmFacilities" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "FirmMarketOperators" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "FacilityTariffs" RESTART IDENTITY CASCADE;

-- =====================================================
-- 3. Truncate Device Tables
-- =====================================================
TRUNCATE TABLE "DeviceLinks" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "Devices" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "DeviceTypes" RESTART IDENTITY CASCADE;

-- =====================================================
-- 4. Truncate Core Entity Tables
-- =====================================================
TRUNCATE TABLE "Facilities" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "Firms" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "Plans" RESTART IDENTITY CASCADE;

-- =====================================================
-- 5. Truncate Market & Tariff Tables
-- =====================================================
TRUNCATE TABLE "Tariffs" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "MarketOperators" RESTART IDENTITY CASCADE;


-- =====================================================
-- 6. Truncate Identity Tables (Users, Roles, Claims)
-- Note: Using "App" prefix as configured in the application
-- =====================================================
TRUNCATE TABLE "AppUserTokens" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "AppUserLogins" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "AppUserClaims" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "AppUserRoles" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "AppRoleClaims" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "AppUsers" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "AppRoles" RESTART IDENTITY CASCADE;

-- =====================================================
-- 7. Truncate Tenant Table (must be last due to FKs)
-- =====================================================
TRUNCATE TABLE "Tenants" RESTART IDENTITY CASCADE;

-- Re-enable triggers
SET session_replication_role = 'origin';

COMMIT;

-- =====================================================
-- Verification: Show table counts (all should be 0)
-- =====================================================
SELECT 'Tenants' as "Table", COUNT(*) as "Count" FROM "Tenants"
UNION ALL SELECT 'Firms', COUNT(*) FROM "Firms"
UNION ALL SELECT 'Facilities', COUNT(*) FROM "Facilities"
UNION ALL SELECT 'Devices', COUNT(*) FROM "Devices"
UNION ALL SELECT 'DeviceTypes', COUNT(*) FROM "DeviceTypes"
UNION ALL SELECT 'DeviceLinks', COUNT(*) FROM "DeviceLinks"
UNION ALL SELECT 'AppUsers', COUNT(*) FROM "AppUsers"
UNION ALL SELECT 'AppRoles', COUNT(*) FROM "AppRoles"
UNION ALL SELECT 'MarketOperators', COUNT(*) FROM "MarketOperators"
UNION ALL SELECT 'FirmMarketOperators', COUNT(*) FROM "FirmMarketOperators"
UNION ALL SELECT 'DayAheadEnergyPriceHistories', COUNT(*) FROM "DayAheadEnergyPriceHistories"
UNION ALL SELECT 'Tariffs', COUNT(*) FROM "Tariffs"
UNION ALL SELECT 'FacilityTariffs', COUNT(*) FROM "FacilityTariffs"
UNION ALL SELECT 'DeviceSolarData', COUNT(*) FROM "DeviceSolarData"
UNION ALL SELECT 'DeviceBessData', COUNT(*) FROM "DeviceBessData"
UNION ALL SELECT 'DevicePcsData', COUNT(*) FROM "DevicePcsData"
UNION ALL SELECT 'DeviceSmartMeterData', COUNT(*) FROM "DeviceSmartMeterData"
ORDER BY "Table";

-- =====================================================
-- After running this script:
-- 1. Run the application: cd src/Web && dotnet run
--    (This triggers SeedAsync to recreate demo data:
--     Demo Tenant, Demo Energy Solutions Firm, Demo Solar Park Amsterdam,
--     Roles, Admin user, Devices, Tariffs)
-- 2. If needed, run additional seed scripts for market operators
-- =====================================================
