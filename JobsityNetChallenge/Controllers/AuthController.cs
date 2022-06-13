using JobsityNetChallenge.Domain;
using JobsityNetChallenge.Domain.Utils;
using JobsityNetChallenge.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace JobsityNetChallenge.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IChatStorage _chatStorage;
        private readonly bool _debug = true;

        public AuthController(IChatStorage chatStorage)
        {
            _chatStorage = chatStorage;
        }

        [HttpPost]
        public ActionResult Login()
        {
            string userName = Request.Form["username"];
            string password = Request.Form["password"];
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {

                TempData["errorMessage"] = "Username or password cannot be null !";
                return Redirect("/");
            }

            User user = _chatStorage.FetchUser(userName);
            if (user == null)
            {
                if (_debug)
                {
                    user = new User() { Id = userName.ToLower() };
                    user.Password = CryptographyUtil.Encrypt("test");
                    _chatStorage.SaveUser(user);
                }
                else
                {
                    TempData["errorMessage"] = "Wrong username or password !";
                    return Redirect("/");
                }
            }

            string inputPassword = CryptographyUtil.Encrypt(password);
            if (!inputPassword.Equals(user.Password))
            {
                TempData["errorMessage"] = "Wrong username or password !";
                return Redirect("/");
            }
            TempData["senderUId"] = user.Id;
            return Redirect("/chat");
        }

    }
}
