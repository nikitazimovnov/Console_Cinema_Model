using System;

namespace Cinema
{
    /// <summary>
    /// Место в кинотеатре.
    /// </summary>
    public class Place
    {
        /// <summary>
        /// Максимальная цена места.
        /// </summary>
        public const int MAX_PRICE = 999999;

        /// <summary>
        /// Минимальная цена места.
        /// </summary>
        public const int MIN_PRICE = 1;

        /// <summary>
        /// Свободно ли место (по умолчанию место свободно).
        /// </summary>
        public bool IsFree { get; private set; } = true;

        /// <summary>
        /// Цена на место.
        /// </summary>
        public int Price { get; private set; }

        /// <summary>
        /// Конструктор класса место в кинотеатре.
        /// </summary>
        /// <param name="price"> Цена на место. </param>
        public Place(int price) => SetNewPrice(price);

        /// <summary>
        /// Занять место.
        /// </summary>
        public void MakeBusy() => IsFree = false;

        /// <summary>
        /// Освободить место.
        /// </summary>
        public void MakeFree() => IsFree = true;

        /// <summary>
        /// Поменять цену на место.
        /// </summary>
        /// <param name="newPrice"> Новая цена. </param>
        public void SetNewPrice(int newPrice)
        {
            if (newPrice < MIN_PRICE || newPrice > MAX_PRICE)
                throw new ArgumentException($"Не удалось сменить цену.{Environment.NewLine}Цена должна быть не меньше, чем {MIN_PRICE} и не больше, чем {MAX_PRICE}.");

            Price = newPrice;
        }
    }
}
