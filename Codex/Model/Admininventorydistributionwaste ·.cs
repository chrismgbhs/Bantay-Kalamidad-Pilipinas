using Bantay_Kalamidad_Pilipinas.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bantay_Kalamidad_Pilipinas.Model
{
    public class AdminInventory : ObservableObject
    {
        public string InventoryId { get; set; }
        public string Item { get; set; }
        public string DonatedItem { get; set; }
        public string QuantityAvailable { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string StorageLocation { get; set; }
    }

    public class AdminDistribution : ObservableObject
    {
        public string DistributionId { get; set; }
        public string Beneficiary { get; set; }
        public string Event { get; set; }
        public string DistributionLocation { get; set; }
        public DateTime? DateDistributed { get; set; }
    }

    public class AdminDistributionItem : ObservableObject
    {
        public string DistributionItemId { get; set; }
        public string DistributionId { get; set; }
        public string InventoryItem { get; set; }
        public string Quantity { get; set; }
    }

    public class AdminWaste : ObservableObject
    {
        public string WasteId { get; set; }
        public string InventoryItem { get; set; }
        public string Quantity { get; set; }
        public string Reason { get; set; }
        public DateTime? Date { get; set; }
    }
}