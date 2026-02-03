-- =====================================================
-- Seed Transmission System Operators (TSO) Providers
-- ENTSOE and EPIAS
-- =====================================================
-- This script is idempotent - safe to run multiple times.
-- It will skip inserts if records already exist.
-- =====================================================

BEGIN;

-- Insert ENTSO-E only if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "TransmissionSystemOperators" WHERE "Code" = 'ENTSOE' AND "TenantId" = 1) THEN
        INSERT INTO "TransmissionSystemOperators" (
            "TenantId",
            "Name",
            "Code",
            "BaseUrl",
            "ApiKey",
            "TaxRate",
            "DistributionFee",
            "TimeResolutionMinutes",
            "Currency",
            "IsActive",
            "AreaCode",
            "Created",
            "CreatedBy",
            "LastModified",
            "LastModifiedBy"
        ) VALUES (
            1,
            'ENTSO-E',
            'ENTSOE',
            'https://web-api.tp.entsoe.eu/api',
            '***REMOVED***',
            0.20,
            5.00,
            60,
            'EUR',
            true,
            '10YTR-TEIAS----W',
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
    IF NOT EXISTS (SELECT 1 FROM "TransmissionSystemOperators" WHERE "Code" = 'EPIAS' AND "TenantId" = 1) THEN
        INSERT INTO "TransmissionSystemOperators" (
            "TenantId",
            "Name",
            "Code",
            "BaseUrl",
            "ApiKey",
            "TaxRate",
            "DistributionFee",
            "TimeResolutionMinutes",
            "Currency",
            "IsActive",
            "AreaCode",
            "Created",
            "CreatedBy",
            "LastModified",
            "LastModifiedBy"
        ) VALUES (
            1,
            'EPIAS',
            'EPIAS',
            'https://seffaflik.epias.com.tr/electricity-service/v1',
            'technology@oklidis.com.tr:***REMOVED***',
            0.20,
            15.00,
            60,
            'TRY',
            true,
            NULL,
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
    "TenantId",
    "Name",
    "Code",
    "BaseUrl",
    "Currency",
    "TaxRate",
    "DistributionFee",
    "IsActive",
    "Created"
FROM "TransmissionSystemOperators"
WHERE "Code" IN ('ENTSOE', 'EPIAS')
ORDER BY "Id";
