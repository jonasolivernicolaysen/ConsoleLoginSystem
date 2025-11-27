using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ConsoleApp
{
    internal class AuthService
    {
        public static bool CheckIfUserExists(string username, string password, List<User> users)
        {
            var user = users.SingleOrDefault(u => u.UserName == username);
            if (user == null)
                return false;

            return PasswordHasher.VerifyPassword(password, user.Password);
        }

        public static bool CheckIfUserNameExists(string username, List<User> users)
        {
            return users.Any(u => u.UserName == username);
        }

        public static string checkUserName(string username, bool checkForDuplicates = false)
        {
            List<string> errors = new();
            var usernames = UserStorage.LoadUsers();

            if (string.IsNullOrWhiteSpace(username))
            {
                return "Username can't be empty!";
            }
            if (checkForDuplicates)
            {
                if (AuthService.CheckIfUserNameExists(username, usernames))
                {
                    return "\nUsername already exists!";
                }
            }
            if (username.Length < 4 || username.Length > 16)
            {
                return "\nUsername length must be between 4 and 16 characters!";
            }
            return "This username is valid!";
        }

        public static string checkPassword(string password)
        {
            List<string> errors = new();
            if (string.IsNullOrWhiteSpace(password))
            {
                return "Password can't be empty!";
            }
            if (password.Length < 4 || password.Length > 16)
            {
                errors.Add("Password length must be between 4 and 16 characters!");
            }
            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                errors.Add("Password must contain at least one special character!");
            }
            if (!password.Any(char.IsUpper) || !password.Any(char.IsLower))
            {
                errors.Add("Password must contain at least one uppercase and one lowercase letter!");
            }
            if (!password.Any(char.IsNumber))
            {
                errors.Add("Password must contain at least one number!");
            }
            if (errors.Count > 0)
            {
                string listAsString = "\n" + string.Join("\n", errors);
                return listAsString;
            }
            return "This password is valid!";
        }

        public static string ReadUserInput(bool isPassword = false)
        {
            var password = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password[..^1];
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    password += keyInfo.KeyChar;
                    if (isPassword)
                    {
                        Console.Write("*");
                    }
                    else
                    {
                        Console.Write(keyInfo.KeyChar);
                    }
                }
            } while (key != ConsoleKey.Enter);
            return password.Trim();
        }

        public static void DisplayMessage(string message, bool success = false)
        {
            var color = success ? ConsoleColor.Green : ConsoleColor.Red;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void Register()
        {
            while (true)
            {
                var username = User.CreateUserName();
                if (string.IsNullOrEmpty(username))
                {
                    break;
                }
                var password = User.CreatePassword();
                if (string.IsNullOrEmpty(password))
                {
                    break;
                }
                var registeredUsers = UserStorage.LoadUsers();
                UserStorage.AddUserToJSON(username, password, registeredUsers);
                UserStorage.LogAction($"{username} registered");
                AuthService.DisplayMessage($"\nUser {username} registered successfully!", success: true);
                break;
            }
        }

        public static void Login()
        {
            var loginUsers = UserStorage.LoadUsers();
            var remainingAttempts = 3;
            bool shouldLogOut = false;
            var currentSleepTime = 5000;

            Console.WriteLine("\nWrite your username here, 'q' to go back, CTRL + C to quit.");
            var username = AuthService.ReadUserInput();
            if (username == "q")
            {
                return;
            }
            while (remainingAttempts > 0 && !shouldLogOut)
            {
                Console.WriteLine("\nWrite your password here, 'q' to go back, CTRL + C to quit.");
                var password = AuthService.ReadUserInput(isPassword: true);
                var userExists = AuthService.CheckIfUserExists(username, password, loginUsers);
                if (password == "q")
                {
                    break;
                }
                if (userExists)
                {
                    AuthService.DisplayMessage("\nLogin successful\n", success: true);
                    UserStorage.LogAction($"{username} logged in");
                }
                else
                {
                    AuthService.DisplayMessage("\nInvalid credentials.\n");
                }
                    
                if (!userExists)
                {
                    remainingAttempts--;
                    Console.WriteLine($"{remainingAttempts} attempts left!");

                    if (remainingAttempts == 0)
                    {
                        Console.WriteLine($"Too many failed attempts. Please wait {currentSleepTime / 1000} seconds...");
                        Thread.Sleep(currentSleepTime);
                        currentSleepTime *= 2;
                        remainingAttempts = 3;
                    }
                }
                else
                {
                    Console.WriteLine($"Hello, {username}!");
                    while (true)
                    {
                        if (shouldLogOut)
                        {
                            break;
                        }
                        Console.WriteLine("\nChoose an option:");
                        Console.WriteLine("1) Change username");
                        Console.WriteLine("2) Change password");
                        Console.WriteLine("3) Delete account");
                        Console.WriteLine("4) Log out");
                        Console.Write("> ");

                        var loggedInUserChoice = Console.ReadLine();

                        if (loggedInUserChoice == "1")
                        {
                            var newUsername = UserStorage.ChangeUsername(username);
                            if (!string.IsNullOrEmpty(newUsername))
                            {
                            username = newUsername;
                            }
                        }
                        else if (loggedInUserChoice == "2")
                        {
                            UserStorage.ChangeUserPassword(username);
                        }
                        else if (loggedInUserChoice == "3")
                        {
                            var isUserDeleted = UserStorage.DeleteAccount(username);
                            if (isUserDeleted)
                            {
                                shouldLogOut = true;
                            }
                        }
                        else if (loggedInUserChoice == "4")
                        {
                            UserStorage.LogAction($"{username} logged out");
                            AuthService.DisplayMessage("Successfully logged out!", success: true);
                            shouldLogOut = true;
                        }
                        else
                        {
                            AuthService.DisplayMessage("Invalid choice.");
                        }
                    }
                }
            }
        }
    }
}
