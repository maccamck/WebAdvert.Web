using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.Accounts;

namespace WebAdvert.Web.Controllers
{
    public class Accounts : Controller
    {
		private readonly SignInManager<CognitoUser> _signInManager;
		private readonly UserManager<CognitoUser> _userManager;
		private readonly CognitoUserPool _pool;

		public Accounts(SignInManager<CognitoUser> signInManager, UserManager<CognitoUser> userManager, CognitoUserPool pool)
		{
			_signInManager = signInManager;
			_userManager = userManager;
			_pool = pool;
		}

		public IActionResult Index()
        {
            return View();
        }

		// **********************************   SIGNUP ***************************************

		public async Task<IActionResult> Signup()
		{
			var model = new SignupModel();
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Signup(SignupModel model)
		{
			if (ModelState.IsValid)
			{
				var user = _pool.GetUser(model.Email);

				if(user.Status != null)
                {
					ModelState.AddModelError("UserExists", "User with this email already exists");
					return View(model);
                }

				//todo - "Shouldnt be using hardcoded "Name"
				user.Attributes.Add("name", model.Email);
				var createdUser = await _userManager.CreateAsync(user, model.Password).ConfigureAwait(false);

				if(createdUser.Succeeded)
                {
					return RedirectToAction("Confirm");
                }
			}
			return View(model);
		}

		//       * **********************************   CONFIRM ***************************************

		public async Task<IActionResult> Confirm()
		{
			var model = new ConfirmModel();
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Confirm_Post(ConfirmModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(model.Email);
				if (user == null)
				{
					ModelState.AddModelError("NotFound", "A user with the given email was not found");
					return View(model);
				}
				//var result = await _userManager.ConfirmEmailAsync(user, model.Code);
				var result = await ((CognitoUserManager<CognitoUser>)_userManager).ConfirmSignUpAsync(user, model.Code, false);

				if (result.Succeeded)
				{
					return RedirectToAction("Index", "Home");
				}
				else
				{
					foreach (var item in result.Errors)
					{
						ModelState.AddModelError(item.Code, item.Description);
					}
					return View(model);
				}

			}

			// return!
			return View(model);
		}
	}
}
