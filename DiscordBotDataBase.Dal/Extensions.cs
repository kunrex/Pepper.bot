using System;
using System.Collections.Generic;

namespace DiscordBotDataBase.Dal
{
    public static class Extensions
    {
        public static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
                typeof(long), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint)
        };

        public static bool IsNumeric(this Type type) => NumericTypes.Contains(type);
    }
}
