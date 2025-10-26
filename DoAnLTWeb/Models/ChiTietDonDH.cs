namespace DoAnLTWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ChiTietDonDH")]
    public partial class ChiTietDonDH
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaDon { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaSach { get; set; }

        public int? SoLuongCTDDH { get; set; }

        public double? DonGiaCTDDH { get; set; }

        public virtual DonDatHang DonDatHang { get; set; }

        public virtual Sach Sach { get; set; }
    }
}
