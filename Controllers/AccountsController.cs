using AuthenticationPlugin;
using ImageUploader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Restfull_API_Project.Data;
using Restfull_API_Project.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Restfull_API_Project.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private CWheelsDbContext _cWheelsDbContext;
        private IConfiguration _configuration;
        private readonly AuthService _auth;
        public AccountsController(IConfiguration configuration, CWheelsDbContext cWheelsDbContext)
        {
            _cWheelsDbContext = cWheelsDbContext;
            _configuration = configuration;
            _auth = new AuthService(_configuration);
        }



        /// <summary>
        /// POST - Register a new user
        /// </summary>
        /// <param name="user"></param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        public IActionResult Register([FromBody] User user)
        {
            var userWithSameEmail = _cWheelsDbContext.Users.Where(u => u.Email == user.Email).SingleOrDefault();
            if (userWithSameEmail != null)
            {
                return BadRequest("User with same email already exist");
            }

            var newUser = new User()
            {
                Name = user.Name,
                Email = user.Email,
                Password = SecurePasswordHasherHelper.Hash(user.Password),
            };

            _cWheelsDbContext.Users.Add(newUser);
            _cWheelsDbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);
        }



        /// <summary>
        /// POST - Logs in a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns>IActionResult ObjectResult(new
        /// {
        ///        access_token = token.AccessToken,
        ///        expires_in = token.ExpiresIn,
        ///     token_type = token.TokenType,
        ///        creation_Time = token.ValidFrom,
        ///     expiration_Time = token.ValidTo,
        ///        user_id = userInDb.Id
        ///});
        ///</returns>
        ///
        [HttpPost]
        public IActionResult Login([FromBody] User user)
        {
            var userInDb = _cWheelsDbContext.Users.FirstOrDefault(u => u.Email == user.Email);
            if (userInDb == null)
            {
                return NotFound();
            }

            if (!SecurePasswordHasherHelper.Verify(user.Password, userInDb.Password))
            {
                return Unauthorized();
            }

            var claims = new[]
            {
               new Claim(JwtRegisteredClaimNames.Email, user.Email),
               new Claim(ClaimTypes.Email, user.Email),
            };

            var token = _auth.GenerateAccessToken(claims);
            return new ObjectResult(new
            {
                access_token = token.AccessToken,
                expires_in = token.ExpiresIn,
                token_type = token.TokenType,
                creation_Time = token.ValidFrom,
                expiration_Time = token.ValidTo,
                user_id = userInDb.Id
            });
        }

        /// <summary>
        /// POST - post a new password for the current user
        /// </summary>
        /// <param name="changePasswordModel"></param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        [Authorize]
        public IActionResult ChangePassword([FromBody] ChangePasswordModel changePasswordModel)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var user = _cWheelsDbContext.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound();
            }

            if (!SecurePasswordHasherHelper.Verify(changePasswordModel.OldPassword, user.Password))
            {
                return Unauthorized("Sorry you can't change the password");
            }

            user.Password = SecurePasswordHasherHelper.Hash(changePasswordModel.NewPassword);
            _cWheelsDbContext.SaveChanges();
            return Ok("Your password has been changed");

        }

        /// <summary>
        /// POST - Changes the phone number of the current user
        /// </summary>
        /// <param name="changePhoneNumber"></param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        [Authorize]
        public IActionResult ChangePhoneNumber([FromBody] ChangePhoneNumber changePhoneNumber)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var user = _cWheelsDbContext.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound();
            };

            user.Phone = changePhoneNumber.PhoneNumber;
            _cWheelsDbContext.SaveChanges();
            return Ok("Your phone number has been updated");

        }

        /// <summary>
        /// POST - post a new profile picture for the current user
        /// </summary>
        /// <param name="ImageArray"></param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        [Authorize]
        public IActionResult EditUserProfileImage([FromBody] byte[] ImageArray)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var user = _cWheelsDbContext.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound();
            }
            var stream = new MemoryStream(ImageArray);
            var guid = Guid.NewGuid().ToString();
            var file = $"{guid}.jpg";
            var folder = "wwwroot/Images/Users_img";
            var response = FilesHelper.UploadImage(stream, folder, file);
            if (!response)
            {
                return BadRequest();
            }
            else
            {
                user.ImageUrl = file;
                _cWheelsDbContext.SaveChanges();
                return StatusCode(StatusCodes.Status201Created);
            }
        }

        /// <summary>
        /// DEL- delets a user from the system 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>IActionResult</returns>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {

            var user = _cWheelsDbContext.Users.Find(id);
            if (user == null)
            {
                return NotFound("Record with id: " + id + " was not found in the database");
            }
            else
            {
                _cWheelsDbContext.Users.Remove(user);
                _cWheelsDbContext.SaveChanges();
                return Ok("Record with id: " + id + " was deleted from the system");
            }
        }
       
    }
}
