using DoAnLTWeb.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAnLTWeb.Controllers
{
    public class SachController : Controller
    {
        private Model1 db = new Model1();

        // GET: Sach/Index
        public ActionResult Index(string searchString, int? theLoaiId)
        {
            // Lấy danh sách thể loại cho menu
            GetTheLoaiMenu();

            // Lấy danh sách sách
            var dsSach = db.Saches.AsQueryable();

            // Tìm kiếm theo tên sách
            if (!string.IsNullOrEmpty(searchString))
            {
                dsSach = dsSach.Where(s => s.TenSach.Contains(searchString));
                ViewBag.SearchString = searchString;
            }

            // Lọc theo thể loại
            if (theLoaiId.HasValue)
            {
                dsSach = dsSach.Where(s => s.MaTLS == theLoaiId.Value);
                ViewBag.TheLoaiId = theLoaiId.Value;

                // Lấy tên thể loại đang lọc
                var theLoai = db.TheLoaiSaches.Find(theLoaiId.Value);
                if (theLoai != null)
                {
                    ViewBag.TenTheLoai = theLoai.TenTLS;
                }
            }

            // Sắp xếp theo tên sách
            dsSach = dsSach.OrderBy(s => s.TenSach);

            return View(dsSach.ToList());
        }

        // GET: Sach/Details/5
        public ActionResult Details(int id)
        {
            var sach = db.Saches.Find(id);
            if (sach == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách sách liên quan (cùng thể loại, khác mã sách)
            ViewBag.SachLienQuan = db.Saches
                .Where(s => s.MaTLS == sach.MaTLS && s.MaSach != sach.MaSach)
                .OrderBy(s => s.TenSach)
                .Take(4)
                .ToList();

            return View(sach);
        }

        private void GetTheLoaiMenu()
        {
            // Lấy tất cả thể loại
            var allTheLoai = db.TheLoaiSaches.OrderBy(t => t.MaTLS).ToList();

            // Tạo danh sách menu với cấu trúc đa cấp
            var menuList = new List<TheLoaiMenuModel>();

            // Định nghĩa thể loại cha "Truyện Cổ Tích" có MaTLS = 5
            int truyenCoTichId = 5;

            // Định nghĩa các thể loại con của "Truyện Cổ Tích"
            var truyenCoTichConIds = new List<int> { 9, 10 }; // 9: Việt Nam, 10: Thế Giới

            foreach (var tl in allTheLoai)
            {
                // Bỏ qua thể loại con, chúng sẽ được thêm vào children của cha
                if (truyenCoTichConIds.Contains(tl.MaTLS))
                {
                    continue;
                }

                var model = new TheLoaiMenuModel
                {
                    TheLoai = tl,
                    Children = new List<TheLoaiSach>()
                };

                // Nếu là "Truyện Cổ Tích", thêm các thể loại con
                if (tl.MaTLS == truyenCoTichId)
                {
                    model.Children = allTheLoai
                        .Where(t => truyenCoTichConIds.Contains(t.MaTLS))
                        .ToList();
                }

                menuList.Add(model);
            }
            Debug.WriteLine($"MenuList có {menuList.Count} thể loại.");
            ViewBag.TheLoaiMenu = menuList;
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

    // Model helper cho menu đa cấp
    public class TheLoaiMenuModel
    {
        public TheLoaiSach TheLoai { get; set; }
        public List<TheLoaiSach> Children { get; set; }
    }
}