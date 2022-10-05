using DatabaseFirst.Dal;
using Microsoft.EntityFrameworkCore;

DbContextInitializer.Build();

//using (var context = new AppDbContext(DbContextInitializer.OptionsBuilder.Options))
using (var context = new AppDbContext())
{
    var products = await context.Products.ToListAsync();
    products.ForEach(product =>
    Console.WriteLine($"Id : {product.Id} \r\nName : {product.Name} \r\nStock : {product.Stock} \r\nPrice : {product.Price} **\r\n")
    );
}
