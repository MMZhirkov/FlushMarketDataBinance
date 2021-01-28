using FlushMarketDataBinanceModel.Enums;

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
        /// Buy/Sell
        /// </summary>
        public OrderBookSide OrderBookSide { get; set; }

        public int OrderBookId { get; set; }

        public OrderBook OrderBook { get; set; }
    }
}