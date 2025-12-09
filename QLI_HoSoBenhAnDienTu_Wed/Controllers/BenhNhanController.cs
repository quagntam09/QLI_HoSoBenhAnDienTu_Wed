using System;
using System.Linq;
using System.Web.Mvc;
using QLI_HoSoBenhAnDienTu_Wed.Models;

namespace QLI_HoSoBenhAnDienTu_Wed.Controllers
{
    public class BenhNhanController : Controller
    {
        private QL_BENHVIEN_WEBEntities1 db = new QL_BENHVIEN_WEBEntities1();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 4)
                filterContext.Result = RedirectToAction("Login", "Account");
            base.OnActionExecuting(filterContext);
        }

        public ActionResult DatLich()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DatLich(DateTime thoiGianHen, string lyDo)
        {
            try
            {
                if (thoiGianHen < DateTime.Now)
                {
                    ViewBag.Error = "Thời gian hẹn không được nhỏ hơn thời điểm hiện tại!";
                    return View();
                }

                long userId = Convert.ToInt64(Session["UserID"]);
                var benhNhan = db.benh_nhan.FirstOrDefault(b => b.ma_tai_khoan == userId);


                db.sp_DatLichKham(benhNhan.ma_benh_nhan, null, thoiGianHen, lyDo);

                TempData["Success"] = "Đặt lịch thành công! Lễ tân sẽ sắp xếp bác sĩ phù hợp cho bạn.";
                return RedirectToAction("LichSuKham");
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ViewBag.Error = "Lỗi: " + msg;
                return View();
            }
        }

        public ActionResult LichSuKham()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            long userId = Convert.ToInt64(Session["UserID"]);

            var benhNhan = db.benh_nhan.FirstOrDefault(b => b.ma_tai_khoan == userId);

            if (benhNhan == null)
            {
                ViewBag.Error = "Không tìm thấy hồ sơ bệnh án cho tài khoản này.";
                return View(new System.Collections.Generic.List<QLI_HoSoBenhAnDienTu_Wed.Models.f_LayLichSuKham_Result>());
            }

            var history = db.f_LayLichSuKham(benhNhan.ma_benh_nhan).ToList();
            return View(history);
        }

        [HttpGet]
        public JsonResult XemChiTietDonThuoc(long maDotKham)
        {
            var listThuoc = (from ct in db.chi_tiet_don_thuoc
                             join dt in db.don_thuoc on ct.ma_don_thuoc equals dt.ma_don_thuoc
                             join t in db.thuoc on ct.ma_thuoc equals t.ma_thuoc
                             where dt.ma_dot_kham == maDotKham
                             select new
                             {
                                 TenThuoc = t.ten_thuoc,
                                 SoLuong = ct.so_luong,
                                 DonVi = t.don_vi,
                                 CachDung = ct.cach_dung,
                                 LoiDan = dt.loi_dan 
                             }).ToList();

            return Json(new { success = true, data = listThuoc }, JsonRequestBehavior.AllowGet);
        }
    }
}