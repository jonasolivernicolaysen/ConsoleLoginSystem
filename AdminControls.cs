using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleApp
{
    internal class AdminControls
    {
        public static void ViewLogs(User user)
        {
            // check if user is admin
            if (user.Role != Role.Admin)
            {
                Console.WriteLine("Access denied.");
                return;
            }
            var log = UserStorage.LoadLog();

            foreach (var item in log)
            {
                Console.WriteLine(item);
            }
        }

        public static void ViewUsers(User user)
        {
            // check if user is admin
            if (user.Role != Role.Admin)
            {
                Console.WriteLine("Access denied.");
                return;
            }
            var users = UserStorage.LoadUsers();

            foreach (User u in users)
            {
                Console.WriteLine($"{u.Role}, {u.UserName}, {u.JoinDate}");
            }
        }
        public static void ViewUserDetails(User adminUser, string userId)
        {
            // check if user is admin
            if (adminUser.Role != Role.Admin)
            {
                Console.WriteLine("Access denied.");
                return;
            }
            var users = UserStorage.LoadUsers();
            var user = users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                Console.WriteLine("Id not found");
                return;
            }
            
            var log = UserStorage.LoadLog();
            // make a new list of the actions of the chosen user
            List<string> userActions = new List<string>();
            var lastLogin = "This user has never logged in.";

            var IdRegex = new Regex(@"Id=([a-fA-F0-9\-]+)");
            var actionRegex = new Regex(@"Action=([A-Za-z]+)");
            
            foreach (var line in log)
            {
                var matchId = IdRegex.Match(line);
                string matchedId = matchId.Success ? matchId.Groups[1].Value : "";

                var matchAction = actionRegex.Match(line);
                string matchedAction = matchAction.Success ? matchAction.Groups[1].Value : "";

                var index = line.IndexOf("|");
                var timestamp = index >= 0 ? line.Substring(0, index).Trim() : "Unknown time.";

                if (userId == matchedId)
                {
                    userActions.Add($"Timestamp: {timestamp}, action: {matchedAction}");
                    if (matchedAction == "Login")
                    {
                        lastLogin = timestamp;
                    }
                }

            }
            // get last ten actions of this user
            List<string> lastTenActions = userActions.TakeLast(10).ToList();

            Console.WriteLine($"\nRole: {user.Role}, Name: {user.UserName}");
            Console.WriteLine($"Last login: {lastLogin}\n");
            Console.WriteLine("Last 10 actions");
            foreach (var action in lastTenActions)
            {
                Console.WriteLine(action);
            }
        }

        
    }
}