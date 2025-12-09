using QLI_HoSoBenhAnDienTu_Wed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLI_HoSoBenhAnDienTu_Wed.Controllers
{
    public class BaseController : Controller
    {
        protected QL_BENHVIEN_WEBEntities1 db = new QL_BENHVIEN_WEBEntities1();

        protected int GetCurrentUserId()
        {
            return Session["UserID"] != null ? (int)Session["UserID"] : 0;
        }

        protected string GetCurrentRole()
        {
            return Session["RoleName"] != null ? Session["RoleName"].ToString() : "";
        }
    }
}