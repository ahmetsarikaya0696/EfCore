using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;

namespace CodeFirst.Dal
{
    public class AppDbContext : DbContext
    {
        #region one-to-many
        /*
         * Category-Product (one-to-many)
         * Category tablosundan bir satır silindiğinde o Category ' e bağlı tüm Productlar default olarak silinir.
         * Sadece Category.cs içine nav prop -public List<Product> Products { get; set; }- yazılıp Product.cs içine herhangi bir 
         navigation prop vermediğimizde Categoryden Products ' a gidebiliyoruz fakat tam tersi geçerli değildir. one-to-many
         ilişkisi yine kurulacaktır.
         * Eğer bir tabloda bir sütun var fakat buna karşılık bir property yoksa bu propertye Shadow Property denir.
         
         * Sadece Product.cs içine nav prop -public Category Category { get; set; }- yazılıp Category.cs içine herhangi bir 
         navigation prop vermediğimizde Product ' tan Category ' e gidebiliyoruz fakat tam tersi geçerli değildir. one-to-many
         ilişkisi yine kurulacaktır.
         
         * Nav proplar her iki classta da verilip FK verilmezse yine ilişki oluşacaktır. FK Shadow property olacaktır.
         Bu durumda direkt Product ekleme işlemi yapılamaz. Category üzerinden yapmak zorundayız.

            var category = _context.Category.First();
            category.Products.Add(new Product()...);
         
         şeklinde eklenecektir. Bu şekilde olmayan bir CategoryId için ekleme probleminin önüne geçilir.
         */
        #endregion

