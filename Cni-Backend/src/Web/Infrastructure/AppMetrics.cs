using Prometheus;

namespace Backend.Web.Infrastructure;

/// <summary>
/// Central registry of all application-level Prometheus metrics.
/// All metric names use the "cni_" prefix for Grafana dashboard consistency.
/// </summary>
public static class AppMetrics
{
    // HTTP request duration (custom middleware — matches DevOps-expected labels)
    public static readonly Histogram HttpRequestDuration = Metrics.CreateHistogram(
        "cni_http_request_duration_seconds",
        "Duration of HTTP requests in seconds",
        new HistogramConfiguration
        {
            LabelNames = ["method", "endpoint", "status_code"],
            Buckets = Histogram.ExponentialBuckets(0.005, 2, 12) // 5ms → ~10s
        });

    // EF Core DB query duration (auto-recorded by MetricsDbCommandInterceptor)
    public static readonly Histogram DbQueryDuration = Metrics.CreateHistogram(
        "cni_db_query_duration_seconds",
        "Duration of database queries in seconds",
        new HistogramConfiguration
        {
            LabelNames = ["operation", "table"],
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 14) // 1ms → ~8s
        });

    // MQTT/webhook message processing duration
    public static readonly Histogram MqttMessageProcessing = Metrics.CreateHistogram(
        "cni_mqtt_message_processing_seconds",
        "Duration of MQTT message processing in seconds",
        new HistogramConfiguration
        {
            LabelNames = ["kind"],
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 12)
        });

    // MQTT/webhook message end-to-end latency (device time → server receive time)
    public static readonly Histogram MqttMessageLatency = Metrics.CreateHistogram(
        "cni_mqtt_message_latency_seconds",
        "End-to-end latency from device timestamp to server processing",
        new HistogramConfiguration
        {
            LabelNames = ["kind"],
            Buckets = Histogram.ExponentialBuckets(0.1, 2, 14) // 100ms → ~819s
        });

    // MQTT/webhook message counter
    public static readonly Counter MqttMessagesTotal = Metrics.CreateCounter(
        "cni_mqtt_messages_total",
        "Total number of MQTT messages processed",
        new CounterConfiguration
        {
            LabelNames = ["kind", "status"]
        });

    // TODO: Wire when ALM service integration is implemented.
    // These metrics will track backend → ALM request duration and ALM → backend webhook callback duration.
    public static readonly Histogram AlmRequestDuration = Metrics.CreateHistogram(
        "cni_alm_request_duration_seconds",
        "Duration of requests to ALM service",
        new HistogramConfiguration
        {
            LabelNames = ["endpoint", "status"],
            Buckets = Histogram.ExponentialBuckets(0.01, 2, 12)
        });

    // TODO: Wire when ALM service integration is implemented.
    public static readonly Histogram AlmCallbackDuration = Metrics.CreateHistogram(
        "cni_alm_callback_duration_seconds",
        "Duration of ALM webhook callback processing",
        new HistogramConfiguration
        {
            LabelNames = ["job_type"],
            Buckets = Histogram.ExponentialBuckets(0.01, 2, 12)
        });
}
