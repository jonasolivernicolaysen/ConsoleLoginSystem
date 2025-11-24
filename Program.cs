using ConsoleApp;
using System.Threading;

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
        AuthService.Register();
    }
    else if (choice == "2")
    {
        // login
        AuthService.Login();
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

