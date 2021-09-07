using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthorizeNETPOC.ViewModels
{
    public class AuthTransactionModel
    {
        public string CustomerProfileId { get; set; }
        public string CustomerPaymentProfileId { get; set; }
        public decimal Amount { get; set; }
    }
}
