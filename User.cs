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
    
    internal class User
    {
        public required string Id { get; set; }
        public required Role Role { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required string JoinDate { get; set; }


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
                    return String.Empty;
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
                    return String.Empty;
                }
                var result = AuthService.checkPassword(password);
                bool isValid = result == "This password is valid!";
                if (isValid)
                {
                    return password;
                }
                AuthService.DisplayMessage(result);
            }
        }
    }
}
