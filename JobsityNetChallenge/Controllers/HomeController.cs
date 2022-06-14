using JobsityNetChallenge.Models;
using JobsityNetChallenge.StockBot;
using JobsityNetChallenge.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JobsityNetChallenge.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStockBotClient _stockBotClient;
        private readonly IChatStorage _chatStorage;

        public HomeController(ILogger<HomeController> logger, IStockBotClient stockBotClient, IChatStorage chatStorage)
        {
            _logger = logger;
            _stockBotClient = stockBotClient;
            _chatStorage = chatStorage;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            //await _stockBotClient.GetStockInfo("aapl.us", cancellationToken);
            //var stock = await _stockBotClient.GetStockInfo("aapl.us", cancellationToken);

            //StringBuilder sb = new StringBuilder();
            //sb.AppendLine(stock != null ? $"{stock.Symbol} quote is ${stock.Close} per share." : "");
            //sb.AppendLine("<br />");
            //var messages = _chatStorage.LoadAllMessages();
            //messages = messages.OrderBy(x => x.Id).ToList();
            //foreach (var item in messages.OrderBy(x => x.Id))
            //{
            //    sb.AppendLine($"{item.User} - {item.MessageContent}");
            //    sb.AppendLine("<br />");
            //}
            //ViewData["stockPrice"] = sb.ToString();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
