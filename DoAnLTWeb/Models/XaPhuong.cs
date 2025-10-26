namespace DoAnLTWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("XaPhuong")]
    public partial class XaPhuong
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public XaPhuong()
        {
            DiaChiGiaoHangs = new HashSet<DiaChiGiaoHang>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaXP { get; set; }

        [StringLength(200)]
        public string TenXP { get; set; }

        public double? ChiPhiGHXP { get; set; }

        public int? MaQH { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DiaChiGiaoHang> DiaChiGiaoHangs { get; set; }

        public virtual QuanHuyen QuanHuyen { get; set; }
    }
}
