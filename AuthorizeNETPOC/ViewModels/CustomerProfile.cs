using AuthorizeNet.Api.Contracts.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthorizeNETPOC.ViewModels
{
    public class CustomerProfileModel
    {
        public CreditCard CreditCard { get; set; }
        //public BankAccount BankAccount { get; set; }
        //public HomeAddressModel CustomerHomeAddress { get; set; }
        //public OfficeAddressModel CustomerOfficeAddress { get; set; }
        public CustomerProfileTypeModel CustomerProfileType { get; set; }
    }

    public class HomeAddressModel
    {
        public string Address { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
    }

    public class OfficeAddressModel
    {
        public string Address { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
    }

    public class CustomerProfileTypeModel
    {
        public string MerchantCustomerId { get; set; }
        public string Email { get; set; }

    }


}