        #region one-to-one
        /*
         * Product parent
         * ProductFeature child
         * FK child classta belirtilmelidir.
         * one-to-one 2de ProductFeature tablosunun FK ' i Product tablosunun Primary Key ' i aynı değer oldu. 
         */
        #endregion

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductFeature> ProductFeatures { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<QueriedProduct> QueriedProducts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Initializer.Build();
            optionsBuilder
                          //.UseLazyLoadingProxies() // Lazy Loading
                          .UseSqlServer(Initializer.Configuration.GetConnectionString("SqlCon"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Precision
            modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(9, 2);
            modelBuilder.Entity<Product>().Property(p => p.DiscountPrice).HasPrecision(9, 2);

            #region Constraints
            modelBuilder.Entity<Product>().HasCheckConstraint("PriceDiscountCheck","[Price]>[DiscountPrice]");

            #endregion

            #region Index
            // Index tablosuna Price ve Stock dahil edilmiş oldu
            modelBuilder.Entity<Product>().HasIndex(p => p.Name)
                                          .IncludeProperties(p => new { p.Price, p.Stock });

            modelBuilder.Entity<Product>().HasIndex(p => new { p.Name, p.Price });
            #endregion

            #region Entity Properties
            //modelBuilder.Entity<Product>().Ignore(p => p.Barcode);
            //modelBuilder.Entity<Product>().Property(p => p.Name).IsUnicode(false); // varchar olarak kullanmak için kullanılır
            //modelBuilder.Entity<Product>().Property(p => p.Name).HasColumnType("nvarchar(200)");
            #endregion

            #region Keyless Entity
            modelBuilder.Entity<QueriedProduct>().HasNoKey();
            #endregion

            #region Owned Entity
            modelBuilder.Entity<Teacher>().OwnsOne(t => t.Person, OwnedNavigationBuilder =>
            {
                OwnedNavigationBuilder.Property(p => p.Name).HasColumnName("Name");
                OwnedNavigationBuilder.Property(p => p.Surname).HasColumnName("Surname");
                OwnedNavigationBuilder.Property(p => p.Age).HasColumnName("Age");
            });

            modelBuilder.Entity<Student>().OwnsOne(s => s.Person, OwnedNavigationBuilder =>
            {
                OwnedNavigationBuilder.Property(p => p.Name).HasColumnName("Name");
                OwnedNavigationBuilder.Property(p => p.Surname).HasColumnName("Surname");
                OwnedNavigationBuilder.Property(p => p.Age).HasColumnName("Age");
            });
            #endregion

            #region DatabaseGeneratedOption.Identity Kullanımı
            // Bu özellik sadece add işleminde veri tabanına yansır.
            modelBuilder.Entity<Product>().Property(p => p.CreatedDate).ValueGeneratedOnAdd();
            #endregion

            #region DatabaseGeneratedOption.Computed Kullanımı
            //// Bu özellik add ve update işlemlerinde bu alanı sorgulara dahil etmez. Add ve Update işlemlerinde önceden bir değerinin olmasını zorunlu kılar.
            //// KDV tutarının SQL tarafında hesaplanmasını sağlar.
            ////modelBuilder.Entity<Product>().Property(p => p.PriceKdv).HasComputedColumnSql("[Price]*[Kdv]"); 
            //modelBuilder.Entity<Product>().Property(p => p.CreatedDate).ValueGeneratedOnAddOrUpdate();
            #endregion

            #region Delete Behaviors
            /*    
             * Cascade : Parent tabloda bir veri silindiğinde default olarak silinen veriye bağlı olan dependent tablodaki veriler de silinir.
             * Restrict : Dependent tabloda Parent tablodaki veriye bağlı bir veri varsa Parent tablodaki verinin silinmesine izin verilmez. Bu durumda Parent tablodaki veriyi silmek istiyorsak önce dependent tablodaki verileri silmemiz gerekir.
             * NoAction : Silme davranışı belirlenmez. Veri tabanında kendimiz belirlememiz gerekir.
             * SetNull : Bu işlemi gerçekleştirebilmek için Dependent tablodaki FK ' in nullable olması gerekir. Ayrıca nullable özelliği açıksa veri referans tip bile olsa not null olarak belirtildiğinden navigation property'inin de nullable olduğu ? kullanarak belirtilmelidir.
             * 
             * Aşağıdaki şekilde Delete Behavior belirtilir.
            */
            modelBuilder.Entity<Category>()
                            .HasMany(c => c.Products)
                            .WithOne(p => p.Category)
                            .HasForeignKey(p => p.CategoryId)
                            .OnDelete(DeleteBehavior.SetNull);
            #endregion

            #region Configuration
            //modelBuilder.Entity<Product>().ToTable("Ürün Tablosu");
            //modelBuilder.Entity<Product>().HasKey(p => p.Product_Id);
            //modelBuilder.Entity<Product>().Property(p => p.Name).IsRequired();
            //modelBuilder.Entity<Product>().Property(p => p.Name).HasMaxLength(100);
            //modelBuilder.Entity<Product>().Property(p => p.Name).HasMaxLength(100).IsFixedLength();
            #endregion

            #region one-to-many Relation Configuration
            //modelBuilder.Entity<Category>()
            //                    .HasMany(c => c.Products)
            //                    .WithOne(p => p.Category)
            //                    .HasForeignKey(p => p.CategoryId);

            #endregion

            #region one-to-one Relation Configuration
            //modelBuilder.Entity<Product>()
            //                    .HasOne(p => p.ProductFeature)
            //                    .WithOne(pf => pf.Product)
            //                    .HasForeignKey<ProductFeature>(pf => pf.ProductId);
            #endregion

            #region one-to-one Relation Configuration 2
            modelBuilder.Entity<Product>()
                                .HasOne(p => p.ProductFeature)
                                .WithOne(pf => pf.Product)
                                .HasForeignKey<ProductFeature>(pf => pf.Id);
            #endregion

            #region many-to-many Relation Configuration
            // many-one-many -İki adet one-to-many ilişkisinin birleştirilmiş hali-
            //modelBuilder.Entity<Teacher>()
            //    .HasMany(t => t.Students)
            //    .WithMany(s => s.Teachers)
            //    .UsingEntity<Dictionary<string, object>>(
            //        "StudentTeacher",
            //        x => x.HasOne<Student>().WithMany().HasForeignKey("TeacherId").HasConstraintName("FK_TeacherId"),
            //        x => x.HasOne<Teacher>().WithMany().HasForeignKey("StudentId").HasConstraintName("FK_StudentId")
            //    );
            #endregion

            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AssignCreatedDate();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            AssignCreatedDate();
            return base.SaveChanges();
        }

        private void AssignCreatedDate()
        {
            this.ChangeTracker.Entries().ToList().ForEach(e =>
            {
                if (e.State == EntityState.Added)
                {
                    if (e.Entity is Product product)
                    {
                        product.CreatedDate = DateTime.Now;
                    }
                }
            });
        }
    }
}
