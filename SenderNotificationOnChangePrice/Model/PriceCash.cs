﻿using System;

namespace SenderNotificationOnChangePrice.Core
{
    public class PriceCash
    {
        public string Symbol { get; set; }
        public double Price { get; set; }
        public DateTime CreateOn { get; set; }
    }
}