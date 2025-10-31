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
        public ActionResult Index(string searchString, int? theLoaiId, string subCategory)
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
                // Kiểm tra nếu là thể loại "Truyện Cổ Tích" (MaTLS = 5)
                if (theLoaiId.Value == 5)
                {
                    // Nếu có subCategory, lọc theo ngôn ngữ
                    if (!string.IsNullOrEmpty(subCategory))
                    {
                        if (subCategory == "vn")
                        {
                            // Truyện Cổ Tích Việt Nam: MaTLS = 5 VÀ AddNgonNgu = "Việt"
                            dsSach = dsSach.Where(s => s.MaTLS == 5 && s.AddNgonNgu == "Việt");
                            ViewBag.TenTheLoai = "Truyện Cổ Tích Việt Nam";
                        }
                        else if (subCategory == "world")
                        {
                            // Truyện Cổ Tích Thế Giới: MaTLS = 5 VÀ AddNgonNgu != "Việt"
                            dsSach = dsSach.Where(s => s.MaTLS == 5 && s.AddNgonNgu != "Việt");
                            ViewBag.TenTheLoai = "Truyện Cổ Tích Thế Giới";
                        }
                    }
                    else
                    {
                        // Hiển thị tất cả truyện cổ tích
                        dsSach = dsSach.Where(s => s.MaTLS == theLoaiId.Value);
                        var theLoai = db.TheLoaiSaches.Find(theLoaiId.Value);
                        if (theLoai != null)
                        {
                            ViewBag.TenTheLoai = theLoai.TenTLS;
                        }
                    }
                }
                else
                {
                    // Các thể loại khác, lọc bình thường
                    dsSach = dsSach.Where(s => s.MaTLS == theLoaiId.Value);
                    var theLoai = db.TheLoaiSaches.Find(theLoaiId.Value);
                    if (theLoai != null)
                    {
                        ViewBag.TenTheLoai = theLoai.TenTLS;
                    }
                }

                ViewBag.TheLoaiId = theLoaiId.Value;
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
            // Lấy tất cả thể loại (trừ thể loại con không dùng nữa: 9, 10)
            var allTheLoai = db.TheLoaiSaches
                .Where(t => t.MaTLS != 9 && t.MaTLS != 10)
                .OrderBy(t => t.MaTLS)
                .ToList();

            // Tạo danh sách menu với cấu trúc đa cấp
            var menuList = new List<TheLoaiMenuModel>();

            // Định nghĩa thể loại cha "Truyện Cổ Tích" có MaTLS = 5
            int truyenCoTichId = 5;

            foreach (var tl in allTheLoai)
            {
                var model = new TheLoaiMenuModel
                {
                    TheLoai = tl,
                    Children = new List<SubCategoryModel>()
                };

                // Nếu là "Truyện Cổ Tích", thêm các menu con động
                if (tl.MaTLS == truyenCoTichId)
                {
                    model.Children.Add(new SubCategoryModel
                    {
                        TenTheLoai = "Truyện Cổ Tích Việt Nam",
                        SubCategoryKey = "vn",
                        ParentId = truyenCoTichId
                    });
                    model.Children.Add(new SubCategoryModel
                    {
                        TenTheLoai = "Truyện Cổ Tích Thế Giới",
                        SubCategoryKey = "world",
                        ParentId = truyenCoTichId
                    });
                }

                menuList.Add(model);
            }

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
        public List<SubCategoryModel> Children { get; set; }
    }

    // Model cho sub-category
    public class SubCategoryModel
    {
        public string TenTheLoai { get; set; }
        public string SubCategoryKey { get; set; }
        public int ParentId { get; set; }
    }
}