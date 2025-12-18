using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
