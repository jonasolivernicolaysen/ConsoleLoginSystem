using System.Text.Json;
using ConsoleApp;

List<User> users = UserStorage.LoadUsers();

while (true)
{
    Console.WriteLine("\nChoose an option:");
    Console.WriteLine("1) Register");
    Console.WriteLine("2) Login");
    Console.WriteLine("3) Log Out");
    Console.WriteLine("4) Exit");
    Console.Write("> ");
        
    var choice = Console.ReadLine();

    if (choice == "1")
    {
        // register
        while (true)
        {
            var username = User.CreateUserName();
            if (username == String.Empty)
            {
                break;
            }
            var password = User.CreatePassword();
            if (password == String.Empty)
            {
                break;
            }
            User.AddUserToJSON(username, password, users);
            break;
        }
    }
    else if (choice == "2")
    {
        // login
        // loginuser()
        var loginUsers = UserStorage.LoadUsers();
        var remainingAttempts = 3;
        bool shouldLogOut = false;

        while (remainingAttempts > 0)
        {
            if (shouldLogOut)
            {
                break;
            }
            remainingAttempts -= 1;
            Console.WriteLine("\nWrite your username here, 'q' to go back, CTRL + C to quit");
            var username = AuthService.ReadUserInput();
            Console.WriteLine("\nWrite your password here, 'q' to go back, CTRL + C to quit");
            var password = AuthService.ReadUserInput(isPassword: true);
            var result = AuthService.CheckIfUserExists(username, password, loginUsers);
            if (username == "q" || password == "q")
            {
                break;
            }
            Console.WriteLine("\n" + result);
            if (result != "Login successful")
            {
                Console.WriteLine($"{remainingAttempts} attempts left");
            }
            else
            {
                while (true)
                {
                    if (shouldLogOut)
                    {
                        break;
                    }

                    Console.WriteLine($"Hello, {username}. Choose an option:");
                    Console.WriteLine("1) Change password");
                    Console.WriteLine("2) Delete account");
                    Console.WriteLine("3) Log out");
                    Console.Write("> ");

                    var loggedInUserChoice = Console.ReadLine();

                    if (loggedInUserChoice == "1")
                    {
                        // changeuserpassword()
                        User.ChangeUserPassword(username);
                    }
                    else if (loggedInUserChoice == "2")
                    {
                        // delete account
                        // deleteuseraccount()
                        Console.WriteLine("Confirm your password to delete your account");
                        var loggedInUserProvidedPassword = AuthService.ReadUserInput(isPassword: true); 
                        var JSONUsers = UserStorage.LoadUsers();
                        foreach (var user in JSONUsers)
                        {
                            if (username == user.UserName && PasswordHasher.ToSHA256(loggedInUserProvidedPassword) == user.Password)
                            {
                                JSONUsers.Remove(user);
                                UserStorage.SaveUsersAsJSON(JSONUsers);
                                Console.WriteLine($"Successfully deleted user: {user.UserName}");
                                shouldLogOut = true;
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Password does not match");
                            }

                        }
                    }
                    else if (loggedInUserChoice == "3")
                    {
                        // log out
                    }
                    else if (loggedInUserChoice == "4")
                    {
                        // exit
                        break;
                    }

                    else
                    {
                        Console.WriteLine("Invalid choice.");
                    }
                }
            }
        }
        
    }
    else if (choice == "3")
    {
        // exit
        break;
    }
    else
    {
        Console.WriteLine("Invalid choice.");
    }
}

