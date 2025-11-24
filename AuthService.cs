using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp
{
    internal class AuthService
    {
        public static string CheckIfUserExists(string username, string password, List<User> users)
        {
            User? user = users.FirstOrDefault(u => u.UserName == username && u.Password == PasswordHasher.ToSHA256(password));
            return user != null ? "Login successful" : "Invalid credentials";
        }

        public static string CheckIfUserNameExists(string username, List<User> users)
        {
            User? user = users.FirstOrDefault(u => u.UserName == username);
            return user != null ? "Username exists" : "Username doesn't exist";
        }

        public static string checkUserName(string username)
        {
            List<string> errors = new();
            var usernames = UserStorage.LoadUsers();

            if (string.IsNullOrWhiteSpace(username))
            {
                return "Username can't be empty";
            }
            if (AuthService.CheckIfUserNameExists(username, usernames) == "Username exists")
            {
                return "\nUsername already exists";
            }
            if (username.Length < 4 || username.Length > 16)
            {
                return "\nUsername length must be between 4 and 16 characters";
            }
            return "This username is valid";
        }

        public static string checkPassword(string password)
        {
            List<string> errors = new();

            if (string.IsNullOrWhiteSpace(password))
            {
                return "Password can't be empty";
            }
            if (password.Length < 4 || password.Length > 16)
            {
                errors.Add("Password length must be between 4 and 16 characters");
            }
            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                errors.Add("Password must contain at least one special character");
            }
            if (!password.Any(char.IsUpper) || !password.Any(char.IsLower))
            {
                errors.Add("Password must contain at least one uppercase and one lowercase letter");
            }
            if (!password.Any(char.IsNumber))
            {
                errors.Add("Password must contain at least one number");
            }
            if (errors.Count > 0)
            {
                string listAsString = "\n" + string.Join("\n", errors);
                return listAsString;
            }
            return "This password is valid";
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
            if (isPassword)
            {
                Console.WriteLine();
            }
            return password.Trim();
        }

        public static void Register()
        {
            // register
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
                User.AddUserToJSON(username, password, registeredUsers);
                break;
            }
        }

        public static void Login()
        {
            var loginUsers = UserStorage.LoadUsers();
            var remainingAttempts = 3;
            bool shouldLogOut = false;
            var currentSleepTime = 5000;

            Console.WriteLine("\nWrite your username here, 'q' to go back, CTRL + C to quit");
            var username = AuthService.ReadUserInput();
            if (username == "q")
            {
                return;
            }
            while (remainingAttempts > 0 && !shouldLogOut)
            {
                Console.WriteLine("\nWrite your password here, 'q' to go back, CTRL + C to quit");
                var password = AuthService.ReadUserInput(isPassword: true);
                var result = AuthService.CheckIfUserExists(username, password, loginUsers);
                if (password == "q")
                {
                    break;
                }
                Console.WriteLine("\n" + result);
                if (result != "Login successful")
                {
                    remainingAttempts--;
                    Console.WriteLine($"{remainingAttempts} attempts left");

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

                        Console.WriteLine("Choose an option:");
                        Console.WriteLine("1) Change password");
                        Console.WriteLine("2) Delete account");
                        Console.WriteLine("3) Log out");
                        Console.Write("> ");

                        var loggedInUserChoice = Console.ReadLine();

                        if (loggedInUserChoice == "1")
                        {
                            User.ChangeUserPassword(username);
                        }
                        else if (loggedInUserChoice == "2")
                        {
                            var isUserDeleted = User.DeleteAccount(username);
                            if (isUserDeleted.StartsWith("Successfully deleted user"))
                            {
                                shouldLogOut = true;
                                Console.WriteLine(isUserDeleted);
                            }
                            else
                            {
                                Console.WriteLine(isUserDeleted);
                            }

                        }
                        else if (loggedInUserChoice == "3")
                        {
                            shouldLogOut = true;
                        }
                        else
                        {
                            Console.WriteLine("Invalid choice.");
                        }
                    }
                }
        }
    }
    }
}
