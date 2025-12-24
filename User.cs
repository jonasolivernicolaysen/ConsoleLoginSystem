using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;


namespace ConsoleApp
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Role 
    {
        User, 
        Admin 
    }
    
    public class User
    {
        public required string Id { get; set; }
        public required Role Role { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required string JoinDate { get; set; }
        public bool MustChangePassword { get; set; }

        public static string CreateUserName()
        {
            while (true)
            {
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Write your username here, 'q' to go back, CTRL + C to quit.");
                var username = AuthService.ReadUserInput();
                if (username == "q")
                {
                    return "";
                }
                var result = AuthService.checkUserName(username, checkForDuplicates: true);
                bool isValid = result == "This username is valid!";
                if (isValid)
                {
                    return username;
                }
                AuthService.DisplayMessage(result);
            }
        }

        public static string CreatePassword()
        {
            while (true)
            {
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Write your password here, 'q' to go back, CTRL + C to quit.");
                var password = AuthService.ReadUserInput(isPassword: true);
                
                if (password == "q")
                {
                    return "";
                }
                var result = AuthService.checkPassword(password);
                bool isValid = result == "This password is valid!";
                if (!isValid)
                {
                    AuthService.DisplayMessage(result);
                    return "";
                }
                Console.WriteLine("\n\nRepeat your password here, 'q' to go back, CTRL + C to quit.");
                var password2 = AuthService.ReadUserInput(isPassword: true);


                if (password != password2)
                {
                    AuthService.DisplayMessage("\nPasswords do not match!");
                    return "";
                }
                return password;
            }
        }
    }
}
