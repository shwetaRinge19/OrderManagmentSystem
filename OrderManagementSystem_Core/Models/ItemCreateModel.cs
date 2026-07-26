using System.ComponentModel.DataAnnotations;

namespace OrderManagementSystem_Core.Models
{
    public class ItemCreateModel
    {
        [Required]
        public string Barcode { get; set; }

        [Required]
        public string ItemName { get; set; }

        public int? AgencyId { get; set; }

        [Required]
        public decimal Rate { get; set; }

        [Range(0, int.MaxValue)]
        public int InitialStock { get; set; }
    }
}