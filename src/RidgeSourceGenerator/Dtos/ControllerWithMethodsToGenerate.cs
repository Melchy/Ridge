namespace RidgeSourceGenerator.Dtos;

public class ControllerWithMethodsToGenerate : IEquatable<ControllerWithMethodsToGenerate>
{
    public readonly ControllerToGenerate ControllerToGenerate;
    public readonly MethodToGenerate?[] MethodsToGenerate;

    public ControllerWithMethodsToGenerate(
        ControllerToGenerate controllerToGenerate,
        MethodToGenerate?[] methodsToGenerate)
    {
        ControllerToGenerate = controllerToGenerate;
        MethodsToGenerate = methodsToGenerate;
    }

    public bool Equals(
        ControllerWithMethodsToGenerate? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (!ControllerToGenerate.Equals(other.ControllerToGenerate))
        {
            return false;
        }
        
        if (MethodsToGenerate.Length != other.MethodsToGenerate.Length)
        {
            return false;
        }
        
        for (var i = 0; i < MethodsToGenerate.Length; i++)
        {
            if (other.MethodsToGenerate[i] == null && MethodsToGenerate[i] == null)
            {
                continue;
            }
            
            if (other.MethodsToGenerate[i]?.Equals(MethodsToGenerate[i]) == false)
            {
                return false;
            }
        }

        return true;
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

        return Equals((ControllerWithMethodsToGenerate) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (ControllerToGenerate.GetHashCode() * 397) ^ MethodsToGenerate.GetHashCode();
        }
    }

    public static bool operator ==(
        ControllerWithMethodsToGenerate? left,
        ControllerWithMethodsToGenerate? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(
        ControllerWithMethodsToGenerate? left,
        ControllerWithMethodsToGenerate? right)
    {
        return !Equals(left, right);
    }
}
