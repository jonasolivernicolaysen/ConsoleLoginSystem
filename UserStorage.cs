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
                File.WriteAllText(filePath, "[]");
                return new List<User>();
            }


            var json = File.ReadAllText(filePath);

            if (string.IsNullOrEmpty(json))
            {
                return new List<User>();
            }
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }


        public static void AddUserToJSON(string username, string password, List<User> users)
        {
            var user = new User()
            {
                UserName = username,
                Password = PasswordHasher.ToSHA256(password)
            };
            users.Add(user);
            UserStorage.SaveUsersAsJSON(users);
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


        public static void ChangeUsername(string oldUsername)
        {
            var users = UserStorage.LoadUsers();
            Console.WriteLine("\nRepeat your password here, CTRL + C to quit");
            var repeatedPassword = AuthService.ReadUserInput(isPassword: true);
            var isPasswordCorrect = AuthService.CheckIfUserExists(oldUsername, repeatedPassword, users);
            if (isPasswordCorrect != "Login successful")
            {
                Console.WriteLine("Password does not match");
            }
            else
            {
                Console.WriteLine("Write your new username here: ");
                var newUsername = AuthService.ReadUserInput();
                var isUsernameAvailable = AuthService.CheckIfUserNameExists(newUsername, users);
                if (isUsernameAvailable == "Username exists")
                {
                    Console.WriteLine("Username already exists");
                    return;
                }

                foreach (User user in users)
                {
                    if (user.UserName == oldUsername)
                    {
                        user.UserName = newUsername;
                        UserStorage.SaveUsersAsJSON(users);
                        Console.WriteLine("\nUsername successfully changed");
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }


        public static void ChangeUserPassword(string username)
        {
            var users = UserStorage.LoadUsers();
            Console.WriteLine("\nRepeat your password here, CTRL + C to quit");
            var repeatedPassword = AuthService.ReadUserInput(isPassword: true);
            var isPasswordCorrect = AuthService.CheckIfUserExists(username, repeatedPassword, users);
            if (isPasswordCorrect != "Login successful")
            {
                Console.WriteLine("Password does not match");
            }
            else
            {
                Console.WriteLine("Write your new password here: ");
                var newPassword = AuthService.ReadUserInput(isPassword: true);
                users = UserStorage.LoadUsers();
                foreach (User user in users)
                {
                    if (user.UserName == username)
                    {
                        user.Password = PasswordHasher.ToSHA256(newPassword);
                        UserStorage.SaveUsersAsJSON(users);
                        Console.WriteLine("Password successfully changed");
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }


        public static string DeleteAccount(string username)
        {
            Console.WriteLine("Confirm your password to delete your account");
            var loggedInUserProvidedPassword = AuthService.ReadUserInput(isPassword: true);
            var users = UserStorage.LoadUsers();

            var user = users.FirstOrDefault(user => user.UserName == username);
            if (user == null)
            {
                return "User doesn't exist";
            }
            else if (username == user.UserName && PasswordHasher.ToSHA256(loggedInUserProvidedPassword) == user.Password)
            {
                users.Remove(user);
                UserStorage.SaveUsersAsJSON(users);
                return $"Successfully deleted user: {user.UserName}";
            }
            else if (username == user.UserName && PasswordHasher.ToSHA256(loggedInUserProvidedPassword) != user.Password)
            {
                return "Password does not match";
            }
            else
            {
                return "An unexpected error occured";
            }
        }
    }
}
