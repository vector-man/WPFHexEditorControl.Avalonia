using System;

namespace WpfHexaEditor.Core.MethodExtention
{
    public static class WithMethodExtention
    {
        /// <summary>
        /// C# like of the very good VB With statement
        /// </summary>
        public static void With<T>(this T obj, Action<T> act) => act(obj);
    }
}
