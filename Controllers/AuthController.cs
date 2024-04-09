using RacerBooksAPI.Interfaces;
using RacerBooksAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Firebase.Auth;

namespace RacerBooksAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        RacerbooksContext _context = new RacerbooksContext();
        private readonly IUnsuccessfulLoginLogger _logger;
        FirebaseAuthProvider auth;

        public AuthController(RacerbooksContext context, IUnsuccessfulLoginLogger logger)
        {
            _context = context;
            _logger = logger;
            auth = new FirebaseAuthProvider(new FirebaseConfig(Environment.GetEnvironmentVariable("FirebaseEBBooks")));
        }

        // POST: api/Auth/AdminLogin
        [HttpPost("AdminLogin")]
        public async Task<IActionResult> AdminLogin([FromBody] LoginModel login)
        {
            try
            {
                var fbAuthLink = await auth.SignInWithEmailAndPasswordAsync(login.Email, login.Password);
                string currentUserId = fbAuthLink.User.LocalId;

                if (currentUserId == null)
                {
                    return Unauthorized(new Error("Token missing!"));
                }

                //Error 400 if incomplete
                if (login.Email == null || login.Password == null)
                {
                    return BadRequest(new Error("Not all details have been submitted!"));
                }

                if (currentUserId != null)
                {
                    var loggedInUser = _context.Users.FirstOrDefault(u => u.FirebaseUuid == currentUserId);
                    if (loggedInUser == null || loggedInUser.UserRole != "Admin")
                    {
                        return Unauthorized("Not an admin.");
                    }

                    HttpContext.Session.SetString("LoggedInUser", currentUserId);
                    TempData["EmailAsTempData"] = login.Email;
                    return Ok("Logged in successfully.");
                }
                else
                {
                    // Log the unsuccessful login attempt
                    await _logger.LogUnsuccessfulLoginAttemptAsync(login.Email, "Incorrect credentials.");
                    return Unauthorized("Incorrect credentials, please login with your administrator credentials");
                }
            }
            catch (FirebaseAuthException ex)
            {
                // Handle Firebase authentication exceptions
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseErrorModel>(ex.ResponseData);
                return BadRequest(firebaseEx.error.message);
            }
        }

        // POST: api/Auth/AdminRegister
        [HttpPost("AdminRegister")]
        public async Task<IActionResult> AdminRegister([FromBody] LoginModel login, [FromBody] Models.User user)
        {
            try
            {
                // Validate email format
                if (!login.Email.Contains("@"))
                {
                    return BadRequest("Invalid email entered");
                }

                // Validate password length
                if (login.Password.Length < 8)
                {
                    return BadRequest("Password must be at least 8 characters.");
                }

                // Validate password complexity (at least 1 special character and 1 number)
                if (!HasSpecialCharacter(login.Password) || !HasNumber(login.Password))
                {
                    return BadRequest("Password must contain at least 1 special character (!@#$%^&*) and 1 number.");
                }

                // Check if the email already exists
                if (UserExists(login.Email))
                {
                    return BadRequest("Email already exists.");
                }

                await auth.CreateUserWithEmailAndPasswordAsync(login.Email, login.Password);

                var fbAuthLink = await auth.SignInWithEmailAndPasswordAsync(login.Email, login.Password);
                string currentUserId = fbAuthLink.User.LocalId;

                user.FirebaseUuid = currentUserId;
                user.Email = login.Email;
                user.UserRole = "Admin";

                //Error 401
                if (user.FirebaseUuid == null)
                {
                    return Unauthorized(new Error("Token missing!"));
                }

                //Error 400 if incomplete
                if (user.Email == null || user.UserRole == null)
                {
                    return BadRequest(new Error("Not all details have been submitted!"));
                }

                _context.Add(user);

                await _context.SaveChangesAsync();
                return Ok("Admin registered successfully.");
            }
            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseErrorModel>(ex.ResponseData);
                return BadRequest(firebaseEx.error.message);
            }
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Email == id);
        }

        //Checks if the password contains special characters
        private bool HasSpecialCharacter(string input)
        {
            var specialCharacters = new char[] { '!', '@', '#', '$', '%', '^', '&', '*' };
            return input.Any(c => specialCharacters.Contains(c));
        }

        //Checks if the password contains a number
        private bool HasNumber(string input)
        {
            return input.Any(char.IsDigit);
        }

    }
}
