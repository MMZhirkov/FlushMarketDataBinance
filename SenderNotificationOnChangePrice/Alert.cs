using System;

namespace SenderNotificationOnChangePrice
{
    public class Alert
    {
        public string Symbol { get; set; }

        public decimal ProcentChangePrice { get; set; }

        public DateTime CreateOn { get; set; }
    }
}