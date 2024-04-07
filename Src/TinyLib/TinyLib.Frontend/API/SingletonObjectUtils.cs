using System;
using System.Collections.Generic;
using System.Reflection;

namespace TinyLib.Frontend.API
{
    public static class SingletonObjectUtils
    {
        private static List<Type> _doNotClearList = new Lazy<List<Type>>(() =>
        {
            Type type = typeof(SingletonObject);
            FieldInfo fieldInfo = type.GetField("DoNotClearList", BindingFlags.NonPublic | BindingFlags.Static);
            List<Type> value = (fieldInfo.GetValue(null) as List<Type>)!;
            return value;
        }).Value;

        /// <summary>
        /// 把单例加入不清除列表，游戏在ReturnToMainMenu时会清除不在表内的单例，
        /// </summary>
        public static void DoNotClear<T>() where T : IDisposable
        {
            Type type = typeof(T);
            DoNotClear(type);
        }

        /// <summary>
        /// 把单例加入不清除列表，游戏在ReturnToMainMenu时会清除不在表内的单例，
        /// </summary>
        public static void DoNotClear(Type type)
        {
            if (_doNotClearList.Contains(type))
            {
                _doNotClearList.Add(type);
            }
        }
    }
}
