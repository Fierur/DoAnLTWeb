namespace DoAnLTWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DiaChiGiaoHang")]
    public partial class DiaChiGiaoHang
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DiaChiGiaoHang()
        {
            DonDatHangs = new HashSet<DonDatHang>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaDCGH { get; set; }

        [StringLength(200)]
        public string HoTenNN { get; set; }

        [StringLength(200)]
        public string SoNha { get; set; }

        [StringLength(200)]
        public string GhiChu { get; set; }

        public int? MaXP { get; set; }

        public int? MaKH { get; set; }

        public virtual KhachHang KhachHang { get; set; }

        public virtual XaPhuong XaPhuong { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DonDatHang> DonDatHangs { get; set; }
    }
}
