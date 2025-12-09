using QLI_HoSoBenhAnDienTu_Wed.Controllers;
using QLI_HoSoBenhAnDienTu_Wed.Models;
using QLI_HoSoBenhAnDienTu_Wed.Models.viewsmodel; 
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

public class AccountController : BaseController
{
    public ActionResult Login() => View();

    [HttpPost]
    public ActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = db.tai_khoan.Include("vai_tro")
                     .FirstOrDefault(u => u.ten_dang_nhap == model.Username && u.mat_khau_hash == model.Password);

        if (user != null)
        {
            Session["UserID"] = user.ma_tai_khoan;

            Session["RoleID"] = user.ma_vai_tro;

            Session["RoleName"] = user.vai_tro.ten_vai_tro;

            Session["Avatar"] = user.anh_dai_dien;
            string tenHienThi = user.ten_dang_nhap;
            if (user.benh_nhan.Any()) tenHienThi = user.benh_nhan.FirstOrDefault().ho_ten;
            else if (user.bac_si.Any()) tenHienThi = user.bac_si.FirstOrDefault().ho_ten;
            else if (user.nhan_vien.Any()) tenHienThi = user.nhan_vien.FirstOrDefault().ho_ten;

            Session["HoTen"] = tenHienThi;

            switch (user.vai_tro.ten_vai_tro)
            {
                case "ADMIN": return RedirectToAction("Dashboard", "Admin");
                case "LE_TAN": return RedirectToAction("Dashboard", "LeTan");
                case "BAC_SI": return RedirectToAction("Dashboard", "BacSi");
                case "BENH_NHAN": return RedirectToAction("DatLich", "BenhNhan");
                default: return RedirectToAction("Index", "Home");
            }
        }

        ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
        return View(model);
    }

    public ActionResult Register() => View();

    [HttpPost]
    public ActionResult Register(RegisterViewModel model)
    {
        try
        {
            db.sp_DangKyTaiKhoan(model.Username, model.Password, model.HoTen, model.SDT, model.NgaySinh, model.GioiTinh);
            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            ViewBag.Error = "Lỗi đăng ký: " + msg;
            return View(model);
        }
    }

    public ActionResult Logout()
    {
        Session.Clear(); 
        Session.Abandon(); 
        return RedirectToAction("Login");
    }

    public ActionResult UserProfile()
    {
        if (Session["UserID"] == null) return RedirectToAction("Login");

        long userId = Convert.ToInt64(Session["UserID"]);

        var user = db.tai_khoan.Find(userId);
        if (user == null) return RedirectToAction("Login");

        var model = new UserProfileViewModel
        {
            MaTaiKhoan = user.ma_tai_khoan,
            TenDangNhap = user.ten_dang_nhap,
            Email = user.email,
            SDT = user.so_dien_thoai,
            AnhDaiDien = user.anh_dai_dien,
            TenVaiTro = user.vai_tro.ten_vai_tro
        };

        if (user.benh_nhan.Any())
        {
            var bn = user.benh_nhan.First();
            model.HoTen = bn.ho_ten;
            model.NgaySinh = bn.ngay_sinh;
            model.GioiTinh = bn.gioi_tinh;
            model.DiaChi = bn.dia_chi;
        }
        else if (user.bac_si.Any())
        {
            var bs = user.bac_si.First();
            model.HoTen = bs.ho_ten;
            model.ChuyenKhoa = bs.chuyen_khoa;
            model.GioiThieu = bs.gioi_thieu;
        }
        else if (user.nhan_vien.Any())
        {
            var nv = user.nhan_vien.First();
            model.HoTen = nv.ho_ten;
        }
        else
        {
            model.HoTen = user.ten_dang_nhap; 
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult UpdateProfile(UserProfileViewModel model, HttpPostedFileBase uploadAnh)
    {
        if (Session["UserID"] == null) return RedirectToAction("Login");
        long userId = Convert.ToInt64(Session["UserID"]);
        var user = db.tai_khoan.Find(userId);

        if (ModelState.IsValid)
        {
            try
            {
                user.email = model.Email;
                user.so_dien_thoai = model.SDT;

                if (uploadAnh != null && uploadAnh.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(uploadAnh.FileName);
                    string ext = Path.GetExtension(fileName);
                    string uniqueName = user.ten_dang_nhap + "_" + DateTime.Now.Ticks + ext;

                    string path = Path.Combine(Server.MapPath("~/Content/Images/Avatars/"), uniqueName);

                    Directory.CreateDirectory(Server.MapPath("~/Content/Images/Avatars/"));

                    uploadAnh.SaveAs(path);

                    user.anh_dai_dien = uniqueName;
                }

                if (user.benh_nhan.Any())
                {
                    var bn = user.benh_nhan.First();
                    bn.ho_ten = model.HoTen;
                    if (model.NgaySinh.HasValue) bn.ngay_sinh = model.NgaySinh.Value;
                    bn.gioi_tinh = model.GioiTinh;
                    bn.dia_chi = model.DiaChi;
                }
                else if (user.bac_si.Any())
                {
                    var bs = user.bac_si.First();
                    bs.ho_ten = model.HoTen;
                    bs.gioi_thieu = model.GioiThieu;
                }
                else if (user.nhan_vien.Any())
                {
                    var nv = user.nhan_vien.First();
                    nv.ho_ten = model.HoTen;
                }

                db.SaveChanges();

                Session["HoTen"] = model.HoTen;
                TempData["Success"] = "Cập nhật hồ sơ thành công!";
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                if (msg.Contains("UNIQUE")) TempData["Error"] = "Email hoặc SĐT này đã được sử dụng!";
                else TempData["Error"] = "Lỗi: " + msg;
            }
        }

        model.AnhDaiDien = user.anh_dai_dien;
        model.TenVaiTro = user.vai_tro.ten_vai_tro;
        return View("UserProfile", model);
    }
    public ActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    public ActionResult ChangePassword(ChangePasswordViewModel model)
    {
        if (model.NewPassword != model.ConfirmPassword)
        {
            ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
            return View(model);
        }

        try
        {
            long userId = Convert.ToInt64(Session["UserID"]);

            db.sp_DoiMatKhau(userId, model.OldPassword, model.NewPassword);

            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("UserProfile");
        }
        catch (Exception ex)
        {
            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            if (msg.Contains("Mật khẩu cũ không chính xác"))
            {
                ModelState.AddModelError("", "Mật khẩu cũ không đúng.");
            }
            else
            {
                ModelState.AddModelError("", "Lỗi: " + msg);
            }
            return View(model);
        }
    }

}