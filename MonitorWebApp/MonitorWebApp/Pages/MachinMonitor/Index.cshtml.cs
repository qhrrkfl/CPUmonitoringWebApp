using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing.Printing;

namespace MonitorWebApp.Pages.MachinMonitor
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IServer _server;
        public string ipport {get;set;}
        public IndexModel(ILogger<IndexModel> logger, IServer server)
        {
            _logger = logger;
            _server = server;
        }
        public void OnGet(string ipPort)
        {
            ipport = ipPort;
        }
    }
}
