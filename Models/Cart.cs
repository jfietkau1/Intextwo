using Microsoft.Build.Evaluation;

namespace Intextwo.Models
{
    public class Cart
    {
        public List<CartLine> Lines { get; set; } = new List<CartLine>();


        public void AddItem(Product prod, int quantity)
        {
            CartLine? line = Lines.
                Where(x => x.Product.product_ID == prod.product_ID)
                .FirstOrDefault();
            //has this item already been added to our cart?
            if (line == null)
            {
                Lines.Add(new CartLine
                {
                    Product = prod,
                    Quantity = quantity
                });
            }
            else
            {
                line.Quantity += quantity;
            }
        }

        public void RemoveLine(Product prod) => Lines.RemoveAll(x => x.Product.product_ID== prod.product_ID);

        public void Clear() => Lines.Clear();

        public decimal CalculateTotal() => (decimal)(Lines.Sum(x => x.Product.price * x.Quantity));




        public class CartLine
        {
            public int CartLineId { get; set; }
            public Product Product { get; set; }
            public int Quantity { get; set; }

        }


    }
}
