using JobsityNetChallenge.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;

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
                userName = HttpContext.Session.GetString("senderUId");
                TempData["senderUId"] = userName;
            }

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
