﻿using Alza.Core.Identity.Dal.Entities;
using Alza.Core.Module.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestWeb.Abstraction;
using TestWeb.Models;
using TestWeb.Models.AccountViewModels;
using Trainee.Core.Business;
using Trainee.User.Business;
using Trainee.User.DAL.Entities;
using Trainee.Core.DAL.Entities;
using System.Collections.Generic;

namespace TestWeb.Controllers
{
    public class AccountController : Controller
    {
        private IHostingEnvironment _env;
        private ILogger<AccountController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly CountryService _countryService;
        private readonly UserService _profileService;

        public AccountController(
            IHostingEnvironment env,
            ILogger<AccountController> logger,
            IStringLocalizer<AccountController> localizerizer,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            CountryService countryService,
            UserService userService)
        {
            _env = env;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _countryService = countryService;
            _profileService = userService;
        }

        //
        // GET: /Account/Login
        [HttpGet]
        [Route("/Account/Login")]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            try
            {
                //Nesmi byt prihlasen
                if (_signInManager.IsSignedIn(User))
                    _signInManager.SignOutAsync();
                
                ViewData["ReturnUrl"] = returnUrl;

                return View("Login");
            }
            catch (Exception e)
            {
                return ExceptionActionResult(e);
            }
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [Route("/Account/Login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            try
            {
                //Nesmi byt prihlasen
                if (_signInManager.IsSignedIn(User))
                    return ErrorActionResult("Uživatel již je přihlášen");
               

                ViewData["ReturnUrl"] = returnUrl;
                if (ModelState.IsValid)
                {

                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                    if (result.Succeeded)
                    {

                        return RedirectToLocal(returnUrl);
                    }
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning(2, "someString");
                        ModelState.AddModelError("UserName", "someString");
                        return View("Lockout");
                    }
                    else
                    {

                        var isExist = await _userManager.FindByEmailAsync(model.Email);
                        if (isExist == null)
                        {
                            ViewData["EmailUnknown"] = true;
                            /*
                            _logger.LogWarning(2, "Neznámý e-mail.");
                            ModelState.AddModelError("UserName", "Neznámý e-mail.");
                            */
                        }
                        else
                        {
                            ViewData["WrongPassword"] = true;
                            /*
                            _logger.LogWarning(2, "Nesprávné heslo.");
                            ModelState.AddModelError("Password", "Nesprávné heslo.");
                            */
                        }

                        return View(model);

                    }
                }

                // If we got this far, something failed, redisplay form
                return View(model);

            }
            catch (Exception e)
            {
                return ExceptionActionResult(e);
            }
        }

