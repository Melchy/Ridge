using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ridge;

/// <summary>
///     Used to access parameters provided by user in CustomParameters argument
/// </summary>
public class CustomParameterProvider : IEnumerable<object?>
{
    private IEnumerable<object?> CustomParameters { get; }

    internal CustomParameterProvider(
        IEnumerable<object?> customParameters)
    {
        CustomParameters = customParameters;
    }

    /// <summary>
    ///     Access custom parameters by type
    /// </summary>
    /// <typeparam name="TParameter">Type of parameter to access</typeparam>
    /// <returns>Collection of parameters found</returns>
    public IEnumerable<TParameter?> GetCustomParametersByType<TParameter>()
    {
        var parameterToReturn = CustomParameters.Where(x => x is TParameter);
        return parameterToReturn.Select(x => (TParameter?)x);
    }

    /// <inheritdoc />
    public IEnumerator<object?> GetEnumerator()
    {
        return CustomParameters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
