using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonitoringService.MonitorServer;

namespace MonitorWebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, IMonitorServer server)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            int a = 0;
            a++;
            
        }

      
    }
}