        //
        // GET: /Account/Register
        [HttpGet]
        [Route("/Account/Register")]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            try
            {
                //Nesmi byt prihlasen
                if (_signInManager.IsSignedIn(User))
                    _signInManager.SignOutAsync();


                ViewData["ReturnUrl"] = returnUrl;

                /*************Added**************/

                var result = _countryService.GetAllCountries();
                RegisterViewModel model;
                if (result.isOK)            //on dummy data invert condition
                {
                    model = new RegisterViewModel
                    { /*Countries = new List<Country> { new Country { Name = "Prr", CountryCode = "Byy", Id = 1 } }*/
                        Countries = (List<Country>)result.data
                    };

                }
                else
                    throw new Exception("Invalid model, database type error");

                /********************************/

                return View("Register", model);
            }
            catch (Exception e)
            {
                return ExceptionActionResult(e);
            }
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [Route("/Account/Register")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            try
            {
                var result = _countryService.GetAllCountries();
                if (result.isOK)
                    model.Countries = (List<Country>)result.data;




                //Nesmi byt prihlasen
                if (_signInManager.IsSignedIn(User))
                {
                    //await _signInManager.SignOutAsync();
                    return ErrorActionResult("Uživatel již je přihlášen");
                }

                ViewData["ReturnUrl"] = returnUrl;
                if (ModelState.IsValid)
                {
                    var user = new ApplicationUser { UserName = model.Email, Email = model.Email };


                    //Kontrola jestli uzivatel uz neexistuje
                    if (String.IsNullOrEmpty(user.NormalizedUserName))
                        user.NormalizedUserName = user.UserName;
                    var exist = await _userManager.GetUserIdAsync(user);

                    if (exist != "")
                    {
                        ViewData["UserExists"] = true;
                        return View(model);
                    }
                    else
                    {
                        ViewData["UserExists"] = false;
                    }
                    // return RedirectToAction("Uzivatel existuje");




                    //osetreni username

                    string usernameTemp = model.Email.Split('@')[0];

                    user.UserName = usernameTemp;


                    //Create AspNet Identity User
                    IdentityResult res = await _userManager.CreateAsync(user, model.Password);
                    IdentityResult res2 = null;
                    if (res.Succeeded)
                    {
                        //zjisteni ulozeneho Id uzivatele
                        var resId = await _userManager.GetUserIdAsync(user);
                        user.Id = Int32.Parse(resId);

                        //prirazeni uzivatele do Role
                        res2 = await _userManager.AddToRoleAsync(user, "User");

                        if (res2.Succeeded)
                        {
                            /*********************/ //should be redone, if it would be checked anywhere else (for now it is only in Register and Edit)

                            List<object> completionList = new List<object> { model.Street, model.City, model.PostalCode, model.Phone };
                            int profileState = 1;

                            //to be changed if there would be more states
                            foreach (object o in completionList)
                                profileState = o == null ? 2 : profileState;

                            /*********************/



                            var userProfile = new UserProfile
                            {
                                Address = model.Street,
                                City = model.City,
                                Id = user.Id,
                                Name = model.Name,
                                Surname = model.Surname,
                                PhoneNumber = model.Phone,
                                PostalCode = model.PostalCode,
                                ProfileStateId = profileState                               
                            };

                            

                            var countryByCode = _countryService.GetCountry(model.CountryCode);
                            if (countryByCode.isOK)
                            {
                                // userProfile.Country = (Country)countryByCode.data;
                                userProfile.CountryId = ((Country)countryByCode.data).Id;
                            }
                            

                            _profileService.AddUserProfile(userProfile);

                            ViewData["RegisterCompleted"] = true;

                            return View(model);

                        }

                    }
                    else
                    {
                        return RedirectToAction("CHYBA");
                    }



                    await _signInManager.SignInAsync(user, isPersistent: true);




                    //??
                    return RedirectToAction("Forbidden");
                }



                // If we got this far, something failed, redisplay form
                return View(model);

            }
            catch (Exception e)
            {
                return ExceptionActionResult(e);
            }
        }


        //
        // GET: /Account/Edit
        [HttpGet]
        [Route("/Account/Edit")]
        public async Task<IActionResult> Edit(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            try
            {
                
                
                if (!_signInManager.IsSignedIn(User))
                    return RedirectToAction("Login", returnUrl);

                var userIdentity = await _userManager.GetUserAsync(User);

                UserProfile userProfile;
                var result = _profileService.GetUserProfile(userIdentity.Id);

                if (!result.isOK)
                    throw new Exception("User profile not found");

                userProfile = (UserProfile)result.data;

                EditViewModel editModel = new EditViewModel
                {
                    Name = userProfile.Name,
                    Surname = userProfile.Surname,
                    City = userProfile.City,
                    Country = userProfile.Country,
                    //CountryCode = userProfile.Country.Name,
                    PostalCode = userProfile.PostalCode,
                    Street = userProfile.Address, Email = userIdentity.Email
                    //Password = null
                };

                var resultCountry = _countryService.GetAllCountries();
                if (resultCountry.isOK)
                    editModel.Countries = (List<Country>)resultCountry.data;
                
                return View(editModel);
               

            }
            catch (Exception e)
            {
                return ExceptionActionResult(e);
            }


        }

