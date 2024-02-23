namespace IIOTS.Util
{ //
    // 摘要:
    //     配置引导
    public class IdHelperBootstrapper
    {
        //
        // 摘要:
        //     机器Id
        //
        // 值:
        //     机器Id
        protected long _worderId { get; set; }

        //
        // 摘要:
        //     获取机器Id
        protected virtual long GetWorkerId()
        {
            return _worderId;
        }

        //
        // 摘要:
        //     是否可用
        public virtual bool Available()
        {
            return true;
        }

        //
        // 摘要:
        //     设置机器Id
        //
        // 参数:
        //   workderId:
        //     机器Id
        public IdHelperBootstrapper SetWorkderId(long workderId)
        {
            _worderId = workderId;
            return this;
        }

        //
        // 摘要:
        //     完成配置
        public void Boot()
        {
            IdHelper.IdWorker = new IdWorker(GetWorkerId(), 0L);
            IdHelper.IdHelperBootstrapper = this;
        }
    }
}
