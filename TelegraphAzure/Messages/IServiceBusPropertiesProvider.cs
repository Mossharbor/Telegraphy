using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    interface IServiceBusPropertiesProvider
    {
        //
        // Summary:
        //     Gets or sets the date and time in UTC at which the message will be enqueued.
        //     This property returns the time in UTC; when setting the property, the supplied
        //     DateTime value must also be in UTC.
        //
        // Remarks:
        //     Message enqueuing time does not mean that the message will be sent at the same
        //     time. It will get enqueued, but the actual sending time depends on the queue's
        //     workload and its state.
        DateTime? ScheduledEnqueueTimeUtc { get; }
        
        //
        // Summary:
        //     Gets or sets the type of the content.
        string ContentType { get; }

        //
        // Summary:
        //     Gets or sets the application specific label.
        string Label { get; }

        //
        // Summary:
        //     Gets the a correlation identifier.
        //
        // Remarks:
        //     Its a custom property that can be used to either transfer a correlation Id to
        //     the destination or be used in Microsoft.Azure.ServiceBus.CorrelationFilter
        string CorrelationId { get; }

        //
        // Summary:
        //     Gets the message’s time to live value. This is the duration after which
        //     the message expires, starting from when the message is sent to the Service Bus.
        //     Messages older than their TimeToLive value will expire and no longer be retained
        //     in the message store. Expired messages cannot be received. TimeToLive is the
        //     maximum lifetime that a message can be received, but its value cannot exceed
        //     the entity specified value on the destination queue or subscription. If a lower
        //     TimeToLive value is specified, it will be applied to the individual message.
        //     However, a larger value specified on the message will be overridden by the entity’s
        //     DefaultMessageTimeToLive value.
        //
        // Remarks:
        //     If the TTL set on a message by the sender exceeds the destination's TTL, then
        //     the message's TTL will be overwritten by the later one.
        TimeSpan? TimeToLive { get; }

        //
        // Summary:
        //     Gets the session identifier to reply to.
        //
        // Remarks:
        //     Max size of ReplyToSessionId is 128.
        string ReplyToSessionId { get; }

        //
        // Summary:
        //     Gets or sets a sessionId. A message with sessionId set can only be received using
        //     a Microsoft.Azure.ServiceBus.IMessageSession object.
        //
        // Remarks:
        //     Max size of sessionId is 128 chars.
        string SessionId { get; }

        //
        // Summary:
        //     Gets the MessageId to identify the message.
        //
        // Remarks:
        //     A value set by the user to identify the message. In case message deduplication
        //     is enabled on the entity, this value will be used for deduplication. Max MessageId
        //     size is 128 chars.
        string MessageId { get; }

        //
        // Summary:
        //     Gets the user property bag, which can be used for custom message properties.
        //
        // Remarks:
        //     Only following value types are supported: byte, sbyte, char, short, ushort, int,
        //     uint, long, ulong, float, double, decimal, bool, Guid, string, Uri, DateTime,
        //     DateTimeOffset, TimeSpan, Stream, byte[], and IList / IDictionary of supported
        //     types
        IDictionary<string, object> UserProperties { get; }
    }
}
