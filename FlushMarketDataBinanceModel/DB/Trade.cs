namespace FlushMarketDataBinanceModel
{
    public class Trade
    {
        public int Id { get; set; }

        /// <summary>
        /// Цена для заявки
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Количество заявок по n- цене
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Заявку на покупку
        /// </summary>
        public bool Bid { get; set; }

        /// <summary>
        /// Заявка на продажу
        /// </summary>
        public bool Ask { get; set; }

        public int OrderBookId { get; set; }

        public OrderBook OrderBook { get; set; }
    }
}