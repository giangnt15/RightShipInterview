using System;
using LegacyOrderService.Models;
using LegacyOrderService.Data;

namespace LegacyOrderService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Order Processor!");
            Console.WriteLine("Enter customer name:");
            string name = Console.ReadLine()!;
            while (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("Customer name is required, please try again:");
                name = Console.ReadLine()!;
            }

            Console.WriteLine("Enter product name:");
            string product = Console.ReadLine()!;
            while (string.IsNullOrEmpty(product))
            {
                Console.WriteLine("Product name is required, please try again:");
                product = Console.ReadLine()!;
            }
            var productRepo = new ProductRepository();
            double price;
            try
            {
                price = productRepo.GetPrice(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting product price: {ex.Message}");
                return;
            }

            Console.WriteLine("Enter quantity:");
            int qty;
            while (true)
            {
                var qtyInput = Console.ReadLine();
                if (int.TryParse(qtyInput, out qty) && qty > 0)
                {
                    break;
                }

                Console.WriteLine("Quantity must be a positive whole number, please try again:");
            }

            Console.WriteLine("Processing order...");

            Order order = new()
            {
                CustomerName = name,
                ProductName = product,
                Quantity = qty,
                Price = price
            };

            Console.WriteLine("Order complete!");
            Console.WriteLine($"Customer: {order.CustomerName}");
            Console.WriteLine($"Product: {order.ProductName}");
            Console.WriteLine($"Quantity: {order.Quantity}");
            Console.WriteLine($"Total: {(order.Quantity * order.Price):C2}");

            Console.WriteLine("Saving order to database...");
            var repo = new OrderRepository();
            repo.Save(order);
        }
    }
}
