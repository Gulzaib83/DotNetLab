using System.Reflection;

public enum ResultCode
{
    [StringValue("Success")]
    Success = 1,
    [StringValue("Failure")]
    Failure = 2,
}

[AttributeUsage(AttributeTargets.Field)]
public class StringValueAttribute : Attribute
{
    public string Value { get; private set; }

    public StringValueAttribute(string value)
    {
        Value = value;
    }
}

public static class EnumExtensions
{
    public static string GetStringValue(this Enum value)
    {
        Type type = value.GetType();
        string name = Enum.GetName(type, value);

        if (name == null)
        {
            return null;
        }

        FieldInfo field = type.GetField(name);
        StringValueAttribute attribute = field.GetCustomAttribute<StringValueAttribute>();

        if (attribute == null)
        {
            return name;
        }

        return attribute.Value;
    }
}