using System.Web;
using System.Web.Mvc;

namespace QLI_HoSoBenhAnDienTu_Wed
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
