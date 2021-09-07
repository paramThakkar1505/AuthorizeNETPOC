using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class ParamController : ControllerBase
    {
        public ParamController()
        {
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = Constants.ApiLoginID,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = Constants.ApiTransactionKey
            };
        }

        [HttpPost("Capture/{transactionID}")]
        [ProducesResponseType(typeof(ViewModels.TransactionResponse), (int)HttpStatusCode.OK)]
        public ViewModels.TransactionResponse Capture(string transactionID , decimal transactionAmount)
        {
            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.priorAuthCaptureTransaction.ToString(),    // capture prior only
                amount = transactionAmount,
                refTransId = transactionID
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };

            var controller = new createTransactionController(request);
            controller.Execute();

            var response = controller.GetApiResponse();
            if (response != null)
            {
                if (response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response.transactionResponse.messages != null)
                    {
                        return new ViewModels.TransactionResponse
                        {
                            AuthorizationCode = response.transactionResponse.authCode,
                            CardType = response.transactionResponse.accountType,
                            Note = response.transactionResponse.messages[0].description,
                            ReferenceNumber = response.transactionResponse.refTransID,
                            ResponseCode = response.transactionResponse.responseCode,
                            ResponseMessage = response.transactionResponse.messages[0].description,
                            ServiceName = "Authorize.Net",
                            TransactionId = response.transactionResponse.transId,
                            TransactionStatus = response.transactionResponse.messages[0].description,
                            amount = transactionAmount
                        };


                        //Console.WriteLine("Successfully created transaction with Transaction ID: " + response.transactionResponse.transId);
                        //Console.WriteLine("Response Code: " + response.transactionResponse.accountType);
                        //Console.WriteLine("Message Code: " + response.transactionResponse.messages[0].code);
                        //Console.WriteLine("Description: " + response.transactionResponse.messages[0].description);
                        //Console.WriteLine("Success, Auth Code : " + response.transactionResponse.authCode);
                    }
                    else
                    {
                        if (response.transactionResponse.errors != null)
                        {
                            throw new Exception(response.transactionResponse.errors[0].errorText);
                            //Console.WriteLine("Error Code: " + response.transactionResponse.errors[0].errorCode);
                            //Console.WriteLine("Error message: " + response.transactionResponse.errors[0].errorText);
                        }
                        else
                        {
                            throw new Exception("Something went wrong");
                        }
                    }
                }
                else
                {
                    //Console.WriteLine("Failed Transaction.");
                    if (response.transactionResponse != null && response.transactionResponse.errors != null)
                    {
                        throw new Exception(response.transactionResponse.errors[0].errorText);
                        //Console.WriteLine("Error Code: " + response.transactionResponse.errors[0].errorCode);
                        //Console.WriteLine("Error message: " + response.transactionResponse.errors[0].errorText);
                    }
                    else
                    {
                        throw new Exception(response.messages.message[0].text);
                        //Console.WriteLine("Error Code: " + response.messages.message[0].code);
                        //Console.WriteLine("Error message: " + response.messages.message[0].text);
                    }
                }
            }
            else
            {
                throw new Exception("Did not get any response");
            }
        }

        [HttpPost("Refund/{transactionID}")]
        [ProducesResponseType(typeof(ViewModels.TransactionResponse), (int)HttpStatusCode.OK)]
        public ViewModels.TransactionResponse Refund(string transactionID, decimal transactionAmount , string customerProfileId , string customerPaymentProfileId)
        {
            customerProfilePaymentType customerProfile = new customerProfilePaymentType();
            customerProfile.customerProfileId = customerProfileId;
            customerProfile.paymentProfile = new paymentProfile
            {
                paymentProfileId = customerPaymentProfileId
            };

            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.refundTransaction.ToString(),    // refund type
                profile = customerProfile,
                amount = transactionAmount,
                refTransId = transactionID
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };

            // instantiate the controller that will call the service
            var controller = new createTransactionController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();

            if (response != null)
            {
                if (response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response.transactionResponse.messages != null)
                    {
                        return new ViewModels.TransactionResponse
                        {
                            AuthorizationCode = response.transactionResponse.authCode,
                            CardType = response.transactionResponse.accountType,
                            Note = response.transactionResponse.messages[0].description,
                            ReferenceNumber = response.transactionResponse.refTransID,
                            ResponseCode = response.transactionResponse.responseCode,
                            ResponseMessage = response.transactionResponse.messages[0].description,
                            ServiceName = "Authorize.Net",
                            TransactionId = response.transactionResponse.transId,
                            TransactionStatus = response.transactionResponse.messages[0].description,
                            amount = transactionAmount
                        };


                        //Console.WriteLine("Successfully created transaction with Transaction ID: " + response.transactionResponse.transId);
                        //Console.WriteLine("Response Code: " + response.transactionResponse.accountType);
                        //Console.WriteLine("Message Code: " + response.transactionResponse.messages[0].code);
                        //Console.WriteLine("Description: " + response.transactionResponse.messages[0].description);
                        //Console.WriteLine("Success, Auth Code : " + response.transactionResponse.authCode);
                    }
                    else
                    {
                        if (response.transactionResponse.errors != null)
                        {
                            throw new Exception(response.transactionResponse.errors[0].errorText);
                            //Console.WriteLine("Error Code: " + response.transactionResponse.errors[0].errorCode);
                            //Console.WriteLine("Error message: " + response.transactionResponse.errors[0].errorText);
                        }
                        else
                        {
                            throw new Exception("Something went wrong");
                        }
                    }
                }
                else
                {
                    //Console.WriteLine("Failed Transaction.");
                    if (response.transactionResponse != null && response.transactionResponse.errors != null)
                    {
                        throw new Exception(response.transactionResponse.errors[0].errorText);
                        //Console.WriteLine("Error Code: " + response.transactionResponse.errors[0].errorCode);
                        //Console.WriteLine("Error message: " + response.transactionResponse.errors[0].errorText);
                    }
                    else
                    {
                        throw new Exception(response.messages.message[0].text);
                        //Console.WriteLine("Error Code: " + response.messages.message[0].code);
                        //Console.WriteLine("Error message: " + response.messages.message[0].text);
                    }
                }
            }
            else
            {
                throw new Exception("Did not get any response");
            }
        }

        [HttpPost("Detail/{transactionID}")]
        [ProducesResponseType(typeof(ViewModels.TransactionResponse), (int)HttpStatusCode.OK)]
        public ViewModels.TransactionResponse Detail(string transactionID)
        {
            var request = new getTransactionDetailsRequest();
            request.transId = transactionID;

            // instantiate the controller that will call the service
            var controller = new getTransactionDetailsController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();
            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (response.transaction == null)
                    throw new Exception("Transaction not found");

                return new ViewModels.TransactionResponse
                {
                    AuthorizationCode = response.transaction.authCode,
                    CardType = (response.transaction.payment.Item as creditCardMaskedType).cardType,
                    Note = "",
                    ReferenceNumber = response.transaction.refTransId,
                    ResponseCode = Convert.ToString(response.transaction.responseCode),
                    ResponseMessage = "",
                    ServiceName = "Authorize.Net",
                    TransactionId = response.transaction.transId,
                    amount = response.transaction.requestedAmount
                };

                //Console.WriteLine("Transaction Id: {0}", response.transaction.transId);
                //Console.WriteLine("Transaction type: {0}", response.transaction.transactionType);
                //Console.WriteLine("Transaction status: {0}", response.transaction.transactionStatus);
                //Console.WriteLine("Transaction auth amount: {0}", response.transaction.authAmount);
                //Console.WriteLine("Transaction settle amount: {0}", response.transaction.settleAmount);
            }
            else 
            {
                throw new Exception(response.messages.message[0].text);
                //Console.WriteLine("Error: " + response.messages.message[0].code + "  " +
                //                  response.messages.message[0].text);
            }
        }

        [HttpPost("PaymentProfile/{customerProfileId}")]
        [ProducesResponseType(typeof(ViewModels.CustomerPaymentProfile), (int)HttpStatusCode.OK)]
        public ViewModels.CustomerPaymentProfile paymentProfile(string customerProfileId , string cardNumber , string expirationDate)
        {
            var creditCard = new creditCardType
            {
                cardNumber = cardNumber,// "4111111111111111",
                expirationDate = expirationDate // "1028"
            };

            paymentType cc = new paymentType { Item = creditCard };

            customerPaymentProfileType echeckPaymentProfile = new customerPaymentProfileType();
            echeckPaymentProfile.payment = cc;

            var request = new createCustomerPaymentProfileRequest
            {
                customerProfileId = customerProfileId,
                paymentProfile = echeckPaymentProfile,
                validationMode = validationModeEnum.none
            };

            // instantiate the controller that will call the service
            var controller = new createCustomerPaymentProfileController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            createCustomerPaymentProfileResponse response = controller.GetApiResponse();

            if (response != null)
            {
                if (response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response.messages.message != null)
                    {
                        return new ViewModels.CustomerPaymentProfile
                        {
                            paymentProfileId = response.customerPaymentProfileId
                        };
                    }
                }
                else
                {
                    //Console.WriteLine("Customer Payment Profile Creation Failed.");
                    //Console.WriteLine("Error Code: " + response.messages.message[0].code);
                    //Console.WriteLine("Error message: " + response.messages.message[0].text);
                    if (response.messages.message[0].code == "E00039")
                    {
                        throw new Exception("Duplicate Payment Profile ID: " + response.customerPaymentProfileId);
                        //Console.WriteLine("Duplicate Payment Profile ID: " + response.customerPaymentProfileId);
                    }
                    throw new Exception(response.messages.message[0].text);
                }
            }
            else
            {
                if (controller.GetErrorResponse().messages.message.Length > 0)
                {
                    //Console.WriteLine("Customer Payment Profile Creation Failed.");
                    //Console.WriteLine("Error Code: " + response.messages.message[0].code);
                    throw new Exception("Error message: " + response.messages.message[0].text);
                }
                else
                {
                    throw new Exception("Null Response.");
                }
            }
            throw new Exception("Something went wrong");
        }
    }
}
