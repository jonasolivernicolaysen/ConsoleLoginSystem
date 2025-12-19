using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ConsoleApp
{
    internal class UserStorage
    {
        public static List<User> LoadUsers()
        {
            var baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var folderPath = Path.Combine(baseFolder, "ConsoleApp");
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

        public static List<string> LoadLog()
        {
            var baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var folderPath = Path.Combine(baseFolder, "ConsoleApp");
            Directory.CreateDirectory(folderPath);
            var filePath = Path.Combine(folderPath, "log.txt");

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "");
                return new List<string>();
            }
            return File.ReadAllLines(filePath).ToList();

        }

        public static void AddUserToJSON(string Id, Role role, string username, string password, List<User> users)
        {
            var user = new User()
            {
                Id = Id,
                Role = role,
                UserName = username,
                Password = PasswordHasher.ToPBKDF2(password),
                JoinDate = DateTime.UtcNow.ToString("O"),
            };
            users.Add(user);
            UserStorage.SaveUsersAsJSON(users);
        }

        public static void SaveUsersAsJSON(List<User> users)
        {
            string userAsJSON = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            var baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var folderPath = Path.Combine(baseFolder, "ConsoleApp");
            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, "users.txt");
            File.WriteAllText(filePath, userAsJSON + Environment.NewLine);
        }

        public enum Actions
        {
            Register,
            Login,
            LogOut,
            ChangeUsername,
            ChangePassword,
            DeleteAccount
        }
    
        public static void LogAction(string id, Role role, string username, Actions action, string? extra = null)
        {
            var baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var folderPath = Path.Combine(baseFolder, "ConsoleApp");
            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, "log.txt");
            string timestamp = DateTime.UtcNow.ToString("O");
            File.AppendAllText(filePath, $"{timestamp} | Id={id}, Role={role} | Username={username} | Action={action} {(extra is null ? "" : "| " + extra)}{Environment.NewLine}");
        }


        public static string ChangeUsername(User user)
        {
            var users = UserStorage.LoadUsers();
            Console.WriteLine("\nRepeat your password here, CTRL + C to quit");
            var repeatedPassword = AuthService.ReadUserInput(isPassword: true);
            
            if (!PasswordHasher.VerifyPassword(repeatedPassword, user.Password))
            {
                AuthService.DisplayMessage("\nPassword does not match!");
                return "";
            }
            Console.WriteLine("\nWrite your new username here: ");
            var newUsername = AuthService.ReadUserInput();
            var validationResult = AuthService.checkUserName(newUsername, checkForDuplicates: false);
            if (validationResult != "This username is valid!")
            {
                AuthService.DisplayMessage(validationResult);
                return "";
            }
            var usernameIsTaken = AuthService.CheckIfUserNameExists(newUsername, users);
            if (usernameIsTaken)
            {
                AuthService.DisplayMessage("\nUsername not available.");
                return "";
            }
            var storedUser = users.FirstOrDefault(u => u.UserName == user.UserName);
            if (storedUser == null)
            {
                AuthService.DisplayMessage("User not found");
                return "";
            }

            var oldUsername = user.UserName;
            storedUser.UserName = newUsername;

            UserStorage.SaveUsersAsJSON(users);
            UserStorage.LogAction(user.Id, user.Role, oldUsername, Actions.ChangeUsername, extra: $"NewUsername={newUsername}");
            AuthService.DisplayMessage("\nUsername successfully changed!", success: true);
            return newUsername;
        }


        public static void ChangeUserPassword(User user)
        {
            var users = UserStorage.LoadUsers();
            Console.WriteLine("\nRepeat your password here, CTRL + C to quit");
            var repeatedPassword = AuthService.ReadUserInput(isPassword: true);
            if (!PasswordHasher.VerifyPassword(repeatedPassword, user.Password))
            {
                AuthService.DisplayMessage("\nPassword does not match!");
                return;
            }
           
            Console.WriteLine("\nWrite your new password here: ");
            var newPassword = AuthService.ReadUserInput(isPassword: true);
            var isNewPasswordValid = AuthService.checkPassword(newPassword);
            if (isNewPasswordValid != "This password is valid!")
            {
                AuthService.DisplayMessage("\n\n" + isNewPasswordValid);
                return;
            }
            if (PasswordHasher.VerifyPassword(newPassword, user.Password))
            {
                AuthService.DisplayMessage("\n\nNew password cannot be identical to the old one!");
                return;
            }

            var storedUser = users.FirstOrDefault(u => u.UserName == user.UserName);
            if (storedUser == null)
            {
                AuthService.DisplayMessage("User not found");
                return;
            }
            storedUser.Password = PasswordHasher.ToPBKDF2(newPassword);
            user.Password = storedUser.Password;
            UserStorage.SaveUsersAsJSON(users);
            UserStorage.LogAction(user.Id, user.Role, user.UserName, Actions.ChangePassword);
            AuthService.DisplayMessage("\nPassword successfully changed!", success: true);
        }


        public static bool DeleteAccount(User user)
        {
            var username = user.UserName;
            Console.WriteLine("Confirm your password to delete your account:");
            var loggedInUserProvidedPassword = AuthService.ReadUserInput(isPassword: true);
            var users = UserStorage.LoadUsers();

            var u = users.FirstOrDefault(u => u.UserName == username);
            if (u == null)
            {
                AuthService.DisplayMessage("\nUser doesn't exist.");
                return false;
            }
            else if (username == u.UserName && !PasswordHasher.VerifyPassword(loggedInUserProvidedPassword, u.Password))
            {
                AuthService.DisplayMessage("\nPassword does not match.");
                return false;

            }
            else if (username == u.UserName && PasswordHasher.VerifyPassword(loggedInUserProvidedPassword, u.Password))
            {
                users.Remove(u);
                UserStorage.SaveUsersAsJSON(users);
                UserStorage.LogAction(user.Id, user.Role, user.UserName, Actions.DeleteAccount);
                AuthService.DisplayMessage($"\nSucessfully deleted user: {u.UserName}", success: true);
                return true;
            }
            else
            {
                AuthService.DisplayMessage("\nAn unexpected error occured.");
                return false;
            }
        }
    }
}