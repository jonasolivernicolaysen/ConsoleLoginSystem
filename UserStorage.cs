using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ConsoleApp
{
    internal class UserStorage
    {
        public static List<User> LoadUsers()
        {
            var baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var folderPath = Path.Combine(baseFolder, "users");
            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, "users.txt");

            if (!File.Exists(filePath))
            {
                return new List<User>();
            }
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<User>>(json)!;
        }

        public static void SaveUsersAsJSON(List<User> users)
        {
            string userAsJSON = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            var baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var folderPath = Path.Combine(baseFolder, "users");
            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, "users.txt");
            File.WriteAllText(filePath, userAsJSON + Environment.NewLine);
        }
    }
}
