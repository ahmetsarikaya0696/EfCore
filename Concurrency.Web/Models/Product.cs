﻿namespace Concurrency.Web.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
