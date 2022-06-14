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

            User user = _chatStorage.FetchUser(userName.ToLowerInvariant());
            if (user == null)
            {
                TempData["errorMessage"] = "Wrong username or password !";
                return Redirect("/");
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

        [HttpGet]
        [Route("Auth/Register/{id}/{password}")]

        public ActionResult Register(string id, string password)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest($"username and password cannot be null");
            }

            int minCharacter = 3;
            if (id.Length < minCharacter || password.Length < minCharacter)
            {
                return BadRequest($"username and password should have at least {minCharacter} characters.");
            }


            User user = _chatStorage.FetchUser(id.ToLowerInvariant());
            if (user == null)
            {
                user = new User()
                {
                    Id = id.ToLowerInvariant(),
                    Password = CryptographyUtil.Encrypt(password)
                };
                _chatStorage.SaveUser(user);
            }
            else
            {
                return BadRequest($"User {id} already exists.");
            }
            return new OkObjectResult($"User {id} registered at database !");
        }

    }
}
