using SenderNotificationOnChangePrice.Const;
using System;

namespace SenderNotificationOnChangePrice
{
    public class Alert
    {
        public string Symbol { get; set; }

        public double ProcentChangePrice { get; set; }

        public DateTime CreateOn { get; set; }

        public TypeAlert TypeAlert { get; set; }
    }
}