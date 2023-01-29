using System;
using System.Net.Http;

namespace Ridge.DelegationHandlers;

/// <summary>
///     Http request message extensions.
/// </summary>
public static class HttpRequestMessageExtensions
{
    /// <summary>
    ///     Get <see cref="RequestDescription" /> from <see cref="HttpRequestMessage" />.
    ///     <see cref="RequestDescription" /> is saved in options property.
    /// </summary>
    /// <param name="httpRequestMessage"><see cref="HttpRequestMessage" /> which provides <see cref="RequestDescription" />.</param>
    /// <returns>
    ///     <see cref="RequestDescription" />
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when <see cref="HttpRequestMessage" /> does not contain
    ///     <see cref="RequestDescription" /> in Options property.
    /// </exception>
    public static RequestDescription GetRequestDescription(
        this HttpRequestMessage httpRequestMessage)
    {
        var requestDescriptionFound = httpRequestMessage.Options.TryGetValue(
            new HttpRequestOptionsKey<RequestDescription>(RequestDescription.OptionsKey),
            out var actionCallDescription);

        if (!requestDescriptionFound)
        {
            throw new InvalidOperationException($"{nameof(RequestDescription)} not found in {nameof(HttpRequestMessage)}. Did you created this request using Ridge?");
        }

        return actionCallDescription!;
    }
}
