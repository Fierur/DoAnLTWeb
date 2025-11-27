namespace DoAnLTWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Sach")]
    public partial class Sach
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Sach()
        {
            ChiTietDonDHs = new HashSet<ChiTietDonDH>();
            ChiTietGHs = new HashSet<ChiTietGH>();
            TacGias = new HashSet<TacGia>();
            TheLoaiSaches = new HashSet<TheLoaiSach>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaSach { get; set; }

        [StringLength(200)]
        public string TenSach { get; set; }

        public double? Gia { get; set; }

        [StringLength(20)]
        public string AddNgonNgu { get; set; }

        public int? SoTrang { get; set; }

        [StringLength(500)]
        public string Mota { get; set; }

        public int? MaTLS { get; set; }

        public int? MaNXB { get; set; }

        [StringLength(200)]
        public string Hinh { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChiTietDonDH> ChiTietDonDHs { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChiTietGH> ChiTietGHs { get; set; }

        public virtual NhaXuatBan NhaXuatBan { get; set; }

        //public virtual TheLoaiSach TheLoaiSach { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TacGia> TacGias { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TheLoaiSach> TheLoaiSaches { get; set; }
    }
}
