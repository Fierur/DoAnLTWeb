using DoAnLTWeb.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace DoAnLTWeb.Controllers
{
    public class AccountController : Controller
    {
        private Model1 db = new Model1();

        // GET: Account/Login
        public ActionResult Login(string returnUrl)
        {
            // Nếu đã đăng nhập thì chuyển về trang chủ
            if (Session["KhachHang"] != null)
            {
                return RedirectToAction("Index", "Sach");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password, string returnUrl)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ email và mật khẩu!";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // Tìm khách hàng theo email và mật khẩu
            var khachHang = db.KhachHangs.FirstOrDefault(k => k.email == email && k.MatKhau == password);

            if (khachHang != null)
            {
                // Lưu thông tin vào Session
                Session["KhachHang"] = khachHang;
                Session["TenKH"] = khachHang.HoTenKH;
                Session["MaKH"] = khachHang.MaKH;
                Session["Email"] = khachHang.email;

                // Chuyển về trang được yêu cầu hoặc trang chủ
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Sach");
            }
            else
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng!";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            // Nếu đã đăng nhập thì chuyển về trang chủ
            if (Session["KhachHang"] != null)
            {
                return RedirectToAction("Index", "Sach");
            }

            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(KhachHang model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email đã tồn tại chưa
                var existingEmail = db.KhachHangs.FirstOrDefault(k => k.email == model.email);
                if (existingEmail != null)
                {
                    ViewBag.Error = "Email này đã được đăng ký!";
                    return View(model);
                }

                // Tạo mã khách hàng tự động (lấy mã lớn nhất + 1)
                var maxMaKH = db.KhachHangs.Any() ? db.KhachHangs.Max(k => k.MaKH) : 500;
                model.MaKH = maxMaKH + 1;

                // Tạo giỏ hàng mới cho khách hàng
                var maxMaGH = db.GioHangs.Any() ? db.GioHangs.Max(g => g.MaGH) : 400;
                var gioHang = new GioHang
                {
                    MaGH = maxMaGH + 1,
                    NgayUpdateCuoiGH = DateTime.Now
                };

                db.GioHangs.Add(gioHang);
                model.MaGH = gioHang.MaGH;

                // Thêm khách hàng vào database
                db.KhachHangs.Add(model);
                db.SaveChanges();

                // Tự động đăng nhập sau khi đăng ký
                Session["KhachHang"] = model;
                Session["TenKH"] = model.HoTenKH;
                Session["MaKH"] = model.MaKH;
                Session["Email"] = model.email;

                TempData["Success"] = "Đăng ký thành công! Chào mừng bạn đến với hệ thống.";
                return RedirectToAction("Index", "Sach");
            }

            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            // Xóa toàn bộ Session
            Session.Clear();
            Session.Abandon();

            TempData["Success"] = "Đăng xuất thành công!";
            return RedirectToAction("Index", "Sach");
        }

        // Add 'new' keyword to explicitly hide inherited member 'Controller.Profile'
        public new ActionResult Profile()
        {
            // Kiểm tra đăng nhập
            if (Session["MaKH"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem thông tin cá nhân!";
                return RedirectToAction("Login", new { returnUrl = Url.Action("Profile") });
            }

            int maKH = (int)Session["MaKH"];
            var khachHang = db.KhachHangs.Find(maKH);

            if (khachHang == null)
            {
                Session.Clear();
                return RedirectToAction("Login");
            }

            return View(khachHang);
        }

        // GET: Account/Edit
        public ActionResult Edit()
        {
            // Kiểm tra đăng nhập
            if (Session["MaKH"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để chỉnh sửa thông tin!";
                return RedirectToAction("Login", new { returnUrl = Url.Action("Edit") });
            }

            int maKH = (int)Session["MaKH"];
            var khachHang = db.KhachHangs.Find(maKH);

            if (khachHang == null)
            {
                Session.Clear();
                return RedirectToAction("Login");
            }

            return View(khachHang);
        }

        // POST: Account/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(KhachHang model)
        {
            // Kiểm tra đăng nhập
            if (Session["MaKH"] == null)
            {
                return RedirectToAction("Login");
            }

            int maKH = (int)Session["MaKH"];

            if (model.MaKH != maKH)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra email có bị trùng với người khác không
                var existingEmail = db.KhachHangs
                    .FirstOrDefault(k => k.email == model.email && k.MaKH != maKH);

                if (existingEmail != null)
                {
                    ViewBag.Error = "Email này đã được người khác sử dụng!";
                    return View(model);
                }

                // Lấy khách hàng hiện tại từ DB
                var khachHang = db.KhachHangs.Find(maKH);
                if (khachHang == null)
                {
                    return HttpNotFound();
                }

                // Cập nhật thông tin
                khachHang.HoTenKH = model.HoTenKH;
                khachHang.email = model.email;
                khachHang.SDT = model.SDT;
                khachHang.DiaChiKH = model.DiaChiKH;

                // Chỉ cập nhật mật khẩu nếu người dùng nhập mới
                if (!string.IsNullOrEmpty(model.MatKhau))
                {
                    khachHang.MatKhau = model.MatKhau;
                }

                db.SaveChanges();

                // Cập nhật lại Session
                Session["KhachHang"] = khachHang;
                Session["TenKH"] = khachHang.HoTenKH;
                Session["Email"] = khachHang.email;

                TempData["Success"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Profile");
            }

            return View(model);
        }

        // GET: Account/ChangePassword
        public ActionResult ChangePassword()
        {
            // Kiểm tra đăng nhập
            if (Session["MaKH"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để đổi mật khẩu!";
                return RedirectToAction("Login", new { returnUrl = Url.Action("ChangePassword") });
            }

            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            // Kiểm tra đăng nhập
            if (Session["MaKH"] == null)
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu mới và xác nhận mật khẩu không khớp!";
                return View();
            }

            int maKH = (int)Session["MaKH"];
            var khachHang = db.KhachHangs.Find(maKH);

            if (khachHang == null)
            {
                Session.Clear();
                return RedirectToAction("Login");
            }

            // Kiểm tra mật khẩu cũ
            if (khachHang.MatKhau != oldPassword)
            {
                ViewBag.Error = "Mật khẩu cũ không đúng!";
                return View();
            }

            // Cập nhật mật khẩu mới
            khachHang.MatKhau = newPassword;
            db.SaveChanges();

            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Profile");
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