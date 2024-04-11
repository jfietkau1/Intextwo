using Intextwo.Infrastructure;
using Intextwo.Models;
using Microsoft.Build.Evaluation;
using System.Security.Cryptography.Xml;
using System.Text.Json.Serialization;

namespace Intextwo.Models
{
    public class SessionCart: Cart
    {
        public static Cart GetCart(IServiceProvider services)
        {
            ISession? session = services.GetRequiredService<IHttpContextAccessor>()
                .HttpContext?.Session;


            SessionCart cart = session?.GetJson<SessionCart>("cart") ?? 
                new SessionCart();

            cart.Session = session;

            return cart;

        }

        [JsonIgnore]
        public ISession? Session { get; set; }

        public override void AddItem(Product prod, int quantity)
        {
            base.AddItem(prod, quantity);
            Session?.SetJson("cart", this);
            
        }

        public override void RemoveLine(Product prod)
        {
            base.RemoveLine(prod);
            Session?.SetJson("cart", this);
        }
        public override void Clear()
        {
            base.Clear();
            Session?.Remove("cart");
        }
        public (List<Product>, List<int>) GetCartProductsAndQuantities()
        {
            var products = new List<Product>();
            var quantities = new List<int>();

            // Get the current SessionCart object stored in session.
            var cart = Session?.GetJson<SessionCart>("cart");

            if (cart != null)
            {
                foreach (var line in cart.Lines)
                {
                    products.Add(line.Product);
                    quantities.Add(line.Quantity);
                }
            }

            return (products, quantities);
        }






    }
}


