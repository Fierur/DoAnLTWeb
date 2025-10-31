using DoAnLTWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAnLTWeb.Controllers
{
    public class OrderController : Controller
    {
        private Model1 db = new Model1();

        // GET: Order/Checkout - Hiển thị trang thanh toán
        public ActionResult Checkout()
        {
            // Kiểm tra đăng nhập
            if (Session["MaKH"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để thanh toán!";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout") });
            }

            int maKH = (int)Session["MaKH"];
            var khachHang = db.KhachHangs.Find(maKH);

            if (khachHang == null || khachHang.MaGH == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng!";
                return RedirectToAction("Index", "Sach");
            }

            // Lấy giỏ hàng
            var gioHang = db.GioHangs.Find(khachHang.MaGH);
            if (gioHang == null)
            {
                TempData["Error"] = "Giỏ hàng không tồn tại!";
                return RedirectToAction("Index", "Sach");
            }

            // Lấy danh sách sản phẩm trong giỏ hàng
            var chiTietGH = db.ChiTietGHs
                .Where(ct => ct.MaGH == gioHang.MaGH)
                .ToList();

            if (!chiTietGH.Any())
            {
                TempData["Error"] = "Giỏ hàng trống! Vui lòng thêm sản phẩm trước khi thanh toán.";
                return RedirectToAction("Cart", "Cart");
            }

            // Tính tổng tiền sản phẩm
            double tongTienSanPham = 0;
            foreach (var item in chiTietGH)
            {
                var sach = db.Saches.Find(item.MaSach);
                if (sach != null && sach.Gia.HasValue && item.SoLuongSachCTGH.HasValue)
                {
                    tongTienSanPham += sach.Gia.Value * item.SoLuongSachCTGH.Value;
                }
            }

            // Lấy danh sách địa chỉ đã lưu của khách hàng
            var diaChiList = db.DiaChiGiaoHangs
                .Where(dc => dc.MaKH == maKH)
                .ToList();

            // Lấy danh sách thành phố, quận huyện, xã phường
            ViewBag.ThanhPhoList = new SelectList(db.ThanhPhoes.OrderBy(t => t.TenTP), "MaTP", "TenTP");
            ViewBag.HinhThucThanhToanList = new SelectList(db.HinhThucThanhToans, "MaHTTT", "TenHTTT");

            // Truyền dữ liệu sang View
            ViewBag.ChiTietGH = chiTietGH;
            ViewBag.TongTienSanPham = tongTienSanPham;
            ViewBag.DiaChiList = diaChiList;
            ViewBag.KhachHang = khachHang;

            return View();
        }

        // GET: Order/GetQuanHuyen - Lấy quận huyện theo thành phố (AJAX)
        [HttpGet]
        public JsonResult GetQuanHuyen(int maTP)
        {
            var quanHuyenList = db.QuanHuyens
                .Where(qh => qh.MaTP == maTP)
                .Select(qh => new
                {
                    MaQH = qh.MaQH,
                    TenQH = qh.TenQH
                })
                .OrderBy(qh => qh.TenQH)
                .ToList();

            return Json(quanHuyenList, JsonRequestBehavior.AllowGet);
        }

        // GET: Order/GetXaPhuong - Lấy xã phường theo quận huyện (AJAX)
        [HttpGet]
        public JsonResult GetXaPhuong(int maQH)
        {
            var xaPhuongList = db.XaPhuongs
                .Where(xp => xp.MaQH == maQH)
                .Select(xp => new
                {
                    MaXP = xp.MaXP,
                    TenXP = xp.TenXP,
                    ChiPhiGHXP = xp.ChiPhiGHXP
                })
                .OrderBy(xp => xp.TenXP)
                .ToList();

            return Json(xaPhuongList, JsonRequestBehavior.AllowGet);
        }

        // POST: Order/PlaceOrder - Đặt hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(
            int? diaChiCoSanId,
            string hoTenNN,
            string soNha,
            string ghiChu,
            int? maXP,
            int maHTTT)
        {
            // Kiểm tra đăng nhập
            if (Session["MaKH"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập!";
                return RedirectToAction("Login", "Account");
            }

            int maKH = (int)Session["MaKH"];
            var khachHang = db.KhachHangs.Find(maKH);

            if (khachHang == null || khachHang.MaGH == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng!";
                return RedirectToAction("Checkout");
            }

            // Lấy giỏ hàng
            var gioHang = db.GioHangs.Find(khachHang.MaGH);
            if (gioHang == null)
            {
                TempData["Error"] = "Giỏ hàng không tồn tại!";
                return RedirectToAction("Checkout");
            }

            // Lấy chi tiết giỏ hàng
            var chiTietGH = db.ChiTietGHs
                .Where(ct => ct.MaGH == gioHang.MaGH)
                .ToList();

            if (!chiTietGH.Any())
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return RedirectToAction("Checkout");
            }

            // Xác định địa chỉ giao hàng
            int maDCGH;
            double phiShip = 0;

            if (diaChiCoSanId.HasValue && diaChiCoSanId.Value > 0)
            {
                // Sử dụng địa chỉ có sẵn
                maDCGH = diaChiCoSanId.Value;
                var diaChi = db.DiaChiGiaoHangs.Find(maDCGH);
                if (diaChi == null || diaChi.MaKH != maKH)
                {
                    TempData["Error"] = "Địa chỉ không hợp lệ!";
                    return RedirectToAction("Checkout");
                }

                // Tính phí ship từ địa chỉ có sẵn
                if (diaChi.MaXP.HasValue)
                {
                    var xaPhuong = db.XaPhuongs.Find(diaChi.MaXP.Value);
                    if (xaPhuong != null && xaPhuong.ChiPhiGHXP.HasValue)
                    {
                        phiShip = xaPhuong.ChiPhiGHXP.Value;
                    }
                }
            }
            else
            {
                // Tạo địa chỉ mới
                if (string.IsNullOrEmpty(hoTenNN) || string.IsNullOrEmpty(soNha) || !maXP.HasValue)
                {
                    TempData["Error"] = "Vui lòng nhập đầy đủ thông tin địa chỉ giao hàng!";
                    return RedirectToAction("Checkout");
                }

                // Tính phí ship từ xã phường mới
                var xaPhuong = db.XaPhuongs.Find(maXP.Value);
                if (xaPhuong != null && xaPhuong.ChiPhiGHXP.HasValue)
                {
                    phiShip = xaPhuong.ChiPhiGHXP.Value;
                }

                // Tạo mã địa chỉ mới
                var maxMaDCGH = db.DiaChiGiaoHangs.Any() ? db.DiaChiGiaoHangs.Max(dc => dc.MaDCGH) : 700;
                var diaChiMoi = new DiaChiGiaoHang
                {
                    MaDCGH = maxMaDCGH + 1,
                    HoTenNN = hoTenNN,
                    SoNha = soNha,
                    GhiChu = ghiChu,
                    MaXP = maXP.Value,
                    MaKH = maKH
                };

                db.DiaChiGiaoHangs.Add(diaChiMoi);
                db.SaveChanges();

                maDCGH = diaChiMoi.MaDCGH;
            }

            // Tính tổng tiền sản phẩm
            double tongTienSanPham = 0;
            foreach (var item in chiTietGH)
            {
                var sach = db.Saches.Find(item.MaSach);
                if (sach != null && sach.Gia.HasValue && item.SoLuongSachCTGH.HasValue)
                {
                    tongTienSanPham += sach.Gia.Value * item.SoLuongSachCTGH.Value;
                }
            }

            // Tính thuế VAT (5%)
            double thueVAT = 0.05;
            double tienThue = tongTienSanPham * thueVAT;
            double tongTien = tongTienSanPham + phiShip + tienThue;

            // Tạo đơn hàng mới
            var maxMaDon = db.DonDatHangs.Any() ? db.DonDatHangs.Max(d => d.MaDon) : 900;
            var donDatHang = new DonDatHang
            {
                MaDon = maxMaDon + 1,
                NgayDat = DateTime.Now,
                TongTien = tongTien,
                PhiShip = phiShip,
                ThueVAT = thueVAT,
                MaKH = maKH,
                MaHTTT = maHTTT,
                MaDCGH = maDCGH
            };

            db.DonDatHangs.Add(donDatHang);

            // Thêm chi tiết đơn hàng
            foreach (var item in chiTietGH)
            {
                var sach = db.Saches.Find(item.MaSach);
                if (sach != null && item.SoLuongSachCTGH.HasValue)
                {
                    var chiTietDonDH = new ChiTietDonDH
                    {
                        MaDon = donDatHang.MaDon,
                        MaSach = item.MaSach,
                        SoLuongCTDDH = item.SoLuongSachCTGH.Value,
                        DonGiaCTDDH = sach.Gia ?? 0
                    };
                    db.ChiTietDonDHs.Add(chiTietDonDH);
                }
            }

            // Thêm trạng thái đơn hàng (Chờ xác nhận)
            var chiTietTrangThai = new ChiTietTrangThai
            {
                MaDon = donDatHang.MaDon,
                MaTT = 1001, // Chờ xác nhận
                NgayCapNhatTT = DateTime.Now,
                GhiChuTT = "Đơn hàng mới được tạo"
            };
            db.ChiTietTrangThais.Add(chiTietTrangThai);

            // Xóa giỏ hàng
            db.ChiTietGHs.RemoveRange(chiTietGH);

            // Lưu thay đổi
            db.SaveChanges();

            // Lưu mã đơn hàng vào TempData để hiển thị ở trang Success
            TempData["MaDonHang"] = donDatHang.MaDon;
            TempData["Success"] = "Đặt hàng thành công!";

            return RedirectToAction("Success", new { maDon = donDatHang.MaDon });
        }

        // GET: Order/Success - Trang đặt hàng thành công
        public ActionResult Success(int maDon)
        {
            // Kiểm tra đăng nhập
            if (Session["MaKH"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int maKH = (int)Session["MaKH"];

            // Lấy thông tin đơn hàng
            var donHang = db.DonDatHangs
                .FirstOrDefault(d => d.MaDon == maDon && d.MaKH == maKH);

            if (donHang == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("Index", "Sach");
            }

            // Lấy chi tiết đơn hàng
            var chiTietDonHang = db.ChiTietDonDHs
                .Where(ct => ct.MaDon == maDon)
                .ToList();

            ViewBag.DonHang = donHang;
            ViewBag.ChiTietDonHang = chiTietDonHang;

            return View();
        }

        // GET: Order/MyOrders - Danh sách đơn hàng của khách hàng
        public ActionResult MyOrders()
        {
            // Kiểm tra đăng nhập
            if (Session["MaKH"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem đơn hàng!";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("MyOrders") });
            }

            int maKH = (int)Session["MaKH"];

            // Lấy danh sách đơn hàng, sắp xếp từ mới đến cũ
            var donHangList = db.DonDatHangs
                .Where(d => d.MaKH == maKH)
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            return View(donHangList);
        }

        // GET: Order/OrderDetail - Chi tiết đơn hàng
        public ActionResult OrderDetail(int id)
        {
            // Kiểm tra đăng nhập
            if (Session["MaKH"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập!";
                return RedirectToAction("Login", "Account");
            }

            int maKH = (int)Session["MaKH"];

            // Lấy thông tin đơn hàng
            var donHang = db.DonDatHangs
                .FirstOrDefault(d => d.MaDon == id && d.MaKH == maKH);

            if (donHang == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("MyOrders");
            }

            // Lấy chi tiết đơn hàng
            var chiTietDonHang = db.ChiTietDonDHs
                .Where(ct => ct.MaDon == id)
                .ToList();

            // Lấy lịch sử trạng thái
            var lichSuTrangThai = db.ChiTietTrangThais
                .Where(ct => ct.MaDon == id)
                .OrderBy(ct => ct.NgayCapNhatTT)
                .ToList();

            ViewBag.DonHang = donHang;
            ViewBag.ChiTietDonHang = chiTietDonHang;
            ViewBag.LichSuTrangThai = lichSuTrangThai;

            return View();
        }

        // POST: Order/CancelOrder - Hủy đơn hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelOrder(int maDon, string lyDoHuy)
        {
            // Kiểm tra đăng nhập
            if (Session["MaKH"] == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            int maKH = (int)Session["MaKH"];

            // Lấy đơn hàng
            var donHang = db.DonDatHangs
                .FirstOrDefault(d => d.MaDon == maDon && d.MaKH == maKH);

            if (donHang == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
            }

            // Kiểm tra trạng thái đơn hàng (chỉ hủy được khi đang "Chờ xác nhận")
            var trangThaiHienTai = db.ChiTietTrangThais
                .Where(ct => ct.MaDon == maDon)
                .OrderByDescending(ct => ct.NgayCapNhatTT)
                .FirstOrDefault();

            if (trangThaiHienTai == null || trangThaiHienTai.MaTT != 1001)
            {
                return Json(new { success = false, message = "Chỉ có thể hủy đơn hàng đang 'Chờ xác nhận'!" });
            }

            // Cập nhật trạng thái thành "Đã hủy"
            var trangThaiHuy = new ChiTietTrangThai
            {
                MaDon = maDon,
                MaTT = 1004, // Đã hủy
                NgayCapNhatTT = DateTime.Now,
                GhiChuTT = string.IsNullOrEmpty(lyDoHuy) ? "Khách hàng hủy đơn" : lyDoHuy
            };

            db.ChiTietTrangThais.Add(trangThaiHuy);
            db.SaveChanges();

            return Json(new { success = true, message = "Hủy đơn hàng thành công!" });
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