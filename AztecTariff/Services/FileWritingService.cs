using AztecTariff.Models;
using System.Xml.Serialization;

namespace AztecTariff.Services
{
    public static class FileWritingService
    {
        public static T LoadFromXml<T>(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (FileStream fileStream = new FileStream(filename, FileMode.Open))
            {
                return (T)serializer.Deserialize(fileStream);
            }
        }
    }
}
