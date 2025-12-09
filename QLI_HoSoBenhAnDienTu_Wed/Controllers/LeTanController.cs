using QLI_HoSoBenhAnDienTu_Wed.Models;
using QLI_HoSoBenhAnDienTu_Wed.Models.viewsmodel;
using System;
using System.Linq;
using System.Web.Mvc;

namespace QLI_HoSoBenhAnDienTu_Wed.Controllers
{
    public class LeTanController : Controller
    {
        private QL_BENHVIEN_WEBEntities1 db = new QL_BENHVIEN_WEBEntities1();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 3)
                filterContext.Result = RedirectToAction("Login", "Account");
            base.OnActionExecuting(filterContext);
        }

        public ActionResult Dashboard()
        {
            var listLichHen = db.v_LichHenChoLeTan.ToList();

            ViewBag.ListKhoa = new SelectList(db.khoa_phong, "ma_khoa", "ten_khoa");

            return View(listLichHen);
        }

        [HttpGet]
        public JsonResult GetBacSiByKhoa(int maKhoa)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var listBS = db.bac_si
                           .Where(bs => bs.ma_khoa == maKhoa)
                           .Select(bs => new {
                               ma_bac_si = bs.ma_bac_si,
                               ho_ten = bs.ho_ten
                           }).ToList();
            return Json(new { success = true, data = listBS }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult XuLyLich(long maLichHen, string hanhDong, int? maBacSi)
        {
            try
            {
                if (Session["UserID"] == null) return Json(new { success = false, message = "Hết phiên đăng nhập." });

                long userId = Convert.ToInt64(Session["UserID"]);
                var nhanVien = db.nhan_vien.FirstOrDefault(n => n.ma_tai_khoan == userId);

                if (nhanVien == null) return Json(new { success = false, message = "Bạn không có quyền Lễ tân." });


                db.sp_LeTanDuyetLich(maLichHen, nhanVien.ma_nhan_vien, hanhDong, maBacSi);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = "Lỗi: " + msg });
            }
        }
        public ActionResult TaoHoSoTaiQuay()
        {
            return View();
        }

        [HttpPost]
        public ActionResult TaoHoSoTaiQuay(RegisterViewModel model)
        {
            try
            {
                string matKhauMacDinh = model.SDT;


                db.sp_DangKyTaiKhoan(model.SDT, matKhauMacDinh, model.HoTen, model.SDT, model.NgaySinh, model.GioiTinh);

                TempData["Success"] = "Đã tạo hồ sơ thành công! Mật khẩu mặc định là SĐT.";
                return RedirectToAction("Dashboard"); 
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                if (msg.Contains("UNIQUE") && msg.Contains("so_dien_thoai"))
                {
                    ViewBag.Error = "Số điện thoại này đã tồn tại!";
                }
                else if (msg.Contains("UNIQUE") && msg.Contains("so_dinh_danh"))
                {
                    ViewBag.Error = "Số CCCD/CMND này đã tồn tại!";
                }
                else if (msg.Contains("UNIQUE") && msg.Contains("email"))
                {
                    ViewBag.Error = "Email này đã tồn tại!";
                }
                else
                {
                    ViewBag.Error = "Lỗi hệ thống: " + msg;
                }

                return View(model);
            }
        }

        
    }
}