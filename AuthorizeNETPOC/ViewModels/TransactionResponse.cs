using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthorizeNETPOC.ViewModels
{
    public class TransactionResponse
    {
        public string Note { get; set; } //message returned from PaymentGateway
        public string AuthorizationCode { get; set; } //auth code 
        public string CardType { get; set; } //Visa or Mastercard etc
        public string ReferenceNumber { get; set; }
        public string ResponseCode { get; set; } 
        public string ResponseMessage { get; set; }
        public string ServiceName { get; set; } //Name of PaymentGateway
        public string TransactionStatus { get; set; }
        public string TransactionId { get; set; }
    }
}
