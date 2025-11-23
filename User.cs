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
                Console.WriteLine("Write your username here, CTRL + C to quit");
                var username = AuthService.ReadUserInput();
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
                Console.WriteLine("Write your password here, CTRL + C to quit");
                var password = AuthService.ReadUserInput(isPassword: true);
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
    }

}
