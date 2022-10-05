using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeFirst.Dal
{
    //[Table("Ürünler Tablosu", Schema ="Ürünler")]
    //[Index(nameof(Name))]
    public class Product
    {
        #region Configuration
        //Primary Key belirtmek için kullanılır.
        //[Key]
        //public int Product_Id { get; set; } 
        #endregion
        public int Id { get; set; }

        #region Configuration
        // Order : Tablodaki sütun sırasını temsil eder.
        //[Column("İsim", TypeName = "nvarchar", Order = 1)] 

        // Referans tipteki değerlerin null olmamasını sağlar.
        //[Required]
        //[MaxLength(100)]
        // 10 karakter girilmek zorunda
        //[StringLength(10, MinimumLength =10)] 
        #endregion
        //[Unicode(false)]
        //[Column(TypeName ="char(200)")]
        public string Name { get; set; }

        #region Configuration
        //[Column("Fiyat", TypeName = "decimal(6,2)")] 
        // Toplam 6 karakter virgülden sonra 2 karakter 
        #endregion
        public decimal Price { get; set; }
        public decimal DiscountPrice { get; set; }
        public int Stock { get; set; }

        //[NotMapped]
        public long Barcode { get; set; }

        #region DatabaseGenerated
        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)] // Hem ekleme hem güncelleme işlemi veri tabanında yapılıyorsa Computed kullanılır.
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Sadece Insert işleminde veri tabanına yansır 
        #endregion
        public DateTime CreatedDate { get; set; }

        public Category Category { get; set; }
        public int? CategoryId { get; set; }

        public ProductFeature ProductFeature { get; set; }

        #region Relation Configuration
        //[ForeignKey("Category_Id")]
        //public Category Category { get; set; }
        //public int Category_Id { get; set; } 
        #endregion
    }
}
