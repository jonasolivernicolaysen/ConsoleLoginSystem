using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp
{
    internal class RegisterService
    {
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
                var role = registeredUsers.Count == 0 ? Role.Admin : Role.User;
                UserStorage.AddUserToJSON(role, username, password, registeredUsers);
                UserStorage.LogAction(role, username, UserStorage.Actions.Register);
                AuthService.DisplayMessage($"\nUser {username} registered successfully!", success: true);
                break;
            }
        }
    }
}
