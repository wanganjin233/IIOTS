namespace IIOTS.Util
{
    public static partial class Extension
    {
        /// <summary>
        /// 转换枚举值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_Enum"></param>
        /// <param name="addressType"></param>
        /// <returns></returns>
        public static bool ToEnum<T>(this string _Enum, out T? addressType)
        {
            System.Enum.TryParse(typeof(T), _Enum, out object? result);
            if (result != null)
            {
                addressType = (T)result;
                return true;
            }
            addressType = default(T);
            return false;
        }
    }
}