        //
        // POST: /Account/Edit
        //[HttpPost]
        [Route("/Account/Edit")]
        public async Task<IActionResult> Edit(EditViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            try
            {
                var userIdentity = await _userManager.GetUserAsync(User);

                if (ModelState.IsValid)
                {
                    bool isPassCorrect = await _userManager.CheckPasswordAsync(userIdentity, model.Password);
                    
                    model.Email = userIdentity.Email;

                    var updateCountry = _countryService.GetCountry(model.CountryCode);
                    model.Country = (Country)updateCountry.data;

                    /*******************/ //should be redone, if it would be checked anywhere else (for now it is only in Register and Edit)

                    List<object> completionList = new List<object> { model.Street, model.City, model.PostalCode, /*model.Phone*/ };
                    int profileState = 1; //complete

                    //to be changed if there would be more states
                    foreach (object o in completionList)
                        profileState = o == null ? 2 : profileState; //incomplete

                    /*******************/

                    if (isPassCorrect)
                    {
                        UserProfile updatedProfile = (UserProfile)_profileService.GetUserProfile(userIdentity.Id).data;
                        updatedProfile.PostalCode = model.PostalCode;
                        updatedProfile.Address = model.Street;
                        updatedProfile.City = model.City;
                        updatedProfile.Name = model.Name;
                        updatedProfile.Surname = model.Surname;
                        updatedProfile.CountryId = model.Country.Id;
                        updatedProfile.ProfileStateId = profileState;


                        _profileService.UpdateUserProfile(updatedProfile);
                    }
                    else
                    {
                        _logger.LogWarning(2, "Nesprávné heslo.");
                        ModelState.AddModelError("Password", "Nesprávné heslo.");
                        ViewData["WrongPassword"] = true;
                    }

                    return RedirectToAction("Details");


                }
                else
                {
                    return RedirectToAction("CHYBA");
                }
            }
            catch (Exception e)
            {
                return ExceptionActionResult(e);
            }


        }


        //
        // GET: /Account/Edit
        [HttpGet]
        [Route("/Account/Details")]
        public async Task<IActionResult> Details(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            try
            {

                if (!_signInManager.IsSignedIn(User))
                    return RedirectToAction("Login", returnUrl);

                var userIdentity = await _userManager.GetUserAsync(User);

                UserProfile userProfile;
                var result = _profileService.GetUserProfile(userIdentity.Id);

                if (!result.isOK)
                    throw new Exception("User profile not found");

                userProfile = (UserProfile)result.data;

                DetailsViewModel detailsModel = new DetailsViewModel
                {
                    Name = userProfile.Name,
                    Surname = userProfile.Surname,
                    City = userProfile.City,
                    Country = userProfile.Country,
                    //CountryCode = userProfile.Country.Name,
                    PostalCode = userProfile.PostalCode,
                    Street = userProfile.Address,
                    //Password = null,
                    Email = userIdentity.Email
                };

                var resultCountry = _countryService.GetAllCountries();

                if (resultCountry.isOK)
                    detailsModel.Countries = (List<Country>)resultCountry.data;
                else
                    throw new Exception("Country database error");

                return View(detailsModel);

            }
            catch (Exception e)
            {
                return ExceptionActionResult(e);
            }

        }
        

        //
        // GET: /Account/Forbidden
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Forbidden()
        {
            try
            {
                return Forbid();
            }
            catch (Exception e)
            {
                return ExceptionActionResult(e);
            }
        }


        // POST: /Account/Logout
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            catch (Exception e)
            {
                return ExceptionActionResult(e);
            }
        }



        #region Helpers




        public void ErrorToModel(AlzaAdminDTO dto, BaseViewModel model)
        {
            model.ErrorNo = dto.errorNo;
            foreach (var item in dto.errors)
            {
                model.Errors.Add(item);
            }
        }
        public AlzaAdminDTO InvalidModel()
        {
            AlzaAdminDTO invalidresult = AlzaAdminDTO.False;
            foreach (var item in ModelState.ToList())
            {

                foreach (var item2 in item.Value.Errors)
                {
                    invalidresult.errors.Add(item.Key + " - " + item2.ErrorMessage);
                }

            }
            return invalidresult;
        }


