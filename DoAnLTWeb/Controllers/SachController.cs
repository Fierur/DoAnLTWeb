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

        // GET: Xem giỏ hàng
        public ActionResult Cart()
        {
            if (Session["MaKH"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem giỏ hàng!";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Cart", "Sach") });
            }

            int maKH = (int)Session["MaKH"];
            var khachHang = db.KhachHangs.Find(maKH);

            if (khachHang == null || !khachHang.MaGH.HasValue)
            {
                ViewBag.CartItems = new List<ChiTietGH>();
                ViewBag.TongTien = 0;
                return View();
            }

            var cartItems = db.ChiTietGHs
                .Where(c => c.MaGH == khachHang.MaGH.Value)
                .ToList();

            // Tính tổng tiền
            double tongTien = 0;
            foreach (var item in cartItems)
            {
                var sach = db.Saches.Find(item.MaSach);
                if (sach != null && sach.Gia.HasValue && item.SoLuongSachCTGH.HasValue)
                {
                    tongTien += sach.Gia.Value * item.SoLuongSachCTGH.Value;
                }
            }

            ViewBag.CartItems = cartItems;
            ViewBag.TongTien = tongTien;
            return View();
        }

        // POST: Thêm vào giỏ hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddToCart(int bookId, int quantity = 1)
        {
            try
            {
                // Kiểm tra đăng nhập
                if (Session["MaKH"] == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập", requireLogin = true });
                }

                int maKH = (int)Session["MaKH"];
                var khachHang = db.KhachHangs.Find(maKH);

                if (khachHang == null || !khachHang.MaGH.HasValue)
                {
                    return Json(new { success = false, message = "Không tìm thấy giỏ hàng!" });
                }

                // Kiểm tra sách có tồn tại không
                var sach = db.Saches.Find(bookId);
                if (sach == null)
                {
                    return Json(new { success = false, message = "Sách không tồn tại!" });
                }

                int maGH = khachHang.MaGH.Value;

                // Kiểm tra sách đã có trong giỏ chưa
                var existingItem = db.ChiTietGHs
                    .FirstOrDefault(c => c.MaGH == maGH && c.MaSach == bookId);

                if (existingItem != null)
                {
                    // Cập nhật số lượng
                    existingItem.SoLuongSachCTGH = (existingItem.SoLuongSachCTGH ?? 0) + quantity;
                }
                else
                {
                    // Thêm mới
                    var chiTiet = new ChiTietGH
                    {
                        MaGH = maGH,
                        MaSach = bookId,
                        SoLuongSachCTGH = quantity
                    };
                    db.ChiTietGHs.Add(chiTiet);
                }

                // Cập nhật ngày update giỏ hàng
                var gioHang = db.GioHangs.Find(maGH);
                if (gioHang != null)
                {
                    gioHang.NgayUpdateCuoiGH = DateTime.Now;
                }

                db.SaveChanges();

                // Đếm tổng số lượng sản phẩm trong giỏ
                //int cartCount = db.ChiTietGHs
                //    .Where(c => c.MaGH == maGH)
                //    .Sum(c => c.SoLuongSachCTGH ?? 0);
                int cartCount = db.ChiTietGHs
                    .Where(c => c.MaGH == maGH)
                    .Select(c => c.SoLuongSachCTGH)
                    .DefaultIfEmpty(0)
                    .Sum() ?? 0;

                return Json(new { success = true, cartCount = cartCount, message = "Đã thêm vào giỏ hàng!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Xóa khỏi giỏ hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RemoveFromCart(int bookId)
        {
            try
            {
                if (Session["MaKH"] == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập!" });
                }

                int maKH = (int)Session["MaKH"];
                var khachHang = db.KhachHangs.Find(maKH);

                if (khachHang == null || !khachHang.MaGH.HasValue)
                {
                    return Json(new { success = false, message = "Không tìm thấy giỏ hàng!" });
                }

                int maGH = khachHang.MaGH.Value;
                var item = db.ChiTietGHs
                    .FirstOrDefault(c => c.MaGH == maGH && c.MaSach == bookId);

                if (item == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không có trong giỏ hàng!" });
                }

                db.ChiTietGHs.Remove(item);
                db.SaveChanges();

                // Cập nhật số lượng giỏ hàng
                //int cartCount = db.ChiTietGHs
                //    .Where(c => c.MaGH == maGH)
                //    .Sum(c => c.SoLuongSachCTGH ?? 0);
                int cartCount = db.ChiTietGHs
                    .Where(c => c.MaGH == maGH)
                    .Select(c => c.SoLuongSachCTGH)
                    .DefaultIfEmpty(0)
                    .Sum() ?? 0;

                return Json(new { success = true, cartCount = cartCount, message = "Đã xóa khỏi giỏ hàng!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Cập nhật số lượng trong giỏ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateCartQuantity(int bookId, int quantity)
        {
            try
            {
                if (Session["MaKH"] == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập!" });
                }

                if (quantity < 0)
                {
                    return Json(new { success = false, message = "Số lượng không hợp lệ!" });
                }

                int maKH = (int)Session["MaKH"];
                var khachHang = db.KhachHangs.Find(maKH);

                if (khachHang == null || !khachHang.MaGH.HasValue)
                {
                    return Json(new { success = false, message = "Không tìm thấy giỏ hàng!" });
                }

                int maGH = khachHang.MaGH.Value;
                var item = db.ChiTietGHs
                    .FirstOrDefault(c => c.MaGH == maGH && c.MaSach == bookId);

                if (item == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không có trong giỏ hàng!" });
                }

                if (quantity == 0)
                {
                    // Nếu số lượng = 0, xóa sản phẩm
                    db.ChiTietGHs.Remove(item);
                }
                else
                {
                    // Cập nhật số lượng
                    item.SoLuongSachCTGH = quantity;
                }

                db.SaveChanges();

                // Tính lại tổng tiền
                var sach = db.Saches.Find(bookId);
                double itemTotal = (sach?.Gia ?? 0) * quantity;

                // Tính tổng tiền giỏ hàng
                var cartItems = db.ChiTietGHs.Where(c => c.MaGH == maGH).ToList();
                double cartTotal = 0;
                foreach (var cartItem in cartItems)
                {
                    var s = db.Saches.Find(cartItem.MaSach);
                    if (s != null && s.Gia.HasValue && cartItem.SoLuongSachCTGH.HasValue)
                    {
                        cartTotal += s.Gia.Value * cartItem.SoLuongSachCTGH.Value;
                    }
                }

                // Đếm tổng số lượng
                int cartCount = cartItems.Sum(c => c.SoLuongSachCTGH ?? 0);

                return Json(new
                {
                    success = true,
                    cartCount = cartCount,
                    itemTotal = itemTotal,
                    cartTotal = cartTotal,
                    message = "Đã cập nhật giỏ hàng!"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Lấy số lượng giỏ hàng (cho AJAX)
        public JsonResult GetCartCount()
        {
            try
            {
                if (Session["MaKH"] == null)
                {
                    return Json(new { success = true, cartCount = 0 }, JsonRequestBehavior.AllowGet);
                }

                int maKH = (int)Session["MaKH"];
                var khachHang = db.KhachHangs.Find(maKH);

                if (khachHang == null || !khachHang.MaGH.HasValue)
                {
                    return Json(new { success = true, cartCount = 0 }, JsonRequestBehavior.AllowGet);
                }

                int cartCount = db.ChiTietGHs
                    .Where(c => c.MaGH == khachHang.MaGH.Value)
                    .Select(c => c.SoLuongSachCTGH)
                    .DefaultIfEmpty(0)
                    .Sum() ?? 0;

                return Json(new { success = true, cartCount = cartCount }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = true, cartCount = 0 }, JsonRequestBehavior.AllowGet);
            }
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