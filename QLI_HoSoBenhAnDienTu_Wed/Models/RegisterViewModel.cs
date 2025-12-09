using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QLI_HoSoBenhAnDienTu_Wed.Models.viewsmodel
{
    public class RegisterViewModel
    {
        [Required] public string Username { get; set; }
        [Required][DataType(DataType.Password)] public string Password { get; set; }
        [Required] public string HoTen { get; set; }
        [Required] public string SDT { get; set; }
        [DataType(DataType.Date)] public DateTime NgaySinh { get; set; }
        public string GioiTinh { get; set; } // "M", "F", "O"
    }
}