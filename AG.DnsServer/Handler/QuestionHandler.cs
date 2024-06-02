using AG.Dns.Requests.Services;
using AG.Dns.Requests;
using AG.DnsServer.Enums;
using AG.DnsServer.Models;
using AG.DnsServer.Models.ResponseData;
using System.Net;

namespace AG.DnsServer.Handler
{
    internal class QuestionHandler: IQueryHandler
    {
        private RequestResolver _requestResolver;
        internal QuestionHandler()
        {
            _requestResolver = new RequestResolver();
        }

        public void Handle(string remoteEndPoint, DnsMessage message, Question question)
        {
            Console.WriteLine("{0} asked for {1} {2} {3}", remoteEndPoint, question.Name, question.Class, question.Type);

            int lastDotIndex = question.Name.LastIndexOf('.');
            if (lastDotIndex == -1 || lastDotIndex == question.Name.Length - 1)
            {
                string errorResponse = "Invalid query format.";
                //return Encoding.UTF8.GetBytes(errorResponse);
            }

            string query;
            TextQueryRData rdata;
            GetQueryAnswer(question, lastDotIndex, out query, out rdata);

            SetResponseMessage(message, query, rdata);
        }

        private void GetQueryAnswer(Question question, int lastDotIndex, out string query, out TextQueryRData rdata)
        {
            string type = question.Name.Substring(lastDotIndex + 1);
            query = question.Name.Substring(0, lastDotIndex);
            IConverter converter = _requestResolver.Resolve(type);
            var result = converter.Query(query);
            rdata = new TextQueryRData() { Data = result.First().Trim() };

            Console.WriteLine(rdata.Data);
        }

        private static void SetResponseMessage(DnsMessage message, string query, TextQueryRData rdata)
        {
            //message.QR = true;
            //message.AA = false;
            //message.RA = false;
            //message.RD = true;
            //message.RCode = (byte)RCode.NOERROR;
            //message.AuthenticatingData = false;

            message.AdditionalCount = 0;
            message.Additionals.Clear();

            message.QR = true;
            message.AA = true;
            message.RA = false;
            message.AnswerCount++;
            message.Answers.Add(new ResourceRecord
            {
                Name = query,
                Class = ResourceClass.IN,
                Type = ResourceType.TXT,
                TTL = 60,
                RData = rdata
            });
        }

        
    }
}
