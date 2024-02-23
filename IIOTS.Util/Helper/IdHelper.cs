namespace IIOTS.Util
{  
    //
    // 摘要:
    //     Id获取帮助类
    public static class IdHelper
    {
        internal static IdWorker IdWorker { get; set; }
        internal static IdHelperBootstrapper IdHelperBootstrapper { get; set; }
        //
        // 摘要:
        //     当前WorkerId,范围:1~1023
        public static long WorkerId => IdWorker.WorkerId;

        //
        // 摘要:
        //     获取String型雪花Id
        public static string GetId()
        {
            return GetLongId().ToString();
        }

        //
        // 摘要:
        //     获取Long型雪花Id
        public static long GetLongId()
        {
            return IdWorker.NextId();
        } 
    }
}
