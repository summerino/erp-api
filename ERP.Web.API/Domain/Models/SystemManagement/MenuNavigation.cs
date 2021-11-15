namespace ERP.Web.API.Domain.Models.SystemManagement
{
    public class MenuNavigation 
    {
        public string Icon { get; set; }
       
        public string Text { get; set; }
        
        public string Link { get; set; }
        
        public string Regex { get; set; }

        public IEnumerable<MenuNavigation> Items { get; set; }
    }
}
