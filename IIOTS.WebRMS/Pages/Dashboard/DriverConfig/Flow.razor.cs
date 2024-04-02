using Microsoft.AspNetCore.Components;
namespace IIOTS.WebRMS.Pages.Dashboard.DriverConfig
{
    public partial class Flow : ComponentBase
    {
        [Parameter]
        public required string FlowId { get; set; } 
        public string IframeSrc => $"{NodeRedApi.Url}/#flow/{FlowId}";

    }
}
