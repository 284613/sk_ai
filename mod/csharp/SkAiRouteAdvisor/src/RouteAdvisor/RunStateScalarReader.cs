using System.Reflection;
using MegaCrit.Sts2.Core.Runs;

namespace SkAiRouteAdvisor.RouteAdvisor;

internal static class RunStateScalarReader
{
    public static int GetInt(RunState runState, params string[] names)
    {
        foreach (var name in names)
        {
            var value = GetMemberValue(runState, name);
            if (value is int intValue)
            {
                return intValue;
            }
        }

        return 0;
    }

    public static string GetString(RunState runState, params string[] names)
    {
        foreach (var name in names)
        {
            var value = GetMemberValue(runState, name);
            if (value is string text)
            {
                return text;
            }
        }

        return string.Empty;
    }

    private static object? GetMemberValue(object source, string name)
    {
        var type = source.GetType();
        var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        if (property != null)
        {
            return property.GetValue(source);
        }

        var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
        return field?.GetValue(source);
    }
}
