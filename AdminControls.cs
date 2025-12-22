using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static ConsoleApp.UserStorage;

namespace ConsoleApp
{
    internal class AdminControls
    {
        public static void ViewLogs()
        {
            if (!AuthService.RequireAdmin(Session.CurrentUser))
            {
                return;
            }
            var log = UserStorage.LoadLog();

            foreach (var item in log)
            {
                Console.WriteLine(item);
            }
            var currentUser = Session.CurrentUser!;
            UserStorage.LogAction(currentUser.Id, currentUser.Role, currentUser.UserName, Actions.ViewLogs);
        }

        public static void ViewUsers()
        {
            if (!AuthService.RequireAdmin(Session.CurrentUser))
            {
                return;
            }
            var users = UserStorage.LoadUsers();

            foreach (User u in users)
            {
                Console.WriteLine($"{u.Role}, {u.UserName}, {u.JoinDate}");
            }
            var currentUser = Session.CurrentUser!;
            UserStorage.LogAction(currentUser.Id, currentUser.Role, currentUser.UserName, Actions.ViewAllUsers);
        }

        public static void ViewUserDetails(string userId)
        {
            if (!AuthService.RequireAdmin(Session.CurrentUser))
            {
                return;
            }
            var users = UserStorage.LoadUsers();
            var user = users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                AuthService.DisplayMessage($"User not found.");
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
            var currentUser = Session.CurrentUser!;
            UserStorage.LogAction(currentUser.Id, currentUser.Role, currentUser.UserName, Actions.InspectUser, extra: $"InspectedUser={user.UserName}");
        }

        public static bool DeleteUser(string userId)
        {
            if (!AuthService.RequireAdmin(Session.CurrentUser))
            {
                return false;
            }
            var users = UserStorage.LoadUsers();
            var user = users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                AuthService.DisplayMessage($"Id not found.");
                return false;
            }

            users.Remove(user);
            UserStorage.SaveUsersAsJSON(users);
            var currentUser = Session.CurrentUser!;
            UserStorage.LogAction(currentUser.Id, currentUser.Role, currentUser.UserName, Actions.DeleteUser, extra: $"DeletedUser={user.UserName}");
            AuthService.DisplayMessage($"\nSucessfully deleted user: {user.UserName}", success: true);
            return true;
        }   
        
        public static void ChangeUserRole(string userId, string roleInput)
        {
            if (!AuthService.RequireAdmin(Session.CurrentUser))
            {
                return;
            }
            if (!Enum.TryParse<Role>(roleInput, ignoreCase: true, out var newRole))
            {
                AuthService.DisplayMessage($"Role input invalid.");
            }

            var users = UserStorage.LoadUsers();
            var user = users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                AuthService.DisplayMessage($"User not found.");
                return;
            }

            else if (user.Role == newRole)
            {
                AuthService.DisplayMessage($"New role cannot be identical to old role.");
                return;
            }
            var oldRole = user.Role;
            user.Role = newRole;
            UserStorage.SaveUsersAsJSON(users);
            var currentUser = Session.CurrentUser!;
            UserStorage.LogAction(currentUser.Id, currentUser.Role, currentUser.UserName, Actions.ChangeUserRole, extra: $"ChangedRoleOf={user.UserName}(from: {oldRole} to: {newRole})");
            AuthService.DisplayMessage($"Successfully changed role of user {user.UserName} to {user.Role}!", success: true);
            return;
        }

        public static void ChangeUserPassword(string userId, string newPassword)
        {
            if (!AuthService.RequireAdmin(Session.CurrentUser))
            {
                return;
            }
            var users = UserStorage.LoadUsers();
            var user = users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                AuthService.DisplayMessage($"User not found.");
                return;
            }
            // new password cannot be identical to old password
            if (PasswordHasher.VerifyPassword(newPassword, user.Password))
            {
                AuthService.DisplayMessage("\nNew password cannot be identical to old one.");
                return;
            }
            user.Password = PasswordHasher.ToPBKDF2(newPassword);
            UserStorage.SaveUsersAsJSON(users);
            var currentUser = Session.CurrentUser!;
            UserStorage.LogAction(currentUser.Id, currentUser.Role, currentUser.UserName, Actions.ChangeUserPassword, extra: $"ChangedPasswordForUser={user.UserName}");
            AuthService.DisplayMessage($"\nSuccessfully changed the password of user {user.UserName}", success: true);
            return;
        }
    public static void ForcePasswordChange(string userId)
        {
            if (!AuthService.RequireAdmin(Session.CurrentUser))
            {
                return;
            }
            var users = UserStorage.LoadUsers();
            var user = users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                AuthService.DisplayMessage($"User not found.");
                return;
            }

            string temporaryPassword = PasswordHasher.GenerateRandomPassword();
            user.Password = PasswordHasher.ToPBKDF2(temporaryPassword);

            Console.WriteLine("Temporary password:");
            Console.WriteLine(temporaryPassword);

            user.MustChangePassword = true;
            UserStorage.SaveUsersAsJSON(users);
            var currentUser = Session.CurrentUser!;
            UserStorage.LogAction(currentUser.Id, currentUser.Role, currentUser.UserName, Actions.ForcePasswordChange, extra: $"ForcedPasswordFor={user.UserName}");
            AuthService.DisplayMessage($"\nSuccessfully forced password change on {user.UserName}", success: true);
        }
    }
}