        /// <summary>
        /// HELPER return and log error
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public AlzaAdminDTO InvalidIdentityResultDTO(IdentityResult result)
        {
            Guid errNo = Guid.NewGuid();
            StringBuilder res = new StringBuilder();

            foreach (var error in result.Errors)
            {
                res.AppendLine(error.Description);
            }

            _logger.LogError(errNo + " - " + res.ToString());
            return AlzaAdminDTO.Error(errNo, res.ToString());

        }

        /// <summary>
        /// HELPER return and log error
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public RedirectToActionResult InvalidIdentityResultActionResult(IdentityResult result)
        {
            Guid errNo = Guid.NewGuid();
            StringBuilder res = new StringBuilder();

            foreach (var error in result.Errors)
            {
                res.AppendLine(error.Description);
            }

            _logger.LogError(errNo + " - " + res.ToString());


            BaseViewModel model = new BaseViewModel();
            model.ErrorNo = errNo;
            model.Errors.Add(res.ToString());

            return RedirectToAction("someString", "someString", model);
        }



        /// <summary>
        /// HELPER return and log error
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public AlzaAdminDTO ErrorDTO(string text)
        {
            Guid errNo = Guid.NewGuid();
            _logger.LogError(errNo + " - " + text);
            return AlzaAdminDTO.Error(errNo, "someString");
        }

        /// <summary>
        /// HELPER return and log error
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public IActionResult ErrorActionResult(string text)
        {
            Guid errNo = Guid.NewGuid();
            _logger.LogError(errNo + " - " + text);

            BaseViewModel model = new BaseViewModel();
            model.ErrorNo = errNo;
            model.Errors.Add(text);



            /*************************************************************************/
            //BUG Notification

            //var userId = _userManager.GetUserId(User);
            //if (String.IsNullOrEmpty(userId))
            //    userId = "0";

            //var bug = new BugNotification
            //{
            //    UserProfileId = Int32.Parse(userId),
            //    Severity = "Error",
            //    ErrorNo = errNo,
            //    CreatedDate = DateTime.Now,
            //    Note = text
            //};
            //_mediator.PublishAsync(bug);

            /*************************************************************************/



            //FINALni varianta Custom ActionResult
            return new AlzaActionResult("someString", model);

        }

        /// <summary>
        /// HELPER return and log error
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public RedirectToActionResult ErrorActionResult(AlzaAdminDTO err)
        {
            _logger.LogError(err.errorNo + " - " + err.errorText);

            BaseViewModel model = new BaseViewModel();
            model.ErrorNo = err.errorNo;
            model.Errors.Add(err.errorText);

            return RedirectToAction("someString", "someString", model);
        }



        /// <summary>
        /// HELPER return and log error
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public AlzaAdminDTO ExceptionDTO(Exception e)
        {
            Guid errNo = Guid.NewGuid();
            _logger.LogError(errNo + " - " + e.Message + Environment.NewLine + e.StackTrace);
            return AlzaAdminDTO.Error(errNo, e.Message + Environment.NewLine + e.StackTrace);
        }

        /// <summary>
        /// HELPER return and log error
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public IActionResult ExceptionActionResult(Exception e)
        {
            Guid errNo = Guid.NewGuid();
            _logger.LogError(errNo + " - " + e.Message + Environment.NewLine + e.StackTrace);


            BaseViewModel model = new BaseViewModel();
            model.ErrorNo = errNo;
            model.Errors.Add(e.Message + Environment.NewLine + e.StackTrace);



            /*************************************************************************/
            //BUG Notification

            //var userId = _userManager.GetUserId(User);
            //if (String.IsNullOrEmpty(userId))
            //    userId = "0";

            //var bug = new BugNotification
            //{
            //    UserProfileId = Int32.Parse(userId),
            //    Severity = "Critical",
            //    ErrorNo = errNo,
            //    CreatedDate = DateTime.Now,
            //    Note = e.Message + Environment.NewLine + e.StackTrace
            //};
            //_mediator.PublishAsync(bug);

            /*************************************************************************/



            return new AlzaActionResult("someString", model);

        }



        #endregion



        //Odstranit??
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect("/");
            }
        }

    }
}
