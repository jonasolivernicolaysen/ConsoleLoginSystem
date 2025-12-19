using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp
{
    internal class LoginService
    {
        public static void Login()
        {
            var users = UserStorage.LoadUsers();
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
                var userExists = AuthService.CheckIfUserExists(username, password, users);
                if (password == "q")
                {
                    break;
                }
                if (!userExists)
                {
                    AuthService.DisplayMessage("\nInvalid credentials.\n");
                    remainingAttempts--;
                    Console.WriteLine($"{remainingAttempts} attempts left!");

                    if (remainingAttempts == 0)
                    {
                        AuthService.DisplayMessage($"Too many failed attempts. Please wait {currentSleepTime / 1000} seconds...");
                        Thread.Sleep(currentSleepTime);
                        currentSleepTime *= 2;
                        remainingAttempts = 3;
                    }
                }
                else
                {
                    var user = users.First(u => u.UserName == username);
                    if (user.Role == Role.User)
                    {
                        AuthService.DisplayMessage("\nLogin successful\n", success: true);
                        UserStorage.LogAction(user.Id, user.Role, user.UserName, UserStorage.Actions.Login);

                        Console.WriteLine($"Hello, {user.UserName}!");
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
                                var newUsername = UserStorage.ChangeUsername(user);
                                if (!string.IsNullOrEmpty(newUsername))
                                {
                                    user.UserName = newUsername;
                                    users = UserStorage.LoadUsers();
                                }
                            }
                            else if (loggedInUserChoice == "2")
                            {
                                UserStorage.ChangeUserPassword(user);
                                users = UserStorage.LoadUsers();
                            }
                            else if (loggedInUserChoice == "3")
                            {
                                var isUserDeleted = UserStorage.DeleteAccount(user);
                                if (isUserDeleted)
                                {
                                    shouldLogOut = true;
                                }
                            }
                            else if (loggedInUserChoice == "4")
                            {
                                UserStorage.LogAction(user.Id, user.Role, user.UserName, UserStorage.Actions.LogOut);
                                AuthService.DisplayMessage("Successfully logged out!", success: true);
                                shouldLogOut = true;
                            }
                            else
                            {
                                AuthService.DisplayMessage("Invalid choice.");
                            }
                        }
                    }
                    else if (user.Role == Role.Admin)
                    {

                        AuthService.DisplayMessage("\nLogin successful", success: true);
                        UserStorage.LogAction(user.Id, user.Role, user.UserName, UserStorage.Actions.Login);
                        while (true)
                        {
                            if (shouldLogOut)
                            {
                                break;
                            }
                            Console.WriteLine("\n\nAdmin control panel:");
                            Console.WriteLine("1) View Logs");
                            Console.WriteLine("2) View all Users");
                            Console.WriteLine("3) View user details");
                            Console.WriteLine("4) Delete User");
                            Console.WriteLine("5) Change user role");
                            Console.WriteLine("6) Change user password");
                            Console.WriteLine("7) Log out");
                            Console.Write("> ");

                            var loggedInUserChoice = Console.ReadLine();

                            if (loggedInUserChoice == "1")
                            {
                                // view logs
                                AdminControls.ViewLogs(user);
                            }
                            else if (loggedInUserChoice == "2")
                            {
                                // view all users
                                AdminControls.ViewUsers(user);
                            }
                            else if (loggedInUserChoice == "3")
                            {
                                // view user details
                                Console.WriteLine("Write the id of the user you'd like to inspect:");
                                var userId = Console.ReadLine();
                                if (string.IsNullOrEmpty(userId))
                                {
                                    AuthService.DisplayMessage("User id cannot be empty.");
                                    break;
                                }
                                else
                                {
                                    AdminControls.ViewUserDetails(user, userId);
                                }
                            }
                            else if (loggedInUserChoice == "4")
                            {
                                // delete user
                                Console.WriteLine("Write the id of the user you'd like to delete:");
                                var userId = Console.ReadLine();
                                if (string.IsNullOrEmpty(userId))
                                {
                                    AuthService.DisplayMessage("User id cannot be empty.");
                                    break;
                                }
                                else
                                {
                                    AdminControls.ViewUserDetails(user, userId);
                                }
                                Console.WriteLine($"Are you sure you'd like to delete user {userId}? (y/n)");
                                var confirmation = Console.ReadLine();
                                if (confirmation?.ToLower() != "y")
                                {
                                    AuthService.DisplayMessage("Action aborted");
                                    break;
                                }
                                AdminControls.DeleteUser(user, userId);
                            }
                            else if (loggedInUserChoice == "5")
                            {
                                // change user role
                                Console.WriteLine("Write the id of the user you'd like to change the role of:");
                                var userId = Console.ReadLine();
                                if (string.IsNullOrEmpty(userId))
                                {
                                    AuthService.DisplayMessage("User id cannot be empty.");
                                    break;
                                }
                                else
                                {
                                    AdminControls.ViewUserDetails(user, userId);
                                }

                                Console.WriteLine($"Write the new role of user {userId}:");
                                var roleInput = Console.ReadLine();
                                if (string.IsNullOrEmpty(roleInput))
                                {
                                    AuthService.DisplayMessage("Role cannot be empty.");
                                    break;
                                }

                                Console.WriteLine("Confirm (y/n)");
                                var confirmation = Console.ReadLine();
                                if (string.IsNullOrEmpty(confirmation))
                                {
                                    AuthService.DisplayMessage("Confirmation cannot be empty.");
                                    break;
                                }
                                else
                                {
                                    AdminControls.ViewUserDetails(user, userId);
                                }
                                if (confirmation.ToLower() != "y")
                                {
                                    AuthService.DisplayMessage("Action aborted.", success: true);
                                    break;
                                }
                                AdminControls.ChangeUserRole(user, userId, roleInput);
                            }
                            else if (loggedInUserChoice == "6")
                            {
                                // change user password
                                Console.WriteLine("Write the id of the user you'd like to change the password of:");
                                var userId = Console.ReadLine();
                                if (string.IsNullOrEmpty(userId))
                                {
                                    AuthService.DisplayMessage("User id cannot be empty.");
                                    break;
                                }
                                var newPassword = User.CreatePassword();
                                AdminControls.ChangeUserPassword(user, userId, newPassword);
                            }
                            else if (loggedInUserChoice == "7")
                            {
                                // log out
                                UserStorage.LogAction(user.Id, user.Role, user.UserName, UserStorage.Actions.LogOut);
                                AuthService.DisplayMessage("Successfully logged out!", success: true);
                                shouldLogOut = true;
                            }

                        }
                    }
                }
            }
        }
    }
}
