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
TRUNCATE TABLE "DayAheadNetEnergyPriceHistories" RESTART IDENTITY CASCADE;

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
UNION ALL SELECT 'DayAheadNetEnergyPriceHistories', COUNT(*) FROM "DayAheadNetEnergyPriceHistories"
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
--    (This triggers SeedAsync to recreate demo data)
-- =====================================================
-- SEEDED DATA REFERENCE:
-- =====================================================
--
-- TENANT:
--   - ID: 1, Identifier: "demo", Name: "Demo Tenant"
--
-- FIRM:
--   - ID: 1, Name: "Demo Energy Solutions", Country: "Netherlands"
--
-- FACILITY:
--   - ID: 1, Name: "Demo Solar Park Amsterdam"
--   - FirmId: 1, Location: Amsterdam (4.8945, 52.3676)
--
-- ROLES:
--   - Administrator (super admin)
--   - Tenant.Admin (tenant administrator)
--   - User (regular user)
--
-- ADMIN USER:
--   - Email: administrator@localhost
--   - Password: ***REMOVED***
--   - Roles: Administrator
--
-- DEVICE TYPES:
--   - Solar, Generator, Wind Turbine, Battery, Inverter, PCS
--   - Smart Meter, Grid, DC Bus, Transformer, Load, EV Charger, Heat Pump
--
-- DEVICES:
--   - BESS_001 (Tesla Megapack)
--   - PV_001 (SunPower Maxeon 6)
--   - INVERTER_001 (SMA Sunny Tripower)
--   - SM_001 (Schneider PM5560)
--   - GRID_001 (Grid Connection)
--   - GEN_001 (Caterpillar C18)
--
-- MARKET OPERATORS:
--   - ENTSO-E: Code "ENTSOE", URL: https://web-api.tp.entsoe.eu/api
--     ApiKey: ***REMOVED***
--     AreaCodes: NL, DE, FR, BE, AT
--   - EPIAS: Code "EPIAS", URL: https://seffaflik.epias.com.tr
--     ApiKey: technology@oklidis.com.tr:***REMOVED*** (user:pass format)
--     AreaCodes: TR
--
-- FIRM MARKET OPERATORS:
--   - Demo Firm (ID:1) -> ENTSO-E with AreaCode "10YNL----------L" (Netherlands)
--     SamplingPeriodMinutes: 60
--
-- TARIFFS:
--   - Netherlands (Liander): DistributionFee=25, TransmissionFee=5, TaxRate=0.21, EUR
--   - Germany (E.ON): DistributionFee=30, TransmissionFee=6, TaxRate=0.19, EUR
--   - Türkiye (TEDAŞ): DistributionFee=150, TransmissionFee=30, TaxRate=0.20, TRY
--
-- FACILITY TARIFFS:
--   - Demo Facility (ID:1) -> Netherlands Tariff (Liander)
--
-- =====================================================
