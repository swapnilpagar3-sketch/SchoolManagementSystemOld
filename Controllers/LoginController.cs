using EntityFrameworkCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
//using System.Data.SqlClient;
using Microsoft.Data.SqlClient;

namespace EntityFrameworkCore.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context, ILogger<LoginController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> Login(LoginViewModel model)
        //{
        //    try
        //    {
        //        _logger.LogInformation("Login attempt started for Username: {Username}", model.Username);

        //        if (!ModelState.IsValid)
        //            return View(model);

        //        var user = await _context.LoginResults
        //            .FromSqlInterpolated(
        //                $"EXEC uspQry_ValidateUser {model.Username}, {model.Password}")
        //            .ToListAsync();

        //        if (!user.Any() || user.First().UserId == 0)
        //        {
        //            _logger.LogWarning("Login failed. Invalid credentials for Username: {Username}", model.Username);

        //            TempData["Error"] = "Username or Password not matched";
        //            return RedirectToAction("Login");
        //        }

        //        // Store UserId
        //        Response.Cookies.Append(
        //            "UserId",
        //            user.First().UserId.ToString());

        //        // Store Username
        //        Response.Cookies.Append(
        //            "Username",
        //            user.First().Username);

        //        // Store Profile Image
        //        Response.Cookies.Append(
        //            "ProfileImage",
        //            user.First().ProfileImage ?? "");

        //        _logger.LogInformation("User cookies created successfully. UserId: {UserId}", user.First().UserId);

        //        if (model.RememberMe)
        //        {
        //            Response.Cookies.Append(
        //                "UserId",
        //                user.First().UserId.ToString(),
        //                new CookieOptions
        //                {
        //                    Expires = DateTimeOffset.Now.AddDays(30),
        //                    HttpOnly = true,
        //                    IsEssential = true
        //                });
        //        }
        //        else
        //        {
        //            Response.Cookies.Append(
        //                "UserId",
        //                user.First().UserId.ToString(),
        //                new CookieOptions
        //                {
        //                    HttpOnly = true,
        //                    IsEssential = true
        //                });
        //        }

        //        TempData["Success"] = "Login Successful";

        //        return RedirectToAction("Index", "Home");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred during login. Username: {Username}", model?.Username);

        //        TempData["Error"] = ex.Message;

        //        return RedirectToAction("Login");
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                _logger.LogInformation(
                    "Login attempt started for Username: {Username}",
                    model.Username);

                if (!ModelState.IsValid)
                    return View(model);

                LoginResult? user = null;

                var connection =
                    _context.Database.GetDbConnection();

                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "uspQry_ValidateUser";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(
                        new SqlParameter("@Username", model.Username));

                    command.Parameters.Add(
                        new SqlParameter("@Password", model.Password));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            user = new LoginResult
                            {
                                UserId = reader["UserId"] != DBNull.Value
                                    ? Convert.ToInt32(reader["UserId"])
                                    : 0,

                                Username = reader["Username"]?.ToString(),

                                ProfileImage = reader["ProfileImage"]?.ToString()
                            };
                        }
                    }
                }

                await connection.CloseAsync();

                if (user == null || user.UserId == 0)
                {
                    _logger.LogWarning(
                        "Login failed. Invalid credentials for Username: {Username}",
                        model.Username);

                    TempData["Error"] =
                        "Username or Password not matched";

                    return RedirectToAction("Login");
                }

                Response.Cookies.Append(
                    "UserId",
                    user.UserId.ToString());

                Response.Cookies.Append(
                    "Username",
                    user.Username ?? "");

                Response.Cookies.Append(
                    "ProfileImage",
                    user.ProfileImage ?? "");

                _logger.LogInformation(
                    "User cookies created successfully. UserId: {UserId}",
                    user.UserId);

                if (model.RememberMe)
                {
                    Response.Cookies.Append(
                        "UserId",
                        user.UserId.ToString(),
                        new CookieOptions
                        {
                            Expires = DateTimeOffset.Now.AddDays(30),
                            HttpOnly = true,
                            IsEssential = true
                        });

                    Response.Cookies.Append(
                        "Username",
                        user.Username ?? "",
                        new CookieOptions
                        {
                            Expires = DateTimeOffset.Now.AddDays(30),
                            HttpOnly = true,
                            IsEssential = true
                        });

                    Response.Cookies.Append(
                        "ProfileImage",
                        user.ProfileImage ?? "",
                        new CookieOptions
                        {
                            Expires = DateTimeOffset.Now.AddDays(30),
                            HttpOnly = true,
                            IsEssential = true
                        });
                }
                else
                {
                    Response.Cookies.Append(
                        "UserId",
                        user.UserId.ToString(),
                        new CookieOptions
                        {
                            HttpOnly = true,
                            IsEssential = true
                        });

                    Response.Cookies.Append(
                        "Username",
                        user.Username ?? "",
                        new CookieOptions
                        {
                            HttpOnly = true,
                            IsEssential = true
                        });

                    Response.Cookies.Append(
                        "ProfileImage",
                        user.ProfileImage ?? "",
                        new CookieOptions
                        {
                            HttpOnly = true,
                            IsEssential = true
                        });
                }

                TempData["Success"] = "Login Successful";

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred during login. Username: {Username}",
                    model?.Username);

                //TempData["Error"] = ex.Message;

                TempData["Error"] = "Username and Password Not Matched";

                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var user = _context.Users
                                   .FirstOrDefault(x =>
                                       x.EmailAddress == model.EmailAddress);

                if (user == null)
                {
                    TempData["Error"] = "Email Address not found";
                    return View(model);
                }

                string token = Guid.NewGuid().ToString();

                user.ResetToken = token;
                user.ResetTokenExpiry = DateTime.Now.AddHours(1);

                _context.SaveChanges();

                var resetLink =
                    Url.Action(
                        "ResetPassword",
                        "Login",
                        new { token = token },
                        Request.Scheme);

                await SendResetEmail(
                    model.EmailAddress,
                    resetLink);

                TempData["Success"] =
                    "Password reset link sent successfully";

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> SignUp(SignUpViewModel model)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //            return View(model);

        //        await _context.Database.ExecuteSqlInterpolatedAsync(
        //            $"EXEC uspQry_SaveUser {model.Username}, {model.EmailAddress}, {model.Password}");

        //        TempData["Success"] = "Registration Successful";

        //        return RedirectToAction("Login");
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = ex.Message;

        //        return View(model);
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> SignUp(SignUpViewModel model)
        //{
        //    try
        //    {
        //        _logger.LogInformation("User registration initiated. Username: {Username}, Email: {Email}",model.Username,model.EmailAddress);

        //        if (!ModelState.IsValid)
        //            return View(model);

        //        string imagePath = "";

        //        if (model.ImageFile != null)
        //        {
        //            //string fileName =
        //            //    Guid.NewGuid().ToString() +
        //            //    Path.GetExtension(model.ImageFile.FileName);

        //            _logger.LogInformation("Profile image upload started. File Name: {FileName}",model.ImageFile.FileName);

        //            string fileName = model.ImageFile.FileName;

        //            string path =
        //                Path.Combine(
        //                    Directory.GetCurrentDirectory(),
        //                    "wwwroot/UserImages",
        //                    fileName);

        //            using (var stream = new FileStream(path, FileMode.Create))
        //            {
        //                model.ImageFile.CopyTo(stream);
        //            }

        //            imagePath = "/UserImages/" + fileName;
        //        }

        //        await _context.Database.ExecuteSqlInterpolatedAsync(
        //            $"EXEC uspQry_SaveUser {model.Username}, {model.EmailAddress}, {model.Password}, {imagePath}");

        //        _logger.LogInformation("User registered successfully. Username: {Username}, Email: {Email}",model.Username,model.EmailAddress);

        //        TempData["Success"] = "Registration Successful";

        //        return RedirectToAction("Login");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex,"Error occurred during user registration. Username: {Username}, Email: {Email}",model?.Username,model?.EmailAddress);

        //        TempData["Error"] = ex.Message;
        //        return View(model);
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            try
            {
                _logger.LogInformation(
                    "User registration initiated. Username: {Username}, Email: {Email}",
                    model.Username,
                    model.EmailAddress);

                if (!ModelState.IsValid)
                    return View(model);

                // Check if Username already exists
                bool usernameExists = await _context.Users
                    .AnyAsync(x => x.Username == model.Username);

                if (usernameExists)
                {
                    _logger.LogWarning(
                        "Registration failed. Username already exists: {Username}",
                        model.Username);

                    TempData["Error"] = "Username already exists";

                    return View(model);
                }

                // Check if Email already exists
                bool emailExists = await _context.Users
                    .AnyAsync(x => x.EmailAddress == model.EmailAddress);

                if (emailExists)
                {
                    _logger.LogWarning(
                        "Registration failed. Email already exists: {Email}",
                        model.EmailAddress);

                    TempData["Error"] = "Email already exists";

                    return View(model);
                }

                string imagePath = "";

                if (model.ImageFile != null)
                {
                    _logger.LogInformation(
                        "Profile image upload started. File Name: {FileName}",
                        model.ImageFile.FileName);

                    string folderPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "UserImages");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string fileName = model.ImageFile.FileName;

                    string filePath = Path.Combine(
                        folderPath,
                        fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }

                    imagePath = "/UserImages/" + fileName;
                }

                var connection = _context.Database.GetDbConnection();

                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "uspQry_SaveUser";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(
                        new SqlParameter("@Username", model.Username));

                    command.Parameters.Add(
                        new SqlParameter("@EmailAddress", model.EmailAddress));

                    command.Parameters.Add(
                        new SqlParameter("@Password", model.Password));

                    command.Parameters.Add(
                        new SqlParameter("@ProfileImage", imagePath));

                    await command.ExecuteNonQueryAsync();
                }

                await connection.CloseAsync();

                _logger.LogInformation(
                    "User registered successfully. Username: {Username}, Email: {Email}",
                    model.Username,
                    model.EmailAddress);

                TempData["Success"] = "Registration Successful";

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred during user registration. Username: {Username}, Email: {Email}",
                    model?.Username,
                    model?.EmailAddress);

                TempData["Error"] =
                    "Something went wrong while registering the user.";

                return View(model);
            }
        }

        public IActionResult Logout()
        {
            try
            {
                Response.Cookies.Delete("UserId");

                TempData["Success"] = "Logged out successfully";

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction("Login");
            }
        }

        private async Task SendResetEmail( string email, string resetLink)
        {
            var emailMessage = new MimeKit.MimeMessage();

            emailMessage.From.Add(
                new MimeKit.MailboxAddress(
                    "Hitachi Payment Services",
                    "swapnilpagar3@gmail.com"));

            emailMessage.To.Add(
                new MimeKit.MailboxAddress("", email));

            emailMessage.Subject = "Password Reset";

            emailMessage.Body =
                new MimeKit.TextPart("html")
                {
                    Text =
                    $"<h3>Password Reset Request</h3>" +
                    $"<p>Click below link:</p>" +
                    $"<a href='{resetLink}'>Reset Password</a>"
                };

            using var client = new MailKit.Net.Smtp.SmtpClient();

            await client.ConnectAsync(
                "smtp.gmail.com",
                587,
                MailKit.Security.SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(
                "swapnilpagar3@gmail.com",
                "Pagar@925129");

            await client.SendAsync(emailMessage);

            await client.DisconnectAsync(true);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            var model = new ResetPasswordViewModel
            {
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ResetPassword( ResetPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var user = _context.Users
                    .FirstOrDefault(x =>
                        x.ResetToken == model.Token &&
                        x.ResetTokenExpiry > DateTime.Now);

                if (user == null)
                {
                    TempData["Error"] =
                        "Invalid or Expired Token";

                    return View(model);
                }

                user.Password = model.NewPassword;

                user.ResetToken = null;
                user.ResetTokenExpiry = null;

                _context.SaveChanges();

                TempData["Success"] =
                    "Password Reset Successfully";

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }
    }
}