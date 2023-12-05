using Telerik.SvgIcons;

namespace AztecTariff.Models
{
    public class Settings
    {
        public bool IsLoggedIn { get; set; }
        public string TemplateFolderLocation { get; set; }
        public string CSVFileLocation { get; set; }
        public string WordFileLocation { get; set; }
        public string LibreLocation { get; set; }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(TemplateFolderLocation) || string.IsNullOrWhiteSpace(LibreLocation) || string.IsNullOrWhiteSpace(WordFileLocation) || string.IsNullOrWhiteSpace(CSVFileLocation))
            {
                return false;
            } else if (!Directory.Exists(TemplateFolderLocation) || !Directory.Exists(CSVFileLocation) || !System.IO.File.Exists(WordFileLocation) || !System.IO.File.Exists(LibreLocation))
            {
                return false;
            } else
            {
                return true;
            }
        }

        public List<string> LoginDetailsValid(string username, string password)
        {
            List<string> errorList = new List<string>();
            if (string.IsNullOrWhiteSpace(username))
            {
                errorList.Add("UsernameError");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                errorList.Add("PasswordError");
            }

            return errorList;
        }
    }
}
