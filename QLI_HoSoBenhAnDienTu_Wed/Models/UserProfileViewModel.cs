using System;
using System.ComponentModel.DataAnnotations;

namespace QLI_HoSoBenhAnDienTu_Wed.Models.viewsmodel
{
    public class UserProfileViewModel
    {
        public long MaTaiKhoan { get; set; }
        public string TenDangNhap { get; set; }

        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string HoTen { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "SĐT không hợp lệ")]
        public string SDT { get; set; }

        public string AnhDaiDien { get; set; }

        public DateTime? NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string DiaChi { get; set; }

        public string ChuyenKhoa { get; set; }
        public string GioiThieu { get; set; }

        public string TenVaiTro { get; set; }
    }
}