using System.ComponentModel.DataAnnotations;

namespace DsiCode.MicroService.Product.API.Models.Dto
{
    public class ProductDto
    {
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public double Price { get; set; }

        public string Description { get; set; } = string.Empty;

        [Required]
        public string CategoryName { get; set; } = string.Empty;

        // ✅ CAMBIO IMPORTANTE: Hacer nullable estos campos
        public string? ImageUrl { get; set; }
        public string? ImageLocalPath { get; set; }

        public IFormFile? Image { get; set; }
    }
}