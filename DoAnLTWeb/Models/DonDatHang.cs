namespace DoAnLTWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DonDatHang")]
    public partial class DonDatHang
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DonDatHang()
        {
            ChiTietDonDHs = new HashSet<ChiTietDonDH>();
            ChiTietTrangThais = new HashSet<ChiTietTrangThai>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaDon { get; set; }

        [Column(TypeName = "date")]
        public DateTime? NgayDat { get; set; }

        public double? TongTien { get; set; }

        public double? PhiShip { get; set; }

        public double? ThueVAT { get; set; }

        public int? MaKH { get; set; }

        public int? MaHTTT { get; set; }

        public int? MaDCGH { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChiTietDonDH> ChiTietDonDHs { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChiTietTrangThai> ChiTietTrangThais { get; set; }

        public virtual DiaChiGiaoHang DiaChiGiaoHang { get; set; }

        public virtual HinhThucThanhToan HinhThucThanhToan { get; set; }

        public virtual KhachHang KhachHang { get; set; }
    }
}
