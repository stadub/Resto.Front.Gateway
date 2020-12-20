using SQLite;

namespace alivery
{
    public class Product : ValueObject
    {
        public int TransactionNumber { get; set; }
        public string OrderItemId { get; set; }


        public string Name { get; set; }



        public string Json { get; set; }
    }
}