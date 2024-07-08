namespace IIOTS.WebRMS.Models
{
    public class SmbFileInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsDirectory { get; set; }
        public DateTime LastDateTime { get; set; } 
    }
}
