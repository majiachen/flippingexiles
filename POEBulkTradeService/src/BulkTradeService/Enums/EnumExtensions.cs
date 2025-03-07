using System;
using System.Reflection;
using System.Runtime.Serialization;

public static class EnumExtensions
{
    public static string GetEnumMemberValue<T>(this T enumValue) where T : Enum
    {
        var type = typeof(T);
        var member = type.GetMember(enumValue.ToString());
        var attribute = member[0].GetCustomAttribute<EnumMemberAttribute>();

        return attribute?.Value ?? enumValue.ToString(); // Default to enum name if no EnumMember attribute
    }
}