-- Update supported tabs for DEMO-BG-001 and its child devices
-- Root (DEMO-BG-001): Summary, Health, Curves, FaultFlags, Faults, Settings
-- Sub-unit tabs are controlled by application code (GetSubUnitTabs):
--   Racks:   Summary, Alarms
--   Packs:   Summary, Alarms
--   Modules: Summary, Cells, Alarms
--   HVAC:    Settings, Alarms
-- This script updates:
--   1. Template supportedTabs for root device
--   2. HVAC device metadata (firmware, protocol, modbusAddress)

DO $$
DECLARE
    v_root_id UUID := '2932350b-4b0f-4b24-bb03-7a49d1996713';
    v_hvac_id UUID := '50473bd0-f43f-44e6-9ca3-60675d89c14a';
    v_template_id UUID;
BEGIN
    -- Get the template linked to DEMO-BG-001
    SELECT "DeviceSpecificationTemplateId" INTO v_template_id
    FROM "Devices"
    WHERE "Id" = v_root_id;

    IF v_template_id IS NULL THEN
        RAISE EXCEPTION 'DEMO-BG-001 (%) has no linked template. Register device first.', v_root_id;
    END IF;

    -- 1. Update supportedTabs in the template's SpecificationsTemplate
    UPDATE "DeviceSpecificationTemplates"
    SET "SpecificationsTemplate" = jsonb_set(
        "SpecificationsTemplate"::jsonb,
        '{supportedTabs}',
        '[
            { "key": "summary",    "label": "Summary",     "isEnabled": true, "order": 1 },
            { "key": "health",     "label": "Health",      "isEnabled": true, "order": 2 },
            { "key": "curves",     "label": "Curves",      "isEnabled": true, "order": 3 },
            { "key": "faultFlags", "label": "Fault Flags", "isEnabled": true, "order": 4 },
            { "key": "faults",     "label": "Faults",      "isEnabled": true, "order": 5 },
            { "key": "settings",   "label": "Settings",    "isEnabled": true, "order": 6 }
        ]'::jsonb
    ),
    "LastModified" = NOW()
    WHERE "Id" = v_template_id;

    RAISE NOTICE 'Updated supportedTabs for template %', v_template_id;

    -- 2. Update HVAC-1 device metadata with device info fields
    UPDATE "Devices"
    SET "Metadata" = jsonb_build_object(
        'firmware', 'v1.8.3',
        'protocol', 'Modbus RTU',
        'modbusAddress', 20
    ),
    "LastModified" = NOW()
    WHERE "Id" = v_hvac_id;

    RAISE NOTICE 'Updated HVAC-1 metadata (%)' , v_hvac_id;
END $$;
