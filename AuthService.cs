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
    }
}
