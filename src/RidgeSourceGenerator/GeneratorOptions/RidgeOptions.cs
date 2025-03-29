namespace RidgeSourceGenerator.GeneratorOptions;

public class RidgeOptions : IEquatable<RidgeOptions>
{
    public RidgeOptions(
        bool useSingleClient)
    {
        UseSingleClient = useSingleClient;
    }

    public bool UseSingleClient { get; }

    public bool Equals(
        RidgeOptions? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return UseSingleClient == other.UseSingleClient;
    }

    public override bool Equals(
        object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((RidgeOptions) obj);
    }

    public override int GetHashCode()
    {
        return UseSingleClient.GetHashCode();
    }

    public static bool operator ==(
        RidgeOptions? left,
        RidgeOptions? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(
        RidgeOptions? left,
        RidgeOptions? right)
    {
        return !Equals(left, right);
    }
}
