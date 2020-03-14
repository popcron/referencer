using Popcron.Referencer;
using System;
using System.Linq;
using System.Reflection;

public static class ReferencesEnumExtensions
{
    /// <summary>
    /// Returns the path from a Path attribute on an enum.
    /// </summary>
    public static string GetPath(this Enum e)
    {
        Type type = e.GetType();
        Array values = Enum.GetValues(type);
        int index = Array.IndexOf(values, e);
        MemberInfo[] memInfo = type.GetMember(type.GetEnumName(values.GetValue(index)));
        if (memInfo[0].GetCustomAttributes(typeof(PathAttribute), false).FirstOrDefault() is PathAttribute pathAttribute)
        {
            return pathAttribute.path;
        }

        return null;
    }
}