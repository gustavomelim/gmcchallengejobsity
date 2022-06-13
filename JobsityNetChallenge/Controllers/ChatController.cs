using JobsityNetChallenge.Models;
using JobsityNetChallenge.StockBot;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JobsityNetChallenge.Controllers
{
    public class ChatController : Controller
    {
        private readonly ILogger<ChatController> _logger;

        public ChatController(ILogger<ChatController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(CancellationToken cancellationToken)
        {
            ViewData["senderUId"] = HttpContext.Session.Id;
            return View();
        }


    }
}
