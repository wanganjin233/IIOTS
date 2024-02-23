namespace IIOTS.Models
{
    public class EdgeLoginInfo
    {
        /// <summary>
        /// 边缘ID
        /// </summary>
        public string? EdgeID { get; set; }
        /// <summary>
        /// 启动时间
        /// </summary>
        public DateTime StartTime { get; set; }=DateTime.Now;
        /// <summary>
        /// 节点状态
        /// </summary>
        public bool State { get; set; } 
        /// <summary>
        /// 边缘进程信息
        /// </summary>
        public List<ProgressLoginInfo> ProgressLoginInfos { get; set; }=new List<ProgressLoginInfo>();
    }
}
