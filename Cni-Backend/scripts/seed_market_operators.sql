-- =====================================================
-- Seed Market Operators (MO)
-- ENTSO-E and EPIAS
-- =====================================================
-- This script is idempotent - safe to run multiple times.
-- It will skip inserts if records already exist.
-- =====================================================

BEGIN;

-- Insert ENTSO-E only if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "MarketOperators" WHERE "Code" = 'ENTSOE') THEN
        INSERT INTO "MarketOperators" (
            "Name",
            "Code",
            "BaseUrl",
            "ApiKey",
            "SupportedResolutions",
            "AreaCodes",
            "IsActive",
            "Created",
            "CreatedBy",
            "LastModified",
            "LastModifiedBy"
        ) VALUES (
            'ENTSO-E',
            'ENTSOE',
            'https://web-api.tp.entsoe.eu/api',
            '***REMOVED***',
            '[15, 60]'::jsonb,
            '["10YNL----------L", "10YDE-VE-------2", "10YFR-RTE------C", "10YBE----------2", "10YAT-APG------L", "10YCH-SWISSGRIDZ", "10YES-REE------0", "10YPT-REN------W", "10YIT-GRTN-----B", "10YGR-HTSO-----Y", "10Y1001A1001A65H", "10YSE-1--------K", "10YNO-0--------C", "10YFI-1--------U", "10YPL-AREA-----S", "10YCZ-CEPS-----N", "10YHU-MAVIR----U", "10YRO-TEL------P", "10YCA-BULGARIA-R", "10YGB----------A", "10YIE-1001A00010", "10YTR-TEIAS----W"]'::jsonb,
            true,
            NOW() AT TIME ZONE 'UTC',
            'system',
            NOW() AT TIME ZONE 'UTC',
            'system'
        );
        RAISE NOTICE 'ENTSOE record inserted successfully';
    ELSE
        RAISE NOTICE 'ENTSOE record already exists, skipping...';
    END IF;
END $$;

-- Insert EPIAS only if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "MarketOperators" WHERE "Code" = 'EPIAS') THEN
        INSERT INTO "MarketOperators" (
            "Name",
            "Code",
            "BaseUrl",
            "ApiKey",
            "SupportedResolutions",
            "AreaCodes",
            "IsActive",
            "Created",
            "CreatedBy",
            "LastModified",
            "LastModifiedBy"
        ) VALUES (
            'EPIAS',
            'EPIAS',
            'https://seffaflik.epias.com.tr/electricity-service/v1',
            'technology@oklidis.com.tr:***REMOVED***',
            '[60]'::jsonb,
            '["TR"]'::jsonb,
            true,
            NOW() AT TIME ZONE 'UTC',
            'system',
            NOW() AT TIME ZONE 'UTC',
            'system'
        );
        RAISE NOTICE 'EPIAS record inserted successfully';
    ELSE
        RAISE NOTICE 'EPIAS record already exists, skipping...';
    END IF;
END $$;

COMMIT;

-- =====================================================
-- Verify the results
-- =====================================================
SELECT
    "Id",
    "Name",
    "Code",
    "BaseUrl",
    "SupportedResolutions",
    "AreaCodes",
    "IsActive",
    "Created"
FROM "MarketOperators"
WHERE "Code" IN ('ENTSOE', 'EPIAS')
ORDER BY "Id";
