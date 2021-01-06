namespace DataModel
{
    public class Trade
    {
        public int Id { get; set; }

        public decimal Price { get; set; }

        public decimal Quantity { get; set; }

        public bool Bid { get; set; }

        public bool Ask { get; set; }

        public int OrderBookId { get; set; }

        public OrderBook OrderBook { get; set; }
    }
}