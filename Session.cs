using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleLoginSystem
{
    public static class Session
    {
        public static User? CurrentUser { get; set; }
    }
}
