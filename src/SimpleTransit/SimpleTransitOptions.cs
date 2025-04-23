namespace SimpleTransit;

internal class SimpleTransitOptions
{
    public PublishStrategy NotificationPublishStrategy { get; set; }

    public PublishStrategy ConsumerPublishStrategy { get; set; }
}
