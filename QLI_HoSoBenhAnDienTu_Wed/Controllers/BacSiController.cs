using System;
using System.Linq;
using System.Web.Mvc;
using QLI_HoSoBenhAnDienTu_Wed.Models;
using QLI_HoSoBenhAnDienTu_Wed.Models.viewsmodel; 

namespace QLI_HoSoBenhAnDienTu_Wed.Controllers
{
    public class BacSiController : Controller
    {
        private QL_BENHVIEN_WEBEntities1 db = new QL_BENHVIEN_WEBEntities1();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 2)
                filterContext.Result = RedirectToAction("Login", "Account");
            base.OnActionExecuting(filterContext);
        }

        public ActionResult Dashboard()
        {

            var list = db.v_DanhSachChoKham.ToList();
            return View(list);
        }

        public ActionResult KhamBenh(long maLichHen)
        {
            var lich = db.lich_hen.Find(maLichHen);

            var dotKham = db.dot_kham.FirstOrDefault(d => d.ma_lich_hen == maLichHen);
            if (dotKham == null)
            {
                dotKham = new dot_kham
                {
                    ma_benh_nhan = lich.ma_benh_nhan,
                    ma_bac_si = lich.ma_bac_si,
                    ma_lich_hen = maLichHen,
                    thoi_gian_bat_dau = DateTime.Now
                };
                db.dot_kham.Add(dotKham);
                db.SaveChanges();
            }

            ViewBag.ListThuoc = new SelectList(db.thuoc, "ma_thuoc", "ten_thuoc");
            return View(dotKham);
        }

        [HttpPost]
        public ActionResult LuuKetQua(long maDotKham, string chanDoan, string ketLuan)
        {
            var dk = db.dot_kham.Find(maDotKham);
            dk.chan_doan_so_bo = chanDoan;
            dk.ket_luan = ketLuan;
            dk.thoi_gian_ket_thuc = DateTime.Now;
            db.SaveChanges(); 
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public JsonResult KeDon(KeDonThuocViewModel model)
        {
            try
            {
                db.sp_BacSiKeDon(model.MaDotKham, model.MaThuoc, model.SoLuong, model.CachDung);
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
        [HttpGet]

        public JsonResult GetDonThuoc(long maDotKham)
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
                                 CachDung = ct.cach_dung
                             }).ToList();

            return Json(new { success = true, data = listThuoc }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult HenTaiKham(long maDotKham, int soNgaySau, string ghiChu)
        {
            try
            {
                db.sp_TaoLichTaiKham(maDotKham, soNgaySau, ghiChu);

                TempData["Success"] = "Đã tạo lịch tái khám thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi tạo lịch: " + ex.Message;
            }

            return RedirectToAction("Dashboard");
        }
    }
}