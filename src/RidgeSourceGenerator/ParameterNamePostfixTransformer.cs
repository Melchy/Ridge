namespace RidgeSourceGenerator;

public class ParameterNamePostfixTransformer
{
    private readonly Dictionary<string, int> _namesBuffer = new();

    public ParameterNamePostfixTransformer(
        IEnumerable<string> methodParameters)
    {
        foreach (var methodParameter in methodParameters)
        {
            _namesBuffer[methodParameter] = 0;
        }
    }

    public string TransformName(
        string name)
    {
        _namesBuffer.TryGetValue(name, out var numberOfUses);
        _namesBuffer[name] = (numberOfUses + 1);
        if (numberOfUses == 0)
        {
            return name;
        }

        return $"{name}{numberOfUses}";
    }
}
