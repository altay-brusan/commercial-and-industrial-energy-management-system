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
TRUNCATE TABLE "SolarDatas" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "BESSDatas" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "PCSDatas" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "SmartMeterDatas" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "GrpcFacilityDatas" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "EnergyPriceHistories" RESTART IDENTITY CASCADE;

-- =====================================================
-- 2. Truncate Junction/Association Tables
-- =====================================================
TRUNCATE TABLE "UserFirmFacilities" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "TenantMarketOperators" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "CountryTariffs" RESTART IDENTITY CASCADE;

-- =====================================================
-- 3. Truncate Topology Tables
-- =====================================================
TRUNCATE TABLE "TopologyEdges" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "TopologyNodes" RESTART IDENTITY CASCADE;

-- =====================================================
-- 4. Truncate Todo Tables (sample/demo data)
-- =====================================================
TRUNCATE TABLE "TodoItems" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "TodoLists" RESTART IDENTITY CASCADE;

-- =====================================================
-- 5. Truncate Device Tables
-- =====================================================
TRUNCATE TABLE "Devices" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "DeviceTypes" RESTART IDENTITY CASCADE;

-- =====================================================
-- 6. Truncate Core Entity Tables
-- =====================================================
TRUNCATE TABLE "Facilities" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "Firms" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "Plans" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "MarketOperators" RESTART IDENTITY CASCADE;

-- =====================================================
-- 7. Truncate Identity Tables (Users, Roles, Claims)
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
-- 8. Truncate Tenant Table (must be last due to FKs)
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
UNION ALL SELECT 'AppUsers', COUNT(*) FROM "AppUsers"
UNION ALL SELECT 'AppRoles', COUNT(*) FROM "AppRoles"
UNION ALL SELECT 'MarketOperators', COUNT(*) FROM "MarketOperators"
UNION ALL SELECT 'TenantMarketOperators', COUNT(*) FROM "TenantMarketOperators"
UNION ALL SELECT 'EnergyPriceHistories', COUNT(*) FROM "EnergyPriceHistories"
UNION ALL SELECT 'CountryTariffs', COUNT(*) FROM "CountryTariffs"
ORDER BY "Table";

-- =====================================================
-- After running this script:
-- 1. Run the application: cd src/Web && dotnet run
--    (This triggers SeedAsync to recreate default data:
--     Tenant, Firm, Facility, Roles, Admin user, Devices, Topology)
-- 2. Run: scripts/seed_market_operators.sql
--    (Adds ENTSO-E and EPIAS Market Operators)
-- =====================================================
