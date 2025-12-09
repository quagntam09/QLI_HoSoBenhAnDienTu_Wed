using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using QLI_HoSoBenhAnDienTu_Wed.Models;

namespace QLI_HoSoBenhAnDienTu_Wed.Controllers
{
    public class AdminController : Controller
    {
        private QL_BENHVIEN_WEBEntities1 db = new QL_BENHVIEN_WEBEntities1();
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1)
            {
                filterContext.Result = RedirectToAction("Login", "Account");
            }
            base.OnActionExecuting(filterContext);
        }

        public ActionResult Dashboard()
        {
            ViewBag.TongBenhNhan = db.benh_nhan.Count();
            ViewBag.TongBacSi = db.bac_si.Count();

            var thangNay = DateTime.Now.Month;
            var doanhThu = db.v_BaoCaoDoanhThuThang.FirstOrDefault(x => x.Thang == thangNay);
            ViewBag.DoanhThu = doanhThu != null ? doanhThu.TongTienDichVu : 0;

            return View();
        }

        public ActionResult QuanLyNhanSu()
        {
            var nhanSu = db.tai_khoan.Include(t => t.vai_tro)
                           .Where(t => t.ma_vai_tro != 4) 
                           .OrderBy(t => t.ma_vai_tro).ToList();
            return View(nhanSu);
        }

        [HttpPost]
        public ActionResult TaoTaiKhoan(string username, string password, int roleId, string hoTen)
        {
            try
            {
                var tk = new tai_khoan
                {
                    ten_dang_nhap = username,
                    mat_khau_hash = password, 
                    ma_vai_tro = roleId,
                    dang_hoat_dong = true,
                    ngay_tao = DateTime.Now
                };
                db.tai_khoan.Add(tk);
                db.SaveChanges();

                if (roleId == 2) 
                {
                    db.bac_si.Add(new bac_si { ma_tai_khoan = tk.ma_tai_khoan, ho_ten = hoTen, kinh_nghiem = 1 });
                }
                else 
                {
                    db.nhan_vien.Add(new nhan_vien { ma_tai_khoan = tk.ma_tai_khoan, ho_ten = hoTen });
                }
                db.SaveChanges();

                TempData["Success"] = "Tạo tài khoản thành công!";
            }
            catch (Exception ex) { TempData["Error"] = "Lỗi: " + ex.Message; }

            return RedirectToAction("QuanLyNhanSu");
        }

        public ActionResult SaoLuuHeThong() => View();

        [HttpPost]
        public ActionResult ThucHienSaoLuu(string loaiBackup)
        {
            try
            {
                db.sp_TuDongSaoLuu(loaiBackup);
                TempData["Success"] = "Sao lưu thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            }
            return RedirectToAction("SaoLuuHeThong");
        }
        public ActionResult GetDuLieuThongKe()
        {
            var dataKhoa = db.v_ThongKeBenhNhanTheoKhoa.Select(x => new {
                TenKhoa = x.ten_khoa,
                SoLuong = x.SoLuongLuotKham,
                DoanhThu = x.DoanhThuKhoa
            }).ToList();

            var dataGioiTinh = db.v_ThongKeTyLeGioiTinh.Select(x => new {
                Label = x.GioiTinh,
                Value = x.SoLuong
            }).ToList();

            return Json(new { success = true, khoa = dataKhoa, gioitinh = dataGioiTinh }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult NhatKyHeThong()
        {
            var logs = db.nhat_ky_he_thong.OrderByDescending(x => x.thoi_gian).Take(50).ToList();
            return View(logs);
        }

        [HttpPost]
        public ActionResult ResetPassword(long targetUserId)
        {
            try
            {
                long adminId = Convert.ToInt64(Session["UserID"]); 
                string defaultPass = "123456";

                db.sp_AdminResetMatKhau(adminId, targetUserId, defaultPass);

                return Json(new { success = true, message = "Đã reset về mật khẩu: 123456" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}