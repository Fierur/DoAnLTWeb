using DoAnLTWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAnLTWeb.Controllers
{
    public class SearchController : Controller
    {
        private Model1 db = new Model1();

        // GET: Search
        public ActionResult Index(string searchString, int? theLoaiId, string sortOrder, decimal? minPrice, decimal? maxPrice)
        {
            // Lấy danh sách thể loại cho sidebar
            ViewBag.TheLoaiList = db.TheLoaiSaches
                .Where(t => t.MaTLS != 9 && t.MaTLS != 10)
                .OrderBy(t => t.TenTLS)
                .ToList();

            // Lưu các tham số tìm kiếm để hiển thị lại
            ViewBag.SearchString = searchString;
            ViewBag.TheLoaiId = theLoaiId;
            ViewBag.SortOrder = sortOrder;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            // Lấy danh sách sách
            var sachQuery = db.Saches.AsQueryable();

            // Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                sachQuery = sachQuery.Where(s =>
                    s.TenSach.Contains(searchString) ||
                    s.Mota.Contains(searchString) ||
                    s.TacGias.Any(t => t.HoTenTG.Contains(searchString))
                );
            }

            // Lọc theo thể loại
            if (theLoaiId.HasValue && theLoaiId.Value > 0)
            {
                sachQuery = sachQuery.Where(s => s.MaTLS == theLoaiId.Value);
                var theLoai = db.TheLoaiSaches.Find(theLoaiId.Value);
                if (theLoai != null)
                {
                    ViewBag.TenTheLoai = theLoai.TenTLS;
                }
            }

            // Lọc theo khoảng giá
            if (minPrice.HasValue)
            {
                sachQuery = sachQuery.Where(s => s.Gia >= (double)minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                sachQuery = sachQuery.Where(s => s.Gia <= (double)maxPrice.Value);
            }

            // Sắp xếp
            switch (sortOrder)
            {
                case "price_asc":
                    sachQuery = sachQuery.OrderBy(s => s.Gia);
                    ViewBag.SortName = "Giá: Thấp đến Cao";
                    break;
                case "price_desc":
                    sachQuery = sachQuery.OrderByDescending(s => s.Gia);
                    ViewBag.SortName = "Giá: Cao đến Thấp";
                    break;
                case "name_asc":
                    sachQuery = sachQuery.OrderBy(s => s.TenSach);
                    ViewBag.SortName = "Tên: A-Z";
                    break;
                case "name_desc":
                    sachQuery = sachQuery.OrderByDescending(s => s.TenSach);
                    ViewBag.SortName = "Tên: Z-A";
                    break;
                default:
                    sachQuery = sachQuery.OrderBy(s => s.TenSach);
                    ViewBag.SortName = "Mặc định";
                    break;
            }

            var dsSach = sachQuery.ToList();
            ViewBag.TotalResults = dsSach.Count;

            return View(dsSach);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}