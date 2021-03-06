using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;
using AuthorizeNETPOC.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AuthorizeNETPOC.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1")]
    public class AnkitPocController : ControllerBase
    {
        public AnkitPocController()
        {
            // set whether to use the sandbox environment, or production enviornment
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = Constants.ApiLoginID,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = Constants.ApiTransactionKey
            };
        }


        [HttpPost("createcustomerprofile")]
        public IActionResult CreateCustomerProfile(CustomerProfileModel customerProfilemodel)
        {
            try
            {
                var creditCard = new creditCardType
                {
                    cardNumber = customerProfilemodel.CreditCard.CardNumber,
                    expirationDate = customerProfilemodel.CreditCard.ExpiryMonth + customerProfilemodel.CreditCard.ExpiryYear
                };

                //var bankAccount = new bankAccountType
                //{
                //    accountNumber = customerProfilemodel.BankAccount.AccountNumber,
                //    routingNumber = customerProfilemodel.BankAccount.RoutingNumber,
                //    accountType = bankAccountTypeEnum.checking,
                //    echeckType = echeckTypeEnum.WEB,
                //    nameOnAccount = customerProfilemodel.BankAccount.NameOnAccount,
                //    bankName = customerProfilemodel.BankAccount.BankName
                //};

                // standard api call to retrieve response
                paymentType cc = new paymentType { Item = creditCard };

                List<customerPaymentProfileType> paymentProfileList = new List<customerPaymentProfileType>();
                customerPaymentProfileType ccPaymentProfile = new customerPaymentProfileType();
                ccPaymentProfile.payment = cc;

                //customerPaymentProfileType echeckPaymentProfile = new customerPaymentProfileType();
                //echeckPaymentProfile.payment = echeck;

                paymentProfileList.Add(ccPaymentProfile);
                //paymentProfileList.Add(echeckPaymentProfile);

                //List<customerAddressType> addressInfoList = new List<customerAddressType>();
                //customerAddressType homeAddress = new customerAddressType();
                //homeAddress.address = customerProfilemodel.CustomerHomeAddress.Address;
                //homeAddress.city = customerProfilemodel.CustomerHomeAddress.City;
                //homeAddress.zip = customerProfilemodel.CustomerHomeAddress.Zip;

                //customerAddressType officeAddress = new customerAddressType();
                //officeAddress.address = customerProfilemodel.CustomerOfficeAddress.Address;
                //officeAddress.city = customerProfilemodel.CustomerOfficeAddress.City;
                //officeAddress.zip = customerProfilemodel.CustomerOfficeAddress.Zip;

                //addressInfoList.Add(homeAddress);
                //addressInfoList.Add(officeAddress);


                customerProfileType customerProfile = new customerProfileType();
                customerProfile.merchantCustomerId = customerProfilemodel.CustomerProfileType.MerchantCustomerId;
                //customerProfile.email = customerProfilemodel.CustomerProfileType.Email;
                customerProfile.paymentProfiles = paymentProfileList.ToArray();
                //customerProfile.shipToList = addressInfoList.ToArray();

                var request = new createCustomerProfileRequest { profile = customerProfile, validationMode = validationModeEnum.none };

                // instantiate the controller that will call the service
                var controller = new createCustomerProfileController(request);
                controller.Execute();

                // get the response from the service (errors contained if any)
                createCustomerProfileResponse response = controller.GetApiResponse();

                // validate response 
                if (response != null)
                {
                    if (response.messages.resultCode == messageTypeEnum.Ok)
                    {
                        if (response.messages.message != null)
                        {
                            Console.WriteLine("Success!");
                            Console.WriteLine("Customer Profile ID: " + response.customerProfileId);
                            Console.WriteLine("Payment Profile ID: " + response.customerPaymentProfileIdList[0]);
                            //Console.WriteLine("Shipping Profile ID: " + response.customerShippingAddressIdList[0]);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Customer Profile Creation Failed.");
                        Console.WriteLine("Error Code: " + response.messages.message[0].code);
                        Console.WriteLine("Error message: " + response.messages.message[0].text);
                    }
                }
                else
                {
                    if (controller.GetErrorResponse().messages.message.Length > 0)
                    {
                        Console.WriteLine("Customer Profile Creation Failed.");
                        Console.WriteLine("Error Code: " + response.messages.message[0].code);
                        Console.WriteLine("Error message: " + response.messages.message[0].text);
                    }
                    else
                    {
                        Console.WriteLine("Null Response.");
                    }
                }

                return StatusCode((int)HttpStatusCode.OK, JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost("authtransaction")]
        public IActionResult TransactionAuth(AuthTransactionModel authTransactionModel)
        {
            try
            {
                //create a customer payment profile
                customerProfilePaymentType profileToCharge = new customerProfilePaymentType();
                profileToCharge.customerProfileId = authTransactionModel.CustomerProfileId;
                profileToCharge.paymentProfile = new paymentProfile { paymentProfileId = authTransactionModel.CustomerPaymentProfileId };

                var transactionRequest = new transactionRequestType
                {
                    transactionType = transactionTypeEnum.authOnlyTransaction.ToString(),    // refund type
                    amount = authTransactionModel.Amount,
                    profile = profileToCharge
                };

                var request = new createTransactionRequest { transactionRequest = transactionRequest };

                // instantiate the collector that will call the service
                var controller = new createTransactionController(request);
                controller.Execute();

                // get the response from the service (errors contained if any)
                var response = controller.GetApiResponse();

                // validate response
                if (response != null)
                {
                    if (response.messages.resultCode == messageTypeEnum.Ok)
                    {
                        if (response.transactionResponse.messages != null)
                        {
                            return StatusCode((int)HttpStatusCode.OK,
                                    JsonConvert.SerializeObject(
                                            new TransactionResponse
                                            {
                                                AuthorizationCode = response.transactionResponse.authCode,
                                                CardType = response.transactionResponse.accountType,
                                                Note = response.transactionResponse.messages[0].description,
                                                ReferenceNumber = response.transactionResponse.refTransID,
                                                ResponseCode = response.transactionResponse.responseCode,
                                                ResponseMessage = "",
                                                ServiceName = "Authorize .net",
                                                TransactionId = response.transactionResponse.transId,
                                                TransactionStatus = response.transactionResponse.messages[0].description,
                                                amount = authTransactionModel.Amount
                                            })
                                            );
                        }
                    }

                    var errorCode = string.Empty;
                    var errorText = string.Empty;

                    if (response.transactionResponse != null && response.transactionResponse.errors != null)
                    {
                        errorCode = response.transactionResponse.errors[0].errorCode;
                        errorText = response.transactionResponse.errors[0].errorText;
                    }
                    else
                    {
                        errorCode = response.messages.message[0].code;
                        errorText = response.messages.message[0].text;
                    }

                    return StatusCode((int)HttpStatusCode.OK,
                                JsonConvert.SerializeObject(
                                        new TransactionResponse
                                        {
                                            Note = response.transactionResponse.errors[0].errorText,
                                            ResponseCode = response.transactionResponse.errors[0].errorCode,
                                            ResponseMessage = "Error",
                                            ServiceName = "Authorize .net",
                                            TransactionStatus = "Failed Transaction.",
                                        })
                                        );
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.OK,
                                JsonConvert.SerializeObject(
                                        new TransactionResponse
                                        {
                                            ResponseMessage = "Error",
                                            ServiceName = "Authorize .net",
                                            TransactionStatus = "Null Responese.",
                                        })
                                        );
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost("voidtransaction")]
        public IActionResult TransactionVoid([FromQuery] string transactionId)
        {
            try
            {
                Console.WriteLine("Void Transaction");

                var transactionRequest = new transactionRequestType
                {
                    transactionType = transactionTypeEnum.voidTransaction.ToString(),    // refund type
                    refTransId = transactionId
                };

                var request = new createTransactionRequest { transactionRequest = transactionRequest };

                // instantiate the controller that will call the service
                var controller = new createTransactionController(request);
                controller.Execute();

                // get the response from the service (errors contained if any)
                var response = controller.GetApiResponse();

                // validate response
                if (response != null)
                {
                    if (response.messages.resultCode == messageTypeEnum.Ok)
                    {
                        if (response.transactionResponse.messages != null)
                        {
                            return StatusCode((int)HttpStatusCode.OK,
                                    JsonConvert.SerializeObject(
                                            new TransactionResponse
                                            {
                                                AuthorizationCode = response.transactionResponse.authCode,
                                                CardType = response.transactionResponse.accountType,
                                                Note = response.transactionResponse.messages[0].description,
                                                ReferenceNumber = response.transactionResponse.refTransID,
                                                ResponseCode = response.transactionResponse.responseCode,
                                                ResponseMessage = "",
                                                ServiceName = "Authorize .net",
                                                TransactionId = response.transactionResponse.transId,
                                                TransactionStatus = response.transactionResponse.messages[0].description
                                            })
                                            );
                        }
                    }

                    var errorCode = string.Empty;
                    var errorText = string.Empty;

                    if (response.transactionResponse != null && response.transactionResponse.errors != null)
                    {
                        errorCode = response.transactionResponse.errors[0].errorCode;
                        errorText = response.transactionResponse.errors[0].errorText;
                    }
                    else
                    {
                        errorCode = response.messages.message[0].code;
                        errorText = response.messages.message[0].text;
                    }

                    return StatusCode((int)HttpStatusCode.OK,
                                JsonConvert.SerializeObject(
                                        new TransactionResponse
                                        {
                                            Note = response.transactionResponse.errors[0].errorText,
                                            ResponseCode = response.transactionResponse.errors[0].errorCode,
                                            ResponseMessage = "Error",
                                            ServiceName = "Authorize .net",
                                            TransactionStatus = "Failed Transaction.",
                                        })
                                        );

                }
                else
                {
                    return StatusCode((int)HttpStatusCode.OK,
                                JsonConvert.SerializeObject(
                                        new TransactionResponse
                                        {
                                            ResponseMessage = "Error",
                                            ServiceName = "Authorize .net",
                                            TransactionStatus = "Null Responese.",
                                        })
                                        );
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpDelete("customerpaymentprofile")]
        public IActionResult CustomerPaymentProfile(CustomerPaymentProfile customerPaymentProfile)
        {
            try
            {
                Console.WriteLine("DeleteCustomerPaymentProfile Sample");

                //please update the subscriptionId according to your sandbox credentials
                var request = new deleteCustomerPaymentProfileRequest
                {
                    customerProfileId = customerPaymentProfile.profileId,
                    customerPaymentProfileId = customerPaymentProfile.profileId
                };

                //Prepare Request
                var controller = new deleteCustomerPaymentProfileController(request);
                controller.Execute();

                //Send Request to EndPoint
                deleteCustomerPaymentProfileResponse response = controller.GetApiResponse();
                if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response != null && response.messages.message != null)
                    {
                        Console.WriteLine("Success, ResultCode : " + response.messages.resultCode.ToString());
                        return StatusCode((int)HttpStatusCode.OK,
                                    JsonConvert.SerializeObject(
                                            new TransactionResponse
                                            {
                                                Note = response.messages.ToString(),
                                                ReferenceNumber = response.refId,
                                                ResponseMessage = "",
                                                ServiceName = "Authorize .net",
                                                TransactionStatus = response.messages.ToString()
                                            })
                                            );
                    }
                }
                else if (response != null)
                {
                    return StatusCode((int)HttpStatusCode.OK,
                                JsonConvert.SerializeObject(
                                        new TransactionResponse
                                        {
                                            Note = response.messages.message[0].text,
                                            ResponseCode = response.messages.message[0].code,
                                            ResponseMessage = "Error",
                                            ServiceName = "Authorize .net",
                                            TransactionStatus = "Failed Transaction.",
                                        })
                                        );
                }

                return StatusCode((int)HttpStatusCode.OK,
                                JsonConvert.SerializeObject(
                                        new TransactionResponse
                                        {
                                            ResponseMessage = "Error",
                                            ServiceName = "Authorize .net",
                                            TransactionStatus = "Null Responese.",
                                        })
                                        );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
