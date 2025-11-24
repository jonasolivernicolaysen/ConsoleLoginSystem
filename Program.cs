using System.Text.Json;
using ConsoleApp;
using System.Threading;

List<User> users = UserStorage.LoadUsers();

while (true)
{
    Console.WriteLine("\nChoose an option:");
    Console.WriteLine("1) Register");
    Console.WriteLine("2) Login");
    Console.WriteLine("3) Exit");
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
        var loginUsers = UserStorage.LoadUsers();
        var remainingAttempts = 3;
        bool shouldLogOut = false;
        var currentSleepTime = 5000;

        Console.WriteLine("\nWrite your username here, 'q' to go back, CTRL + C to quit");
        var username = AuthService.ReadUserInput();
        if (username == "q")
        {
            // continue instead of break so it doesnt break out of main loop
            continue;
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
                    Console.WriteLine($"Too many failed attempts. Please wait {currentSleepTime/1000} seconds...");
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
                        shouldLogOut=true;
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

