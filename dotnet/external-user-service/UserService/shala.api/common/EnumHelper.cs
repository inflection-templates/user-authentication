using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

public static class EnumHelper
{
    public static List<(TEnum, string)> GetEnumWithDescriptions<TEnum>() where TEnum : Enum
    {
        return Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .Select(e => (e, GetEnumDescription(e)))
            .ToList();
    }

    public static List<(TEnum EnumValue, string Name, string Description)> GetEnumWithNameAndDescriptions<TEnum>() where TEnum : Enum
    {
        return Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .Select(e => (e, e.ToString(), GetEnumDescription(e)))
            .ToList();
    }

    private static string GetEnumDescription<TEnum>(TEnum enumValue) where TEnum : Enum
    {
        #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

        FieldInfo field = enumValue.GetType().GetField(enumValue.ToString());
        DescriptionAttribute? attribute = field?.GetCustomAttribute<DescriptionAttribute>();

        #pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        return attribute == null ? enumValue.ToString() : attribute.Description;
    }
}
