using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SA2MsgTextEditor.Extensions
{
    public static class ExEnum
    {
        private static DisplayAttribute? GetDisplayAttribute(this Enum value)
        {
            return value.GetType().GetMember(value.ToString()).First().GetCustomAttribute<DisplayAttribute>();
        }

        public static string GetDisplayName(this Enum value)
        {
            if (value.GetDisplayAttribute() is DisplayAttribute attr && attr.Name != null)
            {
                return attr.Name;
            }

            return value.ToString();
        }

        public static string GetDescription(this Enum value)
        {
            if (value.GetDisplayAttribute() is DisplayAttribute attr && attr.Description != null)
            {
                return attr.Description;
            }

            return value.ToString();
        }
    }
}
