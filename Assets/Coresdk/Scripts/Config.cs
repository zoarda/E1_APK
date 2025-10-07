namespace Games.Coresdk.Unity
{
    public class Config
    {
        public string MerchantId { get; private set; }

        public string ServiceId { get; private set; }

        public string Domain { get; private set; }

        public string ApiDomain { get; private set; }

        public string PaymentDomain { get; private set; }

        public Config(string loginDomain, string paymentDomain)
        {
            this.Domain = loginDomain;
            this.ApiDomain = loginDomain + "/api";
            this.PaymentDomain = paymentDomain;
            this.MerchantId = string.Empty;
            this.ServiceId = string.Empty;
        }

        public override string ToString()
        {
            return string.Format("Domain:{0}\nApiDomain:{1}\nPaymentDomain:{2}", Domain, ApiDomain, PaymentDomain);
        }
    }
}
