using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;

using RestSharp;
using RestSharp.Authenticators;

namespace PickemApp.Utility
{
    public class Mailgun
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        private MailgunSendMethod _sendMethod = MailgunSendMethod.SendViaSmtp;
        public MailgunSendMethod SendMethod
        {
            get { return _sendMethod; }
            set { _sendMethod = value; }
        }

        private string _domain = ConfigurationManager.AppSettings["MAILGUN_DOMAIN"];
        private string _ApiKey = ConfigurationManager.AppSettings["MAILGUN_API_KEY"];
        private string _ApiUrl = ConfigurationManager.AppSettings["MAILGUN_API_URL"];

        public Mailgun()
        {

        }

        public Mailgun(string from, string to, string subject, string body)
        {
            this.From = from;
            this.To = to;
            this.Subject = subject;
            this.Body = body;
        }

        public void Send(MailgunSendMethod sendMethod)
        {
            this.SendMethod = sendMethod;
            Send();
        }

        public void Send()
        {
            switch (_sendMethod)
            {
                case MailgunSendMethod.SendViaApi:
                    SendViaApi();
                    break;
                case MailgunSendMethod.SendViaSmtp:
                default:
                    SendViaSmtp();
                    break;
            }
        }

        private void SendViaApi()
        {
            RestClient client = new RestClient();
            client.BaseUrl = new Uri(_ApiUrl);
            client.Authenticator = new HttpBasicAuthenticator("api", _ApiKey);
            
            RestRequest request = new RestRequest();
            request.AddParameter("domain", _domain, ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";

            request.AddParameter("from", From);
            request.AddParameter("to", To);
            request.AddParameter("subject", Subject);
            request.AddParameter("text", Body);

            request.Method = Method.POST;
            var response = client.Execute(request);
            var mailgunResp = DeserializeJsonResponse(response);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InvalidOperationException(mailgunResp.Message);
            }
        }

        private void SendViaSmtp()
        {
            var message = new MailMessage(From, To, Subject, Body);
            message.IsBodyHtml = false;

            // SMTP settings should be set in web.config mailSettings section
            var smtp = new SmtpClient();
            smtp.Send(message);
        }

        private MailgunResponse DeserializeJsonResponse(IRestResponse response)
        {
            RestSharp.Deserializers.JsonDeserializer deserial = new RestSharp.Deserializers.JsonDeserializer();
            return deserial.Deserialize<MailgunResponse>(response);
        }
    }

    public enum MailgunSendMethod
    {
        SendViaApi,
        SendViaSmtp
    }

    public struct MailgunResponse
    {
        public string Message { get; set; }
        public string Id { get; set; }
    }
}