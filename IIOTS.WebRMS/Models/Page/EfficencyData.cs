namespace IIOTS.WebRMS.Models
{
    public class EfficencyData
    {
        public string? Equ { get; set; }
        public double RunTime { get; set; }
        public double StandbyTime { get; set; }
        public double AlarmTime { get; set; }
        public double OffLineTime { get; set; }
        public double Efficency { get; set; }
    }
}
