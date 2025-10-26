namespace DoAnLTWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ChiTietTrangThai")]
    public partial class ChiTietTrangThai
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaDon { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaTT { get; set; }

        [Column(TypeName = "date")]
        public DateTime? NgayCapNhatTT { get; set; }

        [StringLength(510)]
        public string GhiChuTT { get; set; }

        public virtual DonDatHang DonDatHang { get; set; }

        public virtual TrangThai TrangThai { get; set; }
    }
}
