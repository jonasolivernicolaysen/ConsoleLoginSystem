using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp
{
    internal class User
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    

    public static string CreateUserName()
        {
            while (true)
            {
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Write your username here, 'q' to go back, CTRL + C to quit");
                var username = AuthService.ReadUserInput();
                if (username == "q")
                {
                    return String.Empty;
                }
                var result = AuthService.checkUserName(username);
                bool isValid = result == "This username is valid";
                if (isValid)
                {
                    return username;
                }
                Console.ForegroundColor = isValid ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine(isValid ? "\nUsername is valid" : result);
            }
        }

        public static string CreatePassword()
        {
            while (true)
            {
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Write your password here, 'q' to go back, CTRL + C to quit");
                var password = AuthService.ReadUserInput(isPassword: true);
                if (password == "q")
                {
                    return String.Empty;
                }
                var result = AuthService.checkPassword(password);
                bool isValid = result == "This password is valid";
                if (isValid)
                {
                    return password;
                }
                Console.ForegroundColor = isValid ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine(isValid ? "\nPassword is valid" : result);
            }
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
