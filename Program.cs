using System.ComponentModel.Design;
using System.Text.Json;
using System.Security.Cryptography;

    
string checkUserName(string username)
{
    List<string> errors = new();
    var usernames = LoadUsers();

    if (string.IsNullOrWhiteSpace(username))
    {
        return "Username can't be empty";
    }
    if (CheckIfUserNameExists(username, usernames) == "Username exists")
    {
        return "\nUsername already exists";
    }
    if (username.Length < 4 || username.Length > 16)
    {
        return "\nUsername length must be between 4 and 16 characters";
    }
    return "This username is valid";
}

static string checkPassword(string password)
{
    List<string> errors = new();

    if (string.IsNullOrWhiteSpace(password))
    {
        return "Password can't be empty";
    }
    if (password.Length < 4 || password.Length > 16)
    {
        errors.Add("Password length must be between 4 and 16 characters");
    }
    if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
    {
        errors.Add("Password must contain at least one special character");
    }
    if (!password.Any(char.IsUpper) || !password.Any(char.IsLower))
    {
        errors.Add("Password must contain at least one uppercase and one lowercase letter");
    }
    if (!password.Any(char.IsNumber))
    {
        errors.Add("Password must contain at least one number");
    }
    if (errors.Count > 0)
    {
        string listAsString = "\n" + string.Join("\n", errors);
        return listAsString;
    }
    return "This password is valid";
}


static string ReadUserInput(bool isPassword = false)
{
    var password = string.Empty;
    ConsoleKey key;
    do
    {
        var keyInfo = Console.ReadKey(intercept: true);
        key = keyInfo.Key;

        if (key == ConsoleKey.Backspace && password.Length > 0)
        {
            password = password[..^1];
            Console.Write("\b \b");
        }
        else if (!char.IsControl(keyInfo.KeyChar))
        {
            password += keyInfo.KeyChar;
            if (isPassword)
            {
                Console.Write("*");
            }
            else
            {
                Console.Write(keyInfo.KeyChar);
            }
        }
    } while (key != ConsoleKey.Enter);
    if (isPassword)
    {
        Console.WriteLine();
    }
    return password.Trim();
}

List<User> users = LoadUsers();

string CreateUserName()
{
    while (true)
    {
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Write your username here, CTRL + C to quit");
        var username = ReadUserInput();
        var result = checkUserName(username);
        bool isValid = result == "This username is valid";

        if (isValid)
        {
            return username;
        }
        Console.ForegroundColor = isValid ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(isValid ? "\nUsername is valid" : result);
    }
}

static string CreatePassword()
{
    while (true)
    {
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Write your password here, CTRL + C to quit");
        var password = ReadUserInput(isPassword: true);
        var result = checkPassword(password);
        bool isValid = result == "This password is valid";

        if (isValid)
        {
            return password;
        }
        Console.ForegroundColor = isValid ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(isValid ? "\nPassword is valid" : result);
    }
}

string CheckIfUserExists(string username, string password, List<User> users)
{
    User? user = users.FirstOrDefault(u => u.UserName == username && u.Password == ToSHA256(password));
    return user != null ? "Login successful" : "Invalid credentials";
}

string CheckIfUserNameExists(string username, List<User> users)
{
    User? user = users.FirstOrDefault(u => u.UserName == username);
    return user != null ? "Username exists": "Username doesn't exist";
}

static string ToSHA256(string password)
{
    using var sha256 = SHA256.Create();
    var bytes = System.Text.Encoding.UTF8.GetBytes(password);
    var hash = sha256.ComputeHash(bytes);
    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
}

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
        var username = CreateUserName();
        var password = CreatePassword();
        var user = new User()
        {
            UserName = username,
            Password = ToSHA256(password)
        };
        users.Add(user);
        SaveUsersAsJSON(users);
    }
    else if (choice == "2")
    {
        // login
        var loginUsers = LoadUsers();
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
            var username = ReadUserInput();
            Console.WriteLine("\nWrite your password here, CTRL + C to quit");
            var password = ReadUserInput(isPassword: true);
            var result = CheckIfUserExists(username, password, loginUsers);
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
                        var changePasswordUsers = LoadUsers();
                        Console.WriteLine("\nRepeat your password here, CTRL + C to quit");
                        var repeatedPassword = ReadUserInput(isPassword: true);
                        var isRepeatedPasswordCorrect = CheckIfUserExists(username, repeatedPassword, changePasswordUsers);
                        Console.WriteLine("\n" + result);
                        if (result != "Login successful")
                        {
                            Console.WriteLine("Password does not match");
                        }
                        else
                        {
                            var newPassword = ReadUserInput(isPassword: true);
                            // continue here, i must delete old json array and create new one where this user had updated the password
                            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                            
                        }
                    }
                    else if (loggedInUserChoice == "2")
                    {
                        // delete account
                        Console.WriteLine("Confirm your password to delete your account");
                        var loggedInUserProvidedPassword = ReadUserInput(isPassword: true); 
                        var JSONUsers = LoadUsers();
                        foreach (var user in JSONUsers)
                        {
                            if (username == user.UserName && ToSHA256(loggedInUserProvidedPassword) == user.Password)
                            {
                                JSONUsers.Remove(user);
                                SaveUsersAsJSON(JSONUsers);
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


static void SaveUsersAsJSON(List<User> users)
{
    string userAsJSON = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
    var baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    var folderPath = Path.Combine(baseFolder, "users");
    Directory.CreateDirectory(folderPath);  
    string filePath = Path.Combine(folderPath, "users.txt");
    File.WriteAllText(filePath, userAsJSON + Environment.NewLine);
}

static List<User> LoadUsers()
{
    var baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    var folderPath = Path.Combine(baseFolder, "users");
    Directory.CreateDirectory(folderPath);
    string filePath = Path.Combine(folderPath, "users.txt");

    if (!File.Exists(filePath))
    {
        return new List<User>();
    }
    var json = File.ReadAllText(filePath);
    return JsonSerializer.Deserialize<List<User>>(json)!;
}

class User
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
}

