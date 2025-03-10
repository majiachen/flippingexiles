using System;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;

public static class EnumExtensions
{
    private static readonly ILogger _logger;

    // Static constructor to initialize logger
    static EnumExtensions()
    {
        using var factory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = factory.CreateLogger("EnumExtensions");
    }

    /// <summary>
    /// Retrieves the EnumMember value of an enum or falls back to the default name.
    /// </summary>
    public static string GetEnumMemberValue<T>(this T enumValue) where T : Enum
    {
        var type = enumValue.GetType(); // Get the actual enum type (e.g., Essence, Currency)
        var enumName = enumValue.ToString();
        var member = type.GetMember(enumName);

        if (member.Length == 0)
        {
            _logger.LogWarning("Enum value '{EnumValue}' does not exist in enum type '{EnumType}'. Returning default name.", enumName, type.Name);
            return enumName; // Return default enum name
        }

        var attribute = member[0].GetCustomAttribute<EnumMemberAttribute>();

        if (attribute == null)
        {
            _logger.LogWarning("Enum value '{EnumValue}' in enum type '{EnumType}' does not have an EnumMember attribute. Using default name.", enumName, type.Name);
            return enumName;
        }

        _logger.LogDebug("Retrieved EnumMember value '{EnumMemberValue}' for '{EnumValue}' in enum type '{EnumType}'.", attribute.Value, enumName, type.Name);
        return attribute.Value;
    }
}