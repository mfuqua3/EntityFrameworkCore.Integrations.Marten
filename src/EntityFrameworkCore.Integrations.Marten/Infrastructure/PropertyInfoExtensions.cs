using System.Reflection;

namespace EntityFrameworkCore.Integrations.Marten.Infrastructure;

internal static class PropertyInfoExtensions
{
    public static bool IsStatic(this PropertyInfo property)
        => (property.GetMethod ?? property.SetMethod)!.IsStatic;
}