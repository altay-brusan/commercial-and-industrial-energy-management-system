-- Seed DeviceNotificationTypes for Demo BESS model (DEMO-BG)
-- Run after DeviceTypes are seeded. Requires EnergyStorage DeviceType to exist.
-- All rows use Manufacturer='Demo', Model='DEMO-BG'

DO $$
DECLARE
    v_device_type_id UUID;
    v_now TIMESTAMPTZ := NOW();
BEGIN
    SELECT "Id" INTO v_device_type_id FROM "DeviceTypes" WHERE "Name" = 'EnergyStorage' LIMIT 1;

    IF v_device_type_id IS NULL THEN
        RAISE EXCEPTION 'EnergyStorage DeviceType not found. Seed DeviceTypes first.';
    END IF;

    -- Skip if already seeded
    IF EXISTS (SELECT 1 FROM "DeviceAlarmTypes" WHERE "Model" = 'DEMO-BG' LIMIT 1) THEN
        RAISE NOTICE 'DeviceNotificationTypes for DEMO-BG already seeded, skipping.';
        RETURN;
    END IF;

    INSERT INTO "DeviceAlarmTypes"
        ("Id", "DeviceTypeId", "Model", "Manufacturer", "Code", "FaultKey", "Kind", "Name", "Description", "Category", "Severity", "Source", "IsEnabled", "Created", "LastModified")
    VALUES
    -- ============================================================
    -- ContainerBatteryFaultState — Battery Management System faults
    -- ============================================================
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_CELL_OV_WARN', 'faultCellOverVoltageWarning', 'ContainerBatteryFaultState', 'Cell Over Voltage Warning', 'A cell voltage exceeded the warning threshold', 'battery', 'Warning', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_CELL_UV_WARN', 'faultCellUnderVoltageWarning', 'ContainerBatteryFaultState', 'Cell Under Voltage Warning', 'A cell voltage dropped below the warning threshold', 'battery', 'Warning', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_CELL_OV_FAULT', 'faultCellOverVoltageFault', 'ContainerBatteryFaultState', 'Cell Over Voltage Fault', 'A cell voltage exceeded the fault threshold', 'battery', 'Critical', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_CELL_UV_FAULT', 'faultCellUnderVoltageFault', 'ContainerBatteryFaultState', 'Cell Under Voltage Fault', 'A cell voltage dropped below the fault threshold', 'battery', 'Critical', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_PACK_OC', 'faultPackOC', 'ContainerBatteryFaultState', 'Pack Over Current', 'Pack current exceeded the over-current threshold', 'battery', 'Critical', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_PACK_OV', 'faultPackOV', 'ContainerBatteryFaultState', 'Pack Over Voltage', 'Pack voltage exceeded the over-voltage threshold', 'battery', 'Major', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_PACK_UV', 'faultPackUV', 'ContainerBatteryFaultState', 'Pack Under Voltage', 'Pack voltage dropped below the under-voltage threshold', 'battery', 'Major', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_OT_WARN', 'faultOverTempWarning', 'ContainerBatteryFaultState', 'Over Temperature Warning', 'Cell temperature exceeded the warning threshold', 'battery', 'Warning', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_OT_FAULT', 'faultOverTempFault', 'ContainerBatteryFaultState', 'Over Temperature Fault', 'Cell temperature exceeded the fault threshold', 'battery', 'Critical', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_UT_WARN', 'faultUnderTempWarning', 'ContainerBatteryFaultState', 'Under Temperature Warning', 'Cell temperature dropped below the warning threshold', 'battery', 'Warning', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_UT_FAULT', 'faultUnderTempFault', 'ContainerBatteryFaultState', 'Under Temperature Fault', 'Cell temperature dropped below the fault threshold', 'battery', 'Critical', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_INSUL_WARN', 'faultInsulationWarning', 'ContainerBatteryFaultState', 'Insulation Warning', 'Insulation resistance dropped below warning level', 'battery', 'Warning', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_INSUL_FAULT', 'faultInsulationFault', 'ContainerBatteryFaultState', 'Insulation Fault', 'Insulation resistance dropped below fault level', 'battery', 'Critical', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_VDIFF_WARN', 'faultVoltageDiffWarning', 'ContainerBatteryFaultState', 'Voltage Difference Warning', 'Cell voltage imbalance exceeded warning threshold', 'battery', 'Warning', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_VDIFF_FAULT', 'faultVoltageDiffFault', 'ContainerBatteryFaultState', 'Voltage Difference Fault', 'Cell voltage imbalance exceeded fault threshold', 'battery', 'Major', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_TDIFF_WARN', 'faultTempDiffWarning', 'ContainerBatteryFaultState', 'Temperature Difference Warning', 'Cell temperature spread exceeded warning threshold', 'battery', 'Warning', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_TDIFF_FAULT', 'faultTempDiffFault', 'ContainerBatteryFaultState', 'Temperature Difference Fault', 'Cell temperature spread exceeded fault threshold', 'battery', 'Major', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_SOC_LOW_WARN', 'faultSocLowWarning', 'ContainerBatteryFaultState', 'SOC Low Warning', 'State of charge dropped below warning level', 'battery', 'Warning', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_SOC_LOW_FAULT', 'faultSocLowFault', 'ContainerBatteryFaultState', 'SOC Low Fault', 'State of charge dropped below fault level', 'battery', 'Major', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_SOC_HIGH_WARN', 'faultSocHighWarning', 'ContainerBatteryFaultState', 'SOC High Warning', 'State of charge exceeded warning level', 'battery', 'Warning', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_BMS_COMM', 'faultBmsCommFailure', 'ContainerBatteryFaultState', 'BMS Communication Failure', 'Communication with battery management system lost', 'battery', 'Critical', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_CONTACTOR', 'faultContactorFailure', 'ContainerBatteryFaultState', 'Contactor Failure', 'Main DC contactor failed to open or close', 'battery', 'Critical', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_FUSE', 'faultFuseBlown', 'ContainerBatteryFaultState', 'Fuse Blown', 'A fuse in the battery circuit has blown', 'battery', 'Critical', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_CHG_OC', 'faultChargeOverCurrent', 'ContainerBatteryFaultState', 'Charge Over Current', 'Charge current exceeded maximum allowed', 'battery', 'Major', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_DISCHG_OC', 'faultDischargeOverCurrent', 'ContainerBatteryFaultState', 'Discharge Over Current', 'Discharge current exceeded maximum allowed', 'battery', 'Major', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_EMRG_STOP', 'faultEmergencyStop', 'ContainerBatteryFaultState', 'Emergency Stop', 'Emergency stop activated on battery system', 'battery', 'Critical', 'BMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'BATT_PRECHARGE', 'faultPrechargeFailure', 'ContainerBatteryFaultState', 'Precharge Failure', 'DC bus precharge sequence failed', 'battery', 'Major', 'BMS', true, v_now, v_now),

    -- ============================================================
    -- ChillerAlarmState — Thermal Management System alarms
    -- ============================================================
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'TMS_COOLANT_SHORT', 'systemCoolantShortage', 'ChillerAlarmState', 'System Coolant Shortage', 'Coolant level dropped below minimum', 'chiller', 'Major', 'TMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'TMS_VFD_OC', 'compressorVfdOcAlarm', 'ChillerAlarmState', 'Compressor VFD Over Current', 'Variable frequency drive over-current alarm', 'chiller', 'Major', 'TMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'TMS_PWR_OV', 'powerSupplyOV', 'ChillerAlarmState', 'Power Supply Over Voltage', 'Chiller power supply voltage exceeded limit', 'chiller', 'Warning', 'TMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'TMS_PWR_UV', 'powerSupplyUV', 'ChillerAlarmState', 'Power Supply Under Voltage', 'Chiller power supply voltage dropped below limit', 'chiller', 'Warning', 'TMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'TMS_COMP_OT', 'compressorOverTemp', 'ChillerAlarmState', 'Compressor Over Temperature', 'Compressor discharge temperature exceeded limit', 'chiller', 'Major', 'TMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'TMS_COMP_HP', 'compressorHighPressure', 'ChillerAlarmState', 'Compressor High Pressure', 'Refrigerant high-side pressure exceeded limit', 'chiller', 'Major', 'TMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'TMS_COMP_LP', 'compressorLowPressure', 'ChillerAlarmState', 'Compressor Low Pressure', 'Refrigerant low-side pressure dropped below limit', 'chiller', 'Warning', 'TMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'TMS_PUMP_FAULT', 'coolantPumpFault', 'ChillerAlarmState', 'Coolant Pump Fault', 'Coolant circulation pump failed', 'chiller', 'Critical', 'TMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'TMS_FAN_FAULT', 'condenserFanFault', 'ChillerAlarmState', 'Condenser Fan Fault', 'Condenser fan motor failed', 'chiller', 'Major', 'TMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'TMS_FLOW_LOW', 'coolantFlowLow', 'ChillerAlarmState', 'Coolant Flow Low', 'Coolant flow rate dropped below minimum', 'chiller', 'Warning', 'TMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'TMS_OUTLET_OT', 'outletOverTemp', 'ChillerAlarmState', 'Outlet Over Temperature', 'Coolant outlet temperature exceeded limit', 'chiller', 'Warning', 'TMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'TMS_OUTLET_UT', 'outletUnderTemp', 'ChillerAlarmState', 'Outlet Under Temperature', 'Coolant outlet temperature dropped below limit', 'chiller', 'Warning', 'TMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'TMS_COMM', 'chillerCommFailure', 'ChillerAlarmState', 'Chiller Communication Failure', 'Communication with chiller controller lost', 'chiller', 'Major', 'TMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'TMS_VFD_COMM', 'compressorVfdCommFault', 'ChillerAlarmState', 'VFD Communication Fault', 'Communication with compressor VFD lost', 'chiller', 'Major', 'TMS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'TMS_HEATER', 'heaterFault', 'ChillerAlarmState', 'Heater Fault', 'Anti-freeze heater failed', 'chiller', 'Warning', 'TMS', true, v_now, v_now),

    -- ============================================================
    -- PcsFaultGrp1State — PCS Hardware & Grid Faults
    -- ============================================================
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_HW_EPO', 'pcs_hwEpoFault', 'PcsFaultGrp1State', 'Hardware EPO Fault', 'Emergency power off triggered by hardware', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_GRID_A_OV', 'pcs_gridAOvFault', 'PcsFaultGrp1State', 'Grid Phase A Over Voltage', 'Grid phase A voltage exceeded limit', 'pcs', 'Major', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_GRID_B_OV', 'pcs_gridBOvFault', 'PcsFaultGrp1State', 'Grid Phase B Over Voltage', 'Grid phase B voltage exceeded limit', 'pcs', 'Major', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_GRID_C_OV', 'pcs_gridCOvFault', 'PcsFaultGrp1State', 'Grid Phase C Over Voltage', 'Grid phase C voltage exceeded limit', 'pcs', 'Major', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_GRID_A_UV', 'pcs_gridAUvFault', 'PcsFaultGrp1State', 'Grid Phase A Under Voltage', 'Grid phase A voltage dropped below limit', 'pcs', 'Major', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_GRID_B_UV', 'pcs_gridBUvFault', 'PcsFaultGrp1State', 'Grid Phase B Under Voltage', 'Grid phase B voltage dropped below limit', 'pcs', 'Major', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_GRID_C_UV', 'pcs_gridCUvFault', 'PcsFaultGrp1State', 'Grid Phase C Under Voltage', 'Grid phase C voltage dropped below limit', 'pcs', 'Major', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_GRID_FREQ_HI', 'pcs_gridFreqHighFault', 'PcsFaultGrp1State', 'Grid Frequency High', 'Grid frequency exceeded upper limit', 'pcs', 'Major', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_GRID_FREQ_LO', 'pcs_gridFreqLowFault', 'PcsFaultGrp1State', 'Grid Frequency Low', 'Grid frequency dropped below lower limit', 'pcs', 'Major', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_DC_OV', 'pcs_dcBusOvFault', 'PcsFaultGrp1State', 'DC Bus Over Voltage', 'DC bus voltage exceeded limit', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_DC_UV', 'pcs_dcBusUvFault', 'PcsFaultGrp1State', 'DC Bus Under Voltage', 'DC bus voltage dropped below limit', 'pcs', 'Major', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_IGBT_OT', 'pcs_igbtOverTempFault', 'PcsFaultGrp1State', 'IGBT Over Temperature', 'IGBT module temperature exceeded limit', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_AC_OC_A', 'pcs_acOverCurrentAFault', 'PcsFaultGrp1State', 'AC Over Current Phase A', 'AC output phase A current exceeded limit', 'pcs', 'Major', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_AC_OC_B', 'pcs_acOverCurrentBFault', 'PcsFaultGrp1State', 'AC Over Current Phase B', 'AC output phase B current exceeded limit', 'pcs', 'Major', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_AC_OC_C', 'pcs_acOverCurrentCFault', 'PcsFaultGrp1State', 'AC Over Current Phase C', 'AC output phase C current exceeded limit', 'pcs', 'Major', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_DC_OC', 'pcs_dcOverCurrentFault', 'PcsFaultGrp1State', 'DC Over Current', 'DC side current exceeded limit', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_AMBIENT_OT', 'pcs_ambientOverTempFault', 'PcsFaultGrp1State', 'Ambient Over Temperature', 'PCS enclosure ambient temperature exceeded limit', 'pcs', 'Warning', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_INDUCT_OT', 'pcs_inductanceOverTempFault', 'PcsFaultGrp1State', 'Inductance Over Temperature', 'Filter inductance temperature exceeded limit', 'pcs', 'Major', 'PCS', true, v_now, v_now),

    -- ============================================================
    -- PcsFaultGrp2State — PCS Protection & Relay Faults
    -- ============================================================
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_PREREL_CLOSE', 'pcs_preRelCloseFail', 'PcsFaultGrp2State', 'Pre-charge Relay Close Failure', 'Pre-charge relay failed to close', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_PREREL_OPEN', 'pcs_preRelOpenFail', 'PcsFaultGrp2State', 'Pre-charge Relay Open Failure', 'Pre-charge relay failed to open', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_MAINREL_CLOSE', 'pcs_mainRelCloseFail', 'PcsFaultGrp2State', 'Main Relay Close Failure', 'Main DC relay failed to close', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_MAINREL_OPEN', 'pcs_mainRelOpenFail', 'PcsFaultGrp2State', 'Main Relay Open Failure', 'Main DC relay failed to open', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_ACBKR_CLOSE', 'pcs_acBreakerCloseFail', 'PcsFaultGrp2State', 'AC Breaker Close Failure', 'AC output breaker failed to close', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_ACBKR_OPEN', 'pcs_acBreakerOpenFail', 'PcsFaultGrp2State', 'AC Breaker Open Failure', 'AC output breaker failed to open', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_GROUND_FAULT', 'pcs_groundFault', 'PcsFaultGrp2State', 'Ground Fault', 'Ground fault detected on PCS output', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_FAN_FAULT', 'pcs_coolingFanFault', 'PcsFaultGrp2State', 'Cooling Fan Fault', 'PCS cooling fan failed', 'pcs', 'Major', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_FUSE_FAULT', 'pcs_dcFuseFault', 'PcsFaultGrp2State', 'DC Fuse Fault', 'DC side fuse blown', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_SPD_FAULT', 'pcs_surgeSpdFault', 'PcsFaultGrp2State', 'Surge Protector Fault', 'Surge protection device tripped', 'pcs', 'Major', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_PHASE_SEQ', 'pcs_phaseSeqFault', 'PcsFaultGrp2State', 'Phase Sequence Fault', 'Incorrect AC phase sequence detected', 'pcs', 'Major', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_SYNC_FAIL', 'pcs_gridSyncFail', 'PcsFaultGrp2State', 'Grid Sync Failure', 'Failed to synchronize with grid', 'pcs', 'Major', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_SW_EPO', 'pcs_swEpoFault', 'PcsFaultGrp2State', 'Software EPO Fault', 'Emergency power off triggered by software', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_COMM_BMS', 'pcs_bmsCommFault', 'PcsFaultGrp2State', 'BMS Communication Fault', 'PCS lost communication with BMS', 'pcs', 'Major', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_COMM_EMS', 'pcs_emsCommFault', 'PcsFaultGrp2State', 'EMS Communication Fault', 'PCS lost communication with EMS', 'pcs', 'Warning', 'PCS', true, v_now, v_now),

    -- ============================================================
    -- PcsTimeoutsState — PCS Protection Timeouts
    -- ============================================================
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_DC_OC_TOUT', 'pcs_dcOcProtTout', 'PcsTimeoutsState', 'DC Over Current Protection Timeout', 'DC over-current protection response timed out', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_AC_OC_TOUT', 'pcs_acOcProtTout', 'PcsTimeoutsState', 'AC Over Current Protection Timeout', 'AC over-current protection response timed out', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_DC_OV_TOUT', 'pcs_dcOvProtTout', 'PcsTimeoutsState', 'DC Over Voltage Protection Timeout', 'DC over-voltage protection response timed out', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_GRID_OV_TOUT', 'pcs_gridOvProtTout', 'PcsTimeoutsState', 'Grid Over Voltage Protection Timeout', 'Grid over-voltage protection response timed out', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_GRID_UV_TOUT', 'pcs_gridUvProtTout', 'PcsTimeoutsState', 'Grid Under Voltage Protection Timeout', 'Grid under-voltage protection response timed out', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_GRID_FREQ_TOUT', 'pcs_gridFreqProtTout', 'PcsTimeoutsState', 'Grid Frequency Protection Timeout', 'Grid frequency protection response timed out', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_OT_PROT_TOUT', 'pcs_overTempProtTout', 'PcsTimeoutsState', 'Over Temperature Protection Timeout', 'Over-temperature protection response timed out', 'pcs', 'Critical', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_PRECHARGE_TOUT', 'pcs_prechargeTout', 'PcsTimeoutsState', 'Precharge Timeout', 'DC bus precharge timed out', 'pcs', 'Major', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_STARTUP_TOUT', 'pcs_startupTout', 'PcsTimeoutsState', 'Startup Timeout', 'PCS startup sequence timed out', 'pcs', 'Major', 'PCS', true, v_now, v_now),
    (gen_random_uuid(), v_device_type_id, 'DEMO-BG', 'Demo', 'PCS_SHUTDOWN_TOUT', 'pcs_shutdownTout', 'PcsTimeoutsState', 'Shutdown Timeout', 'PCS shutdown sequence timed out', 'pcs', 'Warning', 'PCS', true, v_now, v_now);

    RAISE NOTICE 'Seeded % DeviceNotificationTypes for DEMO-BG', (SELECT COUNT(*) FROM "DeviceAlarmTypes" WHERE "Model" = 'DEMO-BG');
END $$;
