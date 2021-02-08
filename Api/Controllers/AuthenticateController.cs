using Api.Models;

using Authentication.Exceptions;
using Authentication.Jobs;
using Authentication.Services;

using Hangfire;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthenticateController : Controller
    {
        #region Deps

        private AuthenticateService _authenticateService;
        private IBackgroundJobClient _backgroundJobClient;

        #endregion

        #region Constructors

        public AuthenticateController(AuthenticateService authenticateService, IBackgroundJobClient backgroundJobClient)
        {
            _authenticateService = authenticateService;
            _backgroundJobClient = backgroundJobClient;
        }

        #endregion

        [HttpPost]
        [Route("SignIn")]
        public async Task<IActionResult> SignIn([FromBody] SignInModel model)
        {
            try
            {
                var token = await _authenticateService.SignInAsync(model.Email, model.Password);

                var response = new Response<dynamic> { Status = 200, Data = token };
                return Ok(response);
            }
            catch (Exception e) when (e is InvalidUserException || e is InvalidPasswordException || e is EmailNotConfirmedException)
            {
                return Ok(new Response { Status = 401, Message = e.Message });
            }
            catch
            {
                return Ok(new Response { Status = 500, Message = "Internal Server Error." });
            }
        }

        [HttpPost]
        [Route("SignUp")]
        public async Task<IActionResult> SignUp([FromBody] SignUpModel model)
        {
            try
            {
                var emailConfirmationToken = await _authenticateService.SignUpAsync(model.Email, model.Password);

                var token = new Token { Email = model.Email, Hash = emailConfirmationToken };
                var tokenJson = JsonConvert.SerializeObject(token);
                var tokenJsonBytes = Encoding.UTF8.GetBytes(tokenJson);
                var tokenJsonBase64 = Base64UrlTextEncoder.Encode(tokenJsonBytes);

                var emailConfirmationLink = $"{model.RedirectUrl}?token={tokenJsonBase64}";
                _backgroundJobClient.Enqueue<SendEmailJob>(x =>
                    x.SendConfirmationLinkEmail(model.Email, model.Fullname, emailConfirmationLink)
                );

                var response = new Response { Status = 200, Message = "A confirmation email was sent to your email address." };
                return Ok(response);
            }
            catch (UserSignUpException e)
            {
                return Ok(new Response { Status = 403, Message = e.Error.Description });
            }
            catch
            {
                return Ok(new Response { Status = 500, Message = "Internal Server Error." });
            }
        }

        [HttpPost]
        [Route("SendResetPasswordLink")]
        public async Task<IActionResult> SendResetPasswordLink([FromBody] ResetPasswordLinkModel model)
        {
            try
            {
                var resetPasswordToken = await _authenticateService.CreateResetPasswordTokenAsync(model.Email);

                var token = new Token { Email = model.Email, Hash = resetPasswordToken };
                var tokenJson = JsonConvert.SerializeObject(token);
                var tokenJsonBytes = Encoding.UTF8.GetBytes(tokenJson);
                var tokenJsonBase64 = Base64UrlTextEncoder.Encode(tokenJsonBytes);

                var resetPasswordLink = $"{model.RedirectUrl}?token={tokenJsonBase64}";
                _backgroundJobClient.Enqueue<SendEmailJob>(x => x.SendResetPasswordLinkEmail(model.Email, resetPasswordLink));

                var response = new Response { Status = 200, Message = "Account password reset link was sent to you email address." };
                return Ok(response);
            }
            catch (InvalidUserException e)
            {
                return Ok(new Response { Status = 403, Message = e.Message });
            }
            catch
            {
                return Ok(new Response { Status = 500, Message = "Internal Server Error." });
            }
        }

        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            try
            {
                var dataJsonBytes = Base64UrlTextEncoder.Decode(model.Token);
                var dataJson = Encoding.UTF8.GetString(dataJsonBytes);
                var data = JsonConvert.DeserializeObject<Token>(dataJson);

                await _authenticateService.ResetPasswordAsync(data.Email, model.Password, data.Hash);

                var response = new Response { Status = 200, Message = "Password reset successful" };
                return Ok(response);
            }
            catch (Exception e) when (e is InvalidUserException || e is InvalidTokenException)
            {
                return Ok(new Response { Status = 403, Message = e.Message });
            }
            catch
            {
                return Ok(new Response { Status = 500, Message = "Internal Server Error." });
            }
        }

        [HttpPost]
        [Route("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailModel model)
        {
            try
            {
                var dataJsonBytes = Base64UrlTextEncoder.Decode(model.Token);
                var dataJson = Encoding.UTF8.GetString(dataJsonBytes);
                var data = JsonConvert.DeserializeObject<Token>(dataJson);

                await _authenticateService.ConfirmEmail(data.Email, data.Hash);

                var response = new Response { Status = 200 };
                return Ok(response);
            }
            catch (Exception e) when (e is InvalidUserException || e is InvalidTokenException)
            {
                return Ok(new Response { Status = 403, Message = e.Message });
            }
            catch
            {
                return Ok(new Response { Status = 500, Message = "Internal Server Error." });
            }
        }
    }
}
