using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthorizeNETPOC.ViewModels
{
    public class BankAccount
    {
        public string AccountNumber { get; set; }
        public string RoutingNumber { get; set; }
        public string NameOnAccount { get; set; }
        public string BankName { get; set; }
    }
}
