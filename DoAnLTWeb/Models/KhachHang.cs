namespace DoAnLTWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("KhachHang")]
    public partial class KhachHang
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public KhachHang()
        {
            DiaChiGiaoHangs = new HashSet<DiaChiGiaoHang>();
            DonDatHangs = new HashSet<DonDatHang>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaKH { get; set; }

        [StringLength(200)]
        public string HoTenKH { get; set; }

        [StringLength(200)]
        public string email { get; set; }

        [StringLength(11)]
        public string SDT { get; set; }

        [StringLength(200)]
        public string DiaChiKH { get; set; }

        [StringLength(200)]
        public string MatKhau { get; set; }

        public int? MaGH { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DiaChiGiaoHang> DiaChiGiaoHangs { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DonDatHang> DonDatHangs { get; set; }

        public virtual GioHang GioHang { get; set; }
    }
}
