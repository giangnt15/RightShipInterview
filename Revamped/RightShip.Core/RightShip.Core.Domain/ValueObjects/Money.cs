using System;

namespace RightShip.Core.Domain.ValueObjects
{
    /// <summary>
    /// Monetary value object shared across services.
    ///
    /// In a real system we would normally also model the currency as part of this value object. For this exercise we
    /// intentionally omit currency to keep the example focused on the
    /// aggregate and event-sourcing patterns instead of multi-currency logic.
    /// </summary>
    public record class Money : BaseValueObject
    {
        private decimal amount;

        /// <summary>
        /// Monetary amount. Must be greater than or equal to zero.
        /// </summary>
        public decimal Amount
        {
            get => amount;
            init
            {
                if (value < 0)
                {
                    throw new ArgumentException("Amount cannot be negative", nameof(value));
                }

                amount = value;
            }
        }

        public static Money Zero() => new()
        {
            Amount = 0m
        };

        public static Money operator +(Money a, Money b)
        {
            return new Money
            {
                Amount = a.Amount + b.Amount
            };
        }

        public static Money operator -(Money a, Money b)
        {
            return new Money
            {
                Amount = a.Amount - b.Amount
            };
        }

        public static Money operator *(Money money, int multiplier)
        {
            return money * (decimal)multiplier;
        }

        public static Money operator *(Money money, decimal multiplier)
        {
            return new Money
            {
                Amount = money.Amount * multiplier
            };
        }

        public static Money operator /(Money money, decimal divisor)
        {
            if (divisor == 0)
            {
                throw new DivideByZeroException("Cannot divide a Money value by zero.");
            }

            return new Money
            {
                Amount = money.Amount / divisor
            };
        }
    }
}

