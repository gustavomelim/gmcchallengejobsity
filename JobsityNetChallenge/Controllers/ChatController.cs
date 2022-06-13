using JobsityNetChallenge.Models;
using JobsityNetChallenge.StockBot;
using JobsityNetChallenge.Storage;
using Microsoft.AspNetCore.Http;
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
    public class ChatController : BaseController
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IChatStorage _chatStorage;

        public ChatController(ILogger<ChatController> logger, IChatStorage chatStorage)
        {
            _logger = logger;
            _chatStorage = chatStorage;
        }

        public ActionResult Index(CancellationToken cancellationToken)
        {
            var userName = TempData["senderUId"] as string;
            if (userName == null)
            {
                return Redirect("/");
            }

            var currentUser = _chatStorage.FetchUser(userName);
            if (currentUser == null)
            {
                return Redirect("/");
            }
            ViewData["senderUId"] = currentUser.Id;
            return View();
        }

    }
}
