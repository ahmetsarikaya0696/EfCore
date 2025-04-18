﻿using CodeFirst.Dal;
using Microsoft.EntityFrameworkCore;
using System.Data;

Initializer.Build();

#region Pagination
static List<Product> GetPaginatedProducts(int pageNumber, int itemPerPage)
{
    using (var _context = new AppDbContext())
    {
        int skip = (pageNumber - 1) * itemPerPage;
        var productsWithPage = _context.Products.OrderByDescending(p => p.Id)
                                                .Skip(skip)
                                                .Take(itemPerPage)
                                                .ToList();

        return productsWithPage;
    }
}

//GetPaginatedProducts(1, 3).ForEach(p => Console.WriteLine($"Id : {p.Id}\r\nName : {p.Name}"));
#endregion

using (var _context = new AppDbContext())
{
    #region Transaction
    // Tek bir SaveChanges() kullanılıyorsa scope açmaya gerek yoktur.

    using (var transaction = _context.Database.BeginTransaction())
    {
        Category category = new() { Name = "Telefonlar" };
        _context.Categories.Add(category);
        _context.SaveChanges();

        Product product = new()
        {
            Name = "Kılıf 1",
            Price = 123,
            DiscountPrice = 100,
            Stock = 123,
            Barcode = 123,
            CategoryId = category.Id,
            ProductFeature = new ProductFeature() { Color = "Red", Width = 123, Height = 123 }
        };

        _context.Products.Add(product);
        _context.SaveChanges();
        transaction.Commit();
    }
    #endregion


    #region StoredProcedure Function Farkları
    /*
     * SPler IN ve OUT parametreleri alırken
     * Functionlar OUT parametresi almaz. Yani değer döndürmez.
     * 
     * SPler içinde Function kullanılabilirken.
     * Functionların içinde SP kullanılamaz.
     * 
     * SPler içinde try catch blokları kullanılabilirken
     * Functionlar içinde kullanılamaz.
     * 
     * SELECT cümlecikleri içinde Function kullanılabilirken
     * SP kullanılamaz.
     * 
     * SPden sonra where ifadesi kullanıldığında hata verir fakat
     * Functionlardan sonra kullanılabilir.
     */
    #endregion

    #region Function
    #region Table-Valued Function
    ////Parametresiz Func
    //var products = _context.ProductWithCategoryAndFeatures.ToList();

    ////Parametre Alan Func 1.Yol
    //int categoryId = 1;
    //var products = _context.QueriedProducts.FromSqlInterpolated($"select * from FcProductfullWithParameters({categoryId})");

    ////Parametre Alan Func 2.Yol
    //var product = _context.GetProductWithFeatures(1).ToList();


    #endregion

    #region Scalar-Valued Function
    //var count = _context.GetProductCount(3); // Scalar-Valued metotlar tek başına kullanılamaz.

    var categories = _context.Categories.Select(x => new
    {
        CategoryName = x.Name,
        ProductCount = _context.GetProductCount(x.Id)
    }).ToList();

    int categoryId = 1;
    var productCount = _context.ProductCount.FromSqlInterpolated($"SELECT dbo.fc_get_product_count({categoryId}) AS Count").First().Count;
    #endregion

    #endregion

    #region StoredProcedure
    //// Derleme ve execution plan sp ilk oluştuğunda meydana gelir ve bir daha oluşmaz. Bu yüzden normal sorgulara göre daha hızlıdır.
    //// ssms tarafında sp oluşturan kod: CREATE PROCEDURE SpGetProducts AS BEGIN SELECT * FROM Products END 
    //var products = _context.Products.FromSqlRaw("exec SpGetProducts").ToList();

    //var productWithCategoryAndFeature = _context.ProductWithCategoryAndFeatures.FromSqlRaw("exec SpGetProductsFull").ToList();

    //int categoryId = 1;
    //decimal price = 50;
    //var productsFullWithParameters = _context.ProductWithCategoryAndFeatures.FromSqlInterpolated($"exec SpGetProductsFullWithParameters {categoryId},{price}").ToList();


    //Product product = new()
    //{
    //    Name = "Product",
    //    Price = 300,
    //    Stock = 70,
    //    Barcode = 123,
    //    DiscountPrice = 50,
    //    CategoryId = 1
    //};

    //var newProductId = new SqlParameter("@newId", SqlDbType.Int);
    //newProductId.Direction = ParameterDirection.Output;

    //_context.Database.ExecuteSqlInterpolated($"EXEC SpInsertProduct {product.Name},{product.Price},{product.DiscountPrice},{product.Stock},{product.Barcode},{product.CategoryId}, {newProductId} out");

    //var productId = newProductId.Value;
    #endregion

    #region Global Query Filters
    //var products = _context.Products.ToList(); // Global query filter uygulanmış şekilde gelir
    //var products = _context.Products.IgnoreQueryFilters().ToList(); // Global query filter uygulanmamış şekilde gelir
    #endregion

    #region ToView
    //var products = _context.ProductWithCategoryAndFeatures.ToList();
    #endregion

    #region ToSqlQuery ile kod merkezileştirme
    //.ToSqlQuery("SELECT Id,Name,Price FROM Products"); metodu ile kod merkezileştirildi.
    //var products = _context.ProductDtos.ToList();
    #endregion

    #region Raw Sql
    //var products = _context.Products.FromSqlRaw("SELECT * FROM Products").ToList();

    //var id = new SqlParameter("@_Id", 5);
    //var productWithIdFilter = _context.Products.FromSqlRaw($"SELECT * FROM Products WHERE Id = @_Id", id).First();

    //var price = new SqlParameter("@_Price", 300);
    //var productsWithPriceFilter = _context.Products.FromSqlRaw($"SELECT * FROM Products WHERE Price > @_Price", price).ToList();

    //var productDto = _context.ProductDtos.FromSqlRaw("SELECT Id, Name, Price FROM Products").First();

    #endregion

    #region FullOuterJoin
    //var left = (from p in _context.Products
    //            join pf in _context.ProductFeatures on p.Id equals pf.Id into pfList
    //            from pf in pfList.DefaultIfEmpty()
    //            select new { p, pf }).ToList();

    //var right = (from pf in _context.ProductFeatures
    //             join p in _context.Products on pf.Id equals p.Id into pList
    //             from p in pList.DefaultIfEmpty()
    //             select new { p, pf }).ToList();

    //var fullOuterJoin = left.Union(right);
    #endregion

    #region LeftJoin
    //var result = (from p in _context.Products
    //              join pf in _context.ProductFeatures on p.Id equals pf.Id into pfList
    //              from pf in pfList.DefaultIfEmpty()
    //              select new { p, pf }).ToList();
    #endregion

    #region InnerJoin
    //// nav prop olmadığı zamanlarda tabloları birbirine bağlamak için yapılır.
    //var result = _context.Categories.Join(_context.Products, c => c.Id, p => p.CategoryId, (c, p) => new
    //{
    //    CategoryName = c.Name,
    //    ProductName = p.Name,
    //    ProductPrice = p.Price
    //}).ToList();

    //var result2 = (from c in _context.Categories
    //               join p in _context.Products on c.Id equals p.CategoryId
    //               select new
    //               {
    //                   CategoryName = c.Name,
    //                   ProductName = p.Name,
    //                   ProductPrice = p.Price
    //               });

    //var result3 = _context.Categories.Join(_context.Products, c => c.Id, p => p.CategoryId, (c, p) => new { c, p })
    //                                 .Join(_context.ProductFeatures, cp => cp.p.Id, pf => pf.Id, (cp, pf) => new
    //                                 {
    //                                     CategoryName = cp.c.Name,
    //                                     ProductName = cp.p.Name,
    //                                     ProductPrice = cp.p.Price,
    //                                     ProductHeight = pf.Height
    //                                 }).ToList();
    #endregion

    #region ClientServerEvaluation
    //_context.People.Add(new() { Name = "Ahmet", Phone = "05312228514" });
    //_context.People.Add(new() { Name = "Mehmet", Phone = "05332238533" });
    //_context.SaveChanges();

    ////var people = _context.People.Where(p => FormatPhone(p.Phone) == "5312228514"); // SQL tarafında FormatPhone isminde bir fonksiyon yok.
    //var people = _context.People.ToList().Where(p => FormatPhone(p.Phone) == "5312228514"); // Data memorye alındı.

    #endregion

    #region Constraint
    //var category = new Category() { Name = "Kalemler" };

    //var product = new Product()
    //{
    //    Name = "Kalem 123",
    //    Price = 123,
    //    DiscountPrice = 234, // Constrainte takılacaktır.
    //    Stock = 123,
    //    Barcode = 123,
    //    Category = category,
    //    ProductFeature = new ProductFeature() { Color = "Red", Width = 123, Height = 123 }
    //};

    //_context.Add(product);
    //_context.SaveChanges();
    #endregion

    #region Entity Properties
    // Db ' de oluşmasını istemediğimiz propertyleri ignorelayabiliriz.
    #endregion

    #region Keyless Entity
    //var category = new Category() { Name = "Kalemler" };

    //var kalem1 = new Product()
    //{
    //    Name = "Kalem 1",
    //    Price = 123,
    //    Stock = 123,
    //    Barcode = 123,
    //    Category = category,
    //    ProductFeature = new ProductFeature() { Color = "Red", Width = 123, Height = 123 }
    //};
    //var kalem2 = new Product()
    //{
    //    Name = "Kalem 2",
    //    Price = 234,
    //    Stock = 234,
    //    Barcode = 234,
    //    Category = category,
    //    ProductFeature = new ProductFeature() { Color = "Green", Width = 234, Height = 234 }
    //};
    //var kalem3 = new Product()
    //{
    //    Name = "Kalem 3",
    //    Price = 345,
    //    Stock = 345,
    //    Barcode = 345,
    //    Category = category,
    //    ProductFeature = new ProductFeature() { Color = "Blue", Width = 345, Height = 345 }
    //};

    //_context.AddRange(kalem1, kalem2, kalem3);
    //_context.SaveChanges();

    //// Geriye dönülen datayı QueriedProduct tipine dönüştürür.
    //var queriedProducts = _context.QueriedProducts.FromSqlRaw(
    //    @"SELECT p.Id N'Product_ID', c.Name N'CategoryName', p.Name, p.Price, pf.Height FROM Products P 
    //    JOIN ProductFeatures Pf on P.Id = Pf.Id 
    //    JOIN Categories C on C.Id = P.CategoryId;").ToList();

    //_context.QueriedProducts.AddRange(queriedProducts);
    //Console.WriteLine("İşlem bitti");
    #endregion

    #region Owned Entity
    //// Teacher ' a bağlı olarak Student ' ları eklemek
    //Teacher teacher = new() { Person = new() { Name = "Yiğit", Surname = "Hacıefendioğlu", Age = 33 } };
    //teacher.Students.Add(new()
    //{
    //    Person = new() { Name = "Ahmet", Surname = "Sarıkaya", Age = 26 },
    //    Grade = 3,
    //});
    //_context.Add(teacher);
    //_context.SaveChanges();

    //var student = _context.Students.First();
    //Console.WriteLine(student.Person.Name); 
    #endregion

    #region Related Data Load
    #region LazyLoading
    // Microsoft.EntityFrameworkCore.Proxies kütüphanesinin eklenir
    // onConfiguring kısmında builder.UseLazyLoadlingProxies() ' inin kodu yazılır
    // navigation propertyler virtual olarak işaretlenir.
    #endregion

    #region ExplicitLoading
    //var category = _context.Categories.First();
    ////.... Arada bir takım işlemler yaptık fakat bu işlemlerde kategorinin Products larına ihtiyaç yok
    //if (true) // Durumuna girdiğinde Productslara ihtiyaç var
    //{
    //    _context.Entry(category).Collection(c => c.Products).Load();
    //    // Kategorinin productları dahil edildi.
    //}

    //// Product Üzerinden Örnek
    //var product = _context.Products.First();
    //_context.Entry(product).Reference(p => p.ProductFeature).Load();
    #endregion

    #region EagerLoading
    //var categoryWithProducts = _context.Categories.Include(c => c.Products)
    //                                              .ThenInclude(p => p.ProductFeature)
    //                                              .First();
    #endregion
    #endregion

    #region Delete Behavior : SetNull
    //// Category silinir ve Product ' ın CategoryId ' si null olur.
    //var product = new Product()
    //{
    //    Name = "Kalem 321",
    //    Price = 123,
    //    Stock = 123,
    //    Barcode = 123,
    //    ProductFeature = new ProductFeature() { Color = "Red", Width = 123, Height = 123 }
    //};

    //var category = new Category() { Name = "Kalemler" };
    //category.Products.Add(product);

    //_context.Add(category);
    //_context.SaveChanges();

    //_context.Remove(category);
    //_context.SaveChanges();
    #endregion

    #region many-to-many Data Ekleme
    //// Student ' a bağlı olarak Teacher ' ları eklemek
    //Student student = new() { Name = "Ahmet", Surname = "Sarikaya", Age = 26 };
    //student.Teachers.Add(new() { Name = "Yiğit", Surname = "Hacıefendioğlu", Age = 33 });
    //student.Teachers.Add(new() { Name = "Galip", Surname = "Yıldız", Age = 26 });
    //_context.Add(student);
    //_context.SaveChanges();

    //// Teacher ' a bağlı olarak Student ' ları eklemek
    //Teacher teacher = new() { Name = "Yiğit Hoca", Surname = "Hacıefendioğlu", Age = 33 };
    //teacher.Students.Add(new() { Name = "Ahmet Öğrenci", Surname = "Hacıefendioğlu", Age = 33 });
    //teacher.Students.Add(new() { Name = "Galip Öğrenci", Surname = "Yıldız", Age = 26 });
    //_context.Add(teacher);
    //_context.SaveChanges();

    //// 2.Yol
    //Student student = new() { Name = "Ahmet", Surname = "Sarikaya", Age = 26 };
    //student.Teachers.AddRange(new List<Teacher>()
    //{
    //    new Teacher() { Name = "Yiğit", Surname = "Hacıefendioğlu", Age = 33 } ,
    //    new Teacher() { Name = "Galip", Surname = "Yıldız", Age = 26 }
    //});
    //_context.Add(student);
    //_context.SaveChanges();

    //// Var olan teacher ' a veri eklemek
    //var teacher = _context.Teachers.First(x => x.Name == "Yiğit");
    //teacher.Students.AddRange(new List<Student>()
    //{
    //    new () { Name = "Berkay", Surname = "Özışık", Age = 29 } ,
    //    new () { Name = "Mustafa", Surname = "Yılmaz", Age = 28 }
    //});
    //// zaten var olan bir teacher olduğundan tekrar eklemiyoruz. Öğrencilerine ekleme yapıyoruz.
    //// Update dememize de gerek yok. Direkt save edebiliriz.
    //_context.SaveChanges();
    #endregion

    #region one-to-one Data Ekleme
    /*
     * Product parent, ProductFeature dependent olduğundan Product ProductFeature a sahip olmak zorunda değildir.
       ProductFeature Product ' a sahip olmak zorundadır.
    */
    //var product = new Product()
    //{
    //    Name = "Silgi 1",
    //    Price = 120,
    //    Stock = 200,
    //    Barcode = 320,
    //    Category = new() {Name="Silgiler"},
    //    ProductFeature = new ProductFeature() { Color = "Red", Width = 123, Height = 123 }
    //};

    // 2. Yöntem
    //var category = _context.Categories.First(x => x.Name == "Silgiler");
    //var product = new Product()
    //{
    //    Name = "Silgi 1",
    //    Price = 120,
    //    Stock = 200,
    //    Barcode = 320,
    //    Category = category
    //};
    //ProductFeature productFeature = new() {Color="Blue", Width=200, Height=300,Product = product };
    //_context.ProductFeatures.Add(productFeature);
    //_context.SaveChanges();
    #endregion

    #region Category üzerinden Product Ekleme
    //var product = new Product()
    //{
    //    Name = "Kalem 321",
    //    Price = 123,
    //    Stock = 123,
    //    Barcode = 123,
    //    ProductFeature = new ProductFeature() { Color = "Red", Width = 123, Height = 123 }
    //};

    //var category = new Category() { Name = "Kalemler" };
    //category.Products.Add(product);

    //_context.Add(category);
    //_context.SaveChanges();
    #endregion

    #region Product üzerinden Category ekleme
    //var category = new Category() { Name = "Kalemler" };

    //// Aşağıdaki kod gereksizdir. Navigation propertysi olan classlardan biri eklendiğinde diğeri de eklenir. 
    ////_context.Add(category); 

    //var product = new Product()
    //{
    //    Name = "Kalem 123",
    //    Price = 123,
    //    Stock = 123,
    //    Barcode = 123,
    //    Category = category,
    //    ProductFeature = new ProductFeature() { Color = "Red", Width = 123, Height = 123 }
    //};

    //_context.Add(product);
    //_context.SaveChanges();
    #endregion

    #region DbSet Metotları
    // Id ' si 3 olan Product ' ı arar. Bulamazsa Exception fırlatır.
    //var product = _context.Products.First(p => p.Id == 3);

    // Id ' si 3 olan Product ' ı arar. Bulamazsa null döndürür.
    //var product = _context.Products.FirstOrDefault(p => p.Id == 3);

    // Id ' si 3 olan Product ' ı arar. Bulamazsa ikinci parametredeki değeri döndürür.
    //var product = _context.Products.FirstOrDefault(p => p.Id == 3, new Product() { Id = 3, Name="Default Kalem"});

    // Id ' si 10'dan büyük olan tek bir kayıt varsa döndürür.
    // Eşleşen durum bulamazsa hata fırlatır.
    // Eşleşen birden fazla kayıt bulursa hata fırlatır.
    //var product = _context.Products.Single(p => p.Id > 10);

    // Id ' si 10'dan büyük olan tek bir kayıt varsa döndürür.
    // Eşleşen durum bulamazsa null döner.
    // Eşleşen birden fazla kayıt bulursa hata fırlatır.
    //var product = _context.Products.SingleOrDefault(p => p.Id > 10);

    // Id ' si 10'dan büyük olan Product'ları getirir.
    //var products = await _context.Products.Where(p => p.Id > 10).ToListAsync();

    // Birden fazla primarykey olabilir ve bu FindAsync ile aranabilir.
    //var products = await _context.Products.FindAsync(10);

    // Id ' si 3 olan Product ' ı arar. Bulamazsa null döndürür.
    //var products = await _context.Products.FirstOrDefaultAsync(p => p.Id == 3);
    #endregion

    #region CreatedDate merkezi şekilde ekleme
    //await _context.Products.AddAsync(new Product { Name = "Kalem 1", Barcode = 111, Price = 111, Stock = 111 });
    //await _context.Products.AddAsync(new Product { Name = "Kalem 2", Barcode = 222, Price = 222, Stock = 222 });
    //await _context.Products.AddAsync(new Product { Name = "Kalem 3", Barcode = 333, Price = 333, Stock = 333 });

    //await _context.SaveChangesAsync();
    #endregion

    #region ChangeTracker
    // ChangeTracker EFCore tarafından track edilen verilere erişmemizi sağlar.
    //var products = await _context.Products.AsNoTracking().ToListAsync();

    //products.ForEach(product =>
    //{
    //    Console.WriteLine($"FOREACH\r\nId : {product.Id} \r\nName : {product.Name} \r\nStock : {product.Stock} \r\nPrice : {product.Price}**");
    //});

    //_context.ChangeTracker.Entries().ToList().ForEach(e =>
    //{
    //    if (e.Entity is Product product)
    //    {
    //        Console.WriteLine($"CHANGE_TRACKER\r\nId : {product.Id} \r\nName : {product.Name} \r\nStock : {product.Stock} \r\nPrice : {product.Price}**");
    //    }
    //});
    //await _context.SaveChangesAsync();
    #endregion

    #region Track edilmeyen entity için Update()
    //_context.Update(new Product { Id = 3, Name = "3.Kalem", Barcode = 324, Price = 324, Stock = 324 });
    //await _context.SaveChangesAsync();
    #endregion

    #region States
    #region Detached
    //var product = await _context.Products.FirstAsync();
    //Console.WriteLine($"State Before Update : {_context.Entry(product).State}");

    //_context.Entry(product).State = EntityState.Detached; // entitynin takip edilmemesini sağlar.

    //product.Name = "Pilot Kalem";
    //Console.WriteLine($"State After Remove : {_context.Entry(product).State}");

    //await _context.SaveChangesAsync();
    //Console.WriteLine($"State After SaveChangesAsync : {_context.Entry(product).State}");
    #endregion

    #region State After Remove()
    //var product = await _context.Products.FirstAsync();
    //Console.WriteLine($"State Before Remove : {_context.Entry(product).State}");

    //_context.Remove(product);
    ////_context.Entry(product).State = EntityState.Deleted;
    //Console.WriteLine($"State After Remove : {_context.Entry(product).State}");

    //await _context.SaveChangesAsync();
    //Console.WriteLine($"State After SaveChangesAsync : {_context.Entry(product).State}");
    #endregion

    #region State After Update()
    //var product = await _context.Products.FirstAsync();
    //Console.WriteLine($"State Before Update : {_context.Entry(product).State}");

    //product.Price = 400;
    //Console.WriteLine($"State After Update : {_context.Entry(product).State}");

    //await _context.SaveChangesAsync();
    //Console.WriteLine($"State After SaveChangesAsync : {_context.Entry(product).State}");
    #endregion

    #region State After AddAsync()
    //var newProduct = new Product { Name = "Kalem 200", Price = 200, Stock = 200, Barcode = 333 };
    //Console.WriteLine($"State Before AddAsync : {_context.Entry(newProduct).State}");
    //// Henüz memoryde olmayan veriler için state : Detached

    //// AddAsync() kodunun yaptığı işlem aynı şekilde şöyle yazılabilir.
    //// _context.Entry(newProduct).State = EntityState.Added;
    //await _context.AddAsync(newProduct);
    //Console.WriteLine($"State After AddAsync : {_context.Entry(newProduct).State}");

    //await _context.SaveChangesAsync();
    //Console.WriteLine($"State After SaveChangesAsync : {_context.Entry(newProduct).State}");
    #endregion
    #endregion

    #region Listing Products
    //var products = await _context.Products.AsNoTracking().ToListAsync();
    ////Aldığımız veride herhangi değişiklik yapılmayacaksa AsNoTracking() metodunu çağırmalıyız.
    //products.ForEach(product =>
    //{
    //    var state = _context.Entry(product).State;
    //    string info = _context.Products.Any() ? $"Id : {product.Id} \r\nName : {product.Name} \r\nStock : {product.Stock} \r\nPrice : {product.Price} \r\nPrice : {state} **\r\n" : "No products found";
    //    Console.WriteLine(info);
    //});
    #endregion
}


string FormatPhone(string phone) => phone.Substring(1, phone.Length - 1);
