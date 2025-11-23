using System.Text.Json;
using ConsoleApp;

List<User> users = UserStorage.LoadUsers();

while (true)
{
    Console.WriteLine("Choose an option:");
    Console.WriteLine("1) Register");
    Console.WriteLine("2) Login");
    Console.WriteLine("3) Log Out");
    Console.WriteLine("4) Exit");
    Console.Write("> ");
        
    var choice = Console.ReadLine();

    if (choice == "1")
    {
        // register
        var username = User.CreateUserName();
        var password = User.CreatePassword();
        var user = new User()
        {
            UserName = username,
            Password = PasswordHasher.ToSHA256(password)
        };
        users.Add(user);
        UserStorage.SaveUsersAsJSON(users);
    }
    else if (choice == "2")
    {
        // login
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
            Console.WriteLine("\nWrite your username here, CTRL + C to quit");
            var username = AuthService.ReadUserInput();
            Console.WriteLine("\nWrite your password here, CTRL + C to quit");
            var password = AuthService.ReadUserInput(isPassword: true);
            var result = AuthService.CheckIfUserExists(username, password, loginUsers);
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
                        // change password
                        var changePasswordUsers = UserStorage.LoadUsers();
                        Console.WriteLine("\nRepeat your password here, CTRL + C to quit");
                        var repeatedPassword = AuthService.ReadUserInput(isPassword: true);
                        var isRepeatedPasswordCorrect = AuthService.CheckIfUserExists(username, repeatedPassword, changePasswordUsers);
                        Console.WriteLine("\n" + result);
                        if (result != "Login successful")
                        {
                            Console.WriteLine("Password does not match");
                        }
                        else
                        {
                            var newPassword = AuthService.ReadUserInput(isPassword: true);
                            // continue here, i must delete old json array and create new one where this user had updated the password
                            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                            
                        }
                    }
                    else if (loggedInUserChoice == "2")
                    {
                        // delete account
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

