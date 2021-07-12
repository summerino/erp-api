using System.Collections.Generic;
using ERP.Entity.Inventory;

namespace ERP_API.Model.Inventory
{
    public class ItemGroupRequest : ItemGroup
    {
        public IEnumerable<ItemGroupSubGroup> ItemDetails { get; set; }
    }
}
