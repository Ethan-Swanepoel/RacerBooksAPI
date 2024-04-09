using RacerBooksAPI.Interfaces;
using RacerBooksAPI.Models;
using Firebase.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace RacerBooksAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        RacerbooksContext _context = new RacerbooksContext();
        private readonly IUnsuccessfulLoginLogger _logger;
        FirebaseAuthProvider auth;

        public UsersController(RacerbooksContext context, IUnsuccessfulLoginLogger logger)
        {
            _context = context;
            _logger = logger;
            auth = new FirebaseAuthProvider(new FirebaseConfig(Environment.GetEnvironmentVariable("FirebaseEBBooks")));
        }




        // POST: api/Users/UserRegister
        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser([FromBody] LoginModel login, [FromBody] Models.User user)
        {
            try
            {
                // Validate email format
                if (!login.Email.Contains("@"))
                {
                    return BadRequest(new { message = "Invalid email entered" });
                }

                // Validate password length
                if (login.Password.Length < 8)
                {
                    return BadRequest(new { message = "Password must be at least 8 characters long" });
                }

                // Validate password complexity (at least 1 special character and 1 number)
                if (!HasSpecialCharacter(login.Password) || !HasNumber(login.Password))
                {
                    return BadRequest(new { message = "Password must contain at least 1 special character (!@#$%^&*) and 1 number." });
                }

                // Check if the email already exists
                if (UserExists(login.Email))
                {
                    return BadRequest(new { message = "Email already exists." });
                }

                await auth.CreateUserWithEmailAndPasswordAsync(login.Email, login.Password);

                var fbAuthLink = await auth.SignInWithEmailAndPasswordAsync(login.Email, login.Password);
                string currentUserId = fbAuthLink.User.LocalId;

                user.FirebaseUuid = currentUserId;
                user.Email = login.Email;
                user.UserRole = "Customer";

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

                //Status 200
                return Ok("User registered successfully.");

                }
            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseErrorModel>(ex.ResponseData);
                ModelState.AddModelError(String.Empty, firebaseEx.error.message);
                return BadRequest(firebaseEx.error.message);
            }
        }





        // POST: api/User/UserLogin
        [HttpPost("UserLogin")]
        public async Task<IActionResult> UserLogin([FromBody] LoginModel login)
        {
            try
            {
                var fbAuthLink = await auth.SignInWithEmailAndPasswordAsync(login.Email, login.Password);
                string currentUserId = fbAuthLink.User.LocalId;

                //Error 401
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
                    HttpContext.Session.SetString("LoggedInUser", currentUserId);
                    TempData["EmailAsTempData"] = login.Email;

                    //Success 200
                    return Ok("Logged in successfully.");

                }
                else
                {
                    // Log the unsuccessful login attempt
                    await _logger.LogUnsuccessfulLoginAttemptAsync(login.Email, "Incorrect credentials.");
                    return Unauthorized("Incorrect credentials, please register a user");
                }
            }
            catch (FirebaseAuthException ex)
            {
                // Handle Firebase authentication exceptions
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseErrorModel>(ex.ResponseData);
                return BadRequest(firebaseEx.error.message);

            }

        }



        // POST: api/Login/Logout
        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok("Logged out successfully.");
        }



        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Email == id);
        }

        //Extra methods---------------------------------------------------------------------------------

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
