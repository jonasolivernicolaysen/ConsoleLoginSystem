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


        public static void AddUserToJSON(string username, string password, List<User> users)
        {
            var user = new User()
            {
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
    
        public static void LogAction(string username, Actions action, string? extra = null)
        {
            var baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var folderPath = Path.Combine(baseFolder, "ConsoleApp");
            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, "log.txt");
            string timestamp = DateTime.UtcNow.ToString("O");
            File.AppendAllText(filePath, $"{timestamp} | User={username} | Action={action} {(extra is null ? "" : "| " + extra)}{Environment.NewLine}");
        }


        public static string ChangeUsername(string oldUsername)
        {
            var users = UserStorage.LoadUsers();
            Console.WriteLine("\nRepeat your password here, CTRL + C to quit");
            var repeatedPassword = AuthService.ReadUserInput(isPassword: true);
            var isPasswordCorrect = AuthService.CheckIfUserExists(oldUsername, repeatedPassword, users);
            if (!isPasswordCorrect)
            {
                AuthService.DisplayMessage("\nPassword does not match!");
                return "";
            }
            else
            {
                Console.WriteLine("\nWrite your new username here: ");
                var newUsername = AuthService.ReadUserInput();
                var isUsernameValid = AuthService.checkUserName(newUsername, checkForDuplicates: false);
                if (isUsernameValid != "This username is valid!")
                {
                    AuthService.DisplayMessage(isUsernameValid);
                    return "";
                }
                var usernameIsTaken = AuthService.CheckIfUserNameExists(newUsername, users);
                if (usernameIsTaken)
                {
                    AuthService.DisplayMessage("\nUsername not available.");
                    return "";
                }
                foreach (User user in users)
                {
                    if (user.UserName == oldUsername)
                    {
                        user.UserName = newUsername;
                        UserStorage.SaveUsersAsJSON(users);
                        UserStorage.LogAction(oldUsername, Actions.ChangeUsername, $"New username={newUsername}");
                        AuthService.DisplayMessage("\nUsername successfully changed!", success: true);
                        return newUsername;
                    }
                    else
                    {
                        continue;
                    }
                }
                AuthService.DisplayMessage("User not found");
                return "";
            }
        }


        public static void ChangeUserPassword(string username)
        {
            var users = UserStorage.LoadUsers();
            Console.WriteLine("\nRepeat your password here, CTRL + C to quit");
            var repeatedPassword = AuthService.ReadUserInput(isPassword: true);
            var isPasswordCorrect = AuthService.CheckIfUserExists(username, repeatedPassword, users);
            if (!isPasswordCorrect)
            {
                AuthService.DisplayMessage("\n\nPassword does not match!");
            }
            else
            {
                Console.WriteLine("\nWrite your new password here: ");
                var newPassword = AuthService.ReadUserInput(isPassword: true);
                var isNewPasswordValid = AuthService.checkPassword(newPassword);
                if (isNewPasswordValid != "This password is valid!")
                {
                    AuthService.DisplayMessage("\n\n" + isNewPasswordValid);
                    return;
                }
                var isNewPasswordIdentical = AuthService.CheckIfUserExists(username, newPassword, users);
                if (isNewPasswordIdentical)
                {
                    AuthService.DisplayMessage("\n\nNew password cannot be identical to the old one!");
                    return;
                }
                users = UserStorage.LoadUsers();
                foreach (User user in users)
                {
                    if (user.UserName == username)
                    {
                        user.Password = PasswordHasher.ToPBKDF2(newPassword);
                        UserStorage.SaveUsersAsJSON(users);
                        UserStorage.LogAction(username, Actions.ChangePassword);
                        AuthService.DisplayMessage("\nPassword successfully changed!", success: true);
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }


        public static bool DeleteAccount(string username)
        {
            Console.WriteLine("Confirm your password to delete your account:");
            var loggedInUserProvidedPassword = AuthService.ReadUserInput(isPassword: true);
            var users = UserStorage.LoadUsers();

            var user = users.FirstOrDefault(user => user.UserName == username);
            if (user == null)
            {
                AuthService.DisplayMessage("\nUser doesn't exist.");
                return false;
            }
            else if (username == user.UserName && !PasswordHasher.VerifyPassword(loggedInUserProvidedPassword, user.Password))
            {
                AuthService.DisplayMessage("\nPassword does not match.");
                return false;

            }
            else if (username == user.UserName && PasswordHasher.VerifyPassword(loggedInUserProvidedPassword, user.Password))
            {
                users.Remove(user);
                UserStorage.SaveUsersAsJSON(users);
                UserStorage.LogAction(username, Actions.DeleteAccount);
                AuthService.DisplayMessage($"\nSucessfully deleted user: {user.UserName}", success: true);
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