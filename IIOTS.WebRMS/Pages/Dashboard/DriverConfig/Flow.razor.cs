using Microsoft.AspNetCore.Components;
namespace IIOTS.WebRMS.Pages.Dashboard.DriverConfig
{
    public partial class Flow : ComponentBase
    {
        [Parameter]
        public required string FlowId { get; set; }
        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;
        public string IframeSrc => "http://10.164.18.219:30001/#flow/" + FlowId;
 
        protected override async Task OnInitializedAsync()
        {
            if (FlowId == "new")
            {


                NavigationManager.NavigateTo($"/Flow/f6f2187d.f17ca8"); 
            }
            await base.OnInitializedAsync();
        }
    }
}
