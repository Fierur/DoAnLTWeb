namespace DoAnLTWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("QuanHuyen")]
    public partial class QuanHuyen
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public QuanHuyen()
        {
            XaPhuongs = new HashSet<XaPhuong>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaQH { get; set; }

        [StringLength(200)]
        public string TenQH { get; set; }

        public int? MaTP { get; set; }

        public virtual ThanhPho ThanhPho { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<XaPhuong> XaPhuongs { get; set; }
    }
}
