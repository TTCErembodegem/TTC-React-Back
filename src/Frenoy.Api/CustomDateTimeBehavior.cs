using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Frenoy.Api;

/// <summary>
/// Frenoy sends DateTimes as 2024-09-15 09:43:31.
/// This results in: FormatException: The string '2024-09-15 09:43:31' is not a valid AllXsd value.
///
/// Expected is ISO8601 which has a T between Date and Time.
///
/// This changes the deserialization behavior.
/// </summary>
public class CustomDateTimeBehavior : IEndpointBehavior
{
    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
    {
        clientRuntime.ClientMessageInspectors.Add(new CustomDateTimeInspector());
    }

    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }
    public void Validate(ServiceEndpoint endpoint) { }
}

public class CustomDateTimeInspector : IClientMessageInspector
{
    public void AfterReceiveReply(ref Message reply, object correlationState)
    {
        // Buffer the message to modify it
        var buffer = reply.CreateBufferedCopy(int.MaxValue);
        var document = buffer.CreateMessage().ToString();

        // Replace non-ISO DateTime with ISO format
        document = ReplaceNonIsoDateTimes(document);

        // Create a new reply message
        var newMessage = Message.CreateMessage(buffer.CreateMessage().Version, null, document);
        newMessage.Properties.CopyProperties(reply.Properties);

        reply = newMessage;
    }

    private static string ReplaceNonIsoDateTimes(string xml)
    {
        // Example regex: Adjust to match your specific format
        var regex = new System.Text.RegularExpressions.Regex(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}");
        return regex.Replace(xml, match =>
        {
            // Convert to ISO 8601
            var dateTime = DateTime.ParseExact(match.Value, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
        });
    }

    public object BeforeSendRequest(ref Message request, IClientChannel channel)
    {
        return null;
    }
}