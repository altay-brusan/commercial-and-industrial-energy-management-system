namespace Backend.Domain.Constants;

public abstract class Permissions
{
    // Portfolio
    public const string PortfolioView = "Portfolio.View";

    // Dashboard
    public const string DashboardView = "Dashboard.View";

    // Monitoring
    public const string MonitoringView = "Monitoring.View";

    // Solar
    public const string SolarView = "Solar.View";
    public const string SolarControl = "Solar.Control";

    // Bess
    public const string BessView = "Bess.View";

    // GridAndLoads
    public const string GridAndLoadsView = "GridAndLoads.View";

    // Scada
    public const string ScadaView = "Scada.View";
    public const string ScadaControl = "Scada.Control";

    // Analytics
    public const string AnalyticsView = "Analytics.View";

    // Financials
    public const string FinancialsView = "Financials.View";

    // Reporting
    public const string ReportingView = "Reporting.View";
    public const string ReportingExport = "Reporting.Export";

    // Operations
    public const string OperationsView = "Operations.View";
    public const string OperationsManage = "Operations.Manage";

    // Settings - Firm/Facility/Device management
    public const string FirmView = "Firm.View";
    public const string FirmCreate = "Firm.Create";
    public const string FirmEdit = "Firm.Edit";
    public const string FirmDelete = "Firm.Delete";
    public const string FacilityView = "Facility.View";
    public const string FacilityCreate = "Facility.Create";
    public const string FacilityEdit = "Facility.Edit";
    public const string FacilityDelete = "Facility.Delete";
    public const string DeviceView = "Device.View";
    public const string DeviceCreate = "Device.Create";

    // UserManagement
    public const string UserManagementView = "UserManagement.View";
    public const string UserManagementCreate = "UserManagement.Create";
    public const string UserManagementEdit = "UserManagement.Edit";
    public const string UserManagementDelete = "UserManagement.Delete";

    // Audit
    public const string AuditView = "Audit.View";

    // FacilityMap
    public const string FacilityMapView = "FacilityMap.View";

    // Pcs
    public const string PcsView = "Pcs.View";

    // Alarm
    public const string AlarmView = "Alarm.View";
    public const string AlarmManage = "Alarm.Manage";

    // Maintenance
    public const string MaintenanceView = "Maintenance.View";
    public const string MaintenanceCreate = "Maintenance.Create";
    public const string MaintenanceManage = "Maintenance.Manage";
}
