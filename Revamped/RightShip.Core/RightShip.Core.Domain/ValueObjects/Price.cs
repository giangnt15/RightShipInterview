using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RightShip.Core.Domain.ValueObjects
{
    /// <summary>
    /// A price is a value object that represents a price of a product or service.
    /// It is immutable and has an amount.
    /// It is used to represent the price of a product.
    /// In this case we don't have a currency, we only have an amount for the sake of simplicity.
    /// </summary>
    public record class Price : BaseValueObject
    {
        public decimal Amount
        {
            get { return Amount; }
            init
            {
                if (value < 0)
                {
                    throw new ArgumentException("Amount cannot be negative");
                }
            }
        }

        public static Price operator +(Price a, Price b)
        {
            return new Price() { Amount = a.Amount + b.Amount};
        }

        public static Price operator -(Price a, Price b)
        {
            return new Price() { Amount = a.Amount - b.Amount};
        }

        public static Price operator *(Price a, decimal b)
        {
            return new Price() { Amount = a.Amount * b};
        }

        public static Price operator /(Price a, decimal b)
        {
            return new Price() { Amount = a.Amount / b};
        }
    }
}
