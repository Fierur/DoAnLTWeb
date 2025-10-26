namespace DoAnLTWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("NhanVien")]
    public partial class NhanVien
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaNV { get; set; }

        [StringLength(60)]
        public string HoTenNV { get; set; }

        [StringLength(11)]
        public string SDTNV { get; set; }

        [StringLength(200)]
        public string DiaChiNV { get; set; }

        [StringLength(200)]
        public string MatKhauNV { get; set; }

        [StringLength(5)]
        public string GioiTinhNV { get; set; }

        [StringLength(200)]
        public string Email { get; set; }
    }
}
