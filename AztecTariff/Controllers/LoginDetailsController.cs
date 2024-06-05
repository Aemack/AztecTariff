using AztecTariff.Data;
using AztecTariffModels.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace AztecTariff.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginDetailsController : ControllerBase
    {
        private readonly TariffDatabaseContext _dbContext;

        public LoginDetailsController(TariffDatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("Validate/{apiKey}")]
        public bool ValidateAPIKey(string apiKey)
        {
            var foundLogin = _dbContext.LoginDetails.Where(x => x.APIKey == apiKey).FirstOrDefault();
            if (foundLogin != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [HttpGet("{id}")]
        public string GetAPIKey(int userId)
        {
            var foundLogin = _dbContext.LoginDetails.Where(x => x.UserId == userId).FirstOrDefault();
            if (foundLogin != null)
            {
                return foundLogin.APIKey;
            }
            else
            {
                return null;
            }
        }


        // POST: LoginDetailsController/Create

        [HttpPost(Name = "Create")]
        public async Task<ActionResult> Create(LoginDetails details)
        {
            var r = new Random();
            details.APIKey = GenerateApiKey();
            _dbContext.LoginDetails.Add(details);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(Create), details);
        }


        public static string GenerateApiKey()
        {
            int length = 16;
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ01234567891234567890";
            StringBuilder apiKeyBuilder = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                int randomIndex = random.Next(validChars.Length);
                apiKeyBuilder.Append(validChars[randomIndex]);
            }

            return apiKeyBuilder.ToString();
        }
    }
}
