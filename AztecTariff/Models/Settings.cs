namespace AztecTariff.Models
{
    public class Settings
    {
        public bool IsLoggedIn { get; set; }
        public string CSVFileLocation { get; set; }
        public string APIBaseAddress { get; set; }
        public string PDFOuputFilesLocation { get; set; }
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(CSVFileLocation) ||string.IsNullOrWhiteSpace(PDFOuputFilesLocation) )
            {
                return false;
            }
            else
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
