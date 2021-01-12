using System;
using System.Collections.Generic;

namespace DataModel
{
    public class OrderBook
    {
        public int Id { get; set; }

        /// <summary>
        ///   Наименования пары
        /// </summary>
        public string Symbol { get; set; }
        
        /// <summary>
        /// Все заявки в стакане котировок(слепок в определенный момент)
        /// </summary>
        public List<Trade> Trades { get; set; }

        /// <summary>
        /// Время получения слепка
        /// </summary>
        public DateTime TimeTrade { get; set; }
    }
}