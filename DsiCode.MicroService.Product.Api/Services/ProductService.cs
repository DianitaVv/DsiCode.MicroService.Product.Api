using AutoMapper;
using DsiCode.MicroService.Product.API.Data;
using DsiCode.MicroService.Product.API.Models.Dto;
using DsiCode.MicroService.Product.API.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace DsiCode.MicroService.Product.API.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public ProductService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ResponseDto> GetAllProductsAsync()
        {
            var response = new ResponseDto();
            try
            {
                var products = await _db.Products.ToListAsync();
                response.Result = _mapper.Map<IEnumerable<ProductDto>>(products);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDto> GetProductByIdAsync(int id)
        {
            var response = new ResponseDto();
            try
            {
                var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == id);
                if (product == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Producto no encontrado";
                    return response;
                }
                response.Result = _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDto> CreateProductAsync(ProductDto productDto)
        {
            var response = new ResponseDto();
            try
            {
                var product = _mapper.Map<DsiCode.MicroService.Product.API.Models.Product>(productDto);
                _db.Products.Add(product);
                await _db.SaveChangesAsync();

                if (productDto.Image != null)
                {
                    string fileName = product.ProductId + Path.GetExtension(productDto.Image.FileName);
                    string filePath = @"wwwroot\ProductImages\" + fileName;
                    var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePathDirectory));

                    using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
                    {
                        await productDto.Image.CopyToAsync(fileStream);
                    }

                    product.ImageUrl = $"/ProductImages/{fileName}";
                    product.ImageLocalPath = filePath;
                }
                else
                {
                    product.ImageUrl = "https://placehold.co/600x400";
                }

                _db.Products.Update(product);
                await _db.SaveChangesAsync();
                response.Result = _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDto> UpdateProductAsync(ProductDto productDto)
        {
            var response = new ResponseDto();
            try
            {
                var product = _mapper.Map<DsiCode.MicroService.Product.API.Models.Product>(productDto);

                if (productDto.Image != null)
                {
                    if (!string.IsNullOrEmpty(product.ImageLocalPath))
                    {
                        var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), product.ImageLocalPath);
                        if (File.Exists(oldFilePathDirectory))
                        {
                            File.Delete(oldFilePathDirectory);
                        }
                    }

                    string fileName = product.ProductId + Path.GetExtension(productDto.Image.FileName);
                    string filePath = @"wwwroot\ProductImages\" + fileName;
                    var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePathDirectory));

                    using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
                    {
                        await productDto.Image.CopyToAsync(fileStream);
                    }

                    product.ImageUrl = $"/ProductImages/{fileName}";
                    product.ImageLocalPath = filePath;
                }

                _db.Products.Update(product);
                await _db.SaveChangesAsync();
                response.Result = _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDto> DeleteProductAsync(int id)
        {
            var response = new ResponseDto();
            try
            {
                var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == id);
                if (product == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Producto no encontrado";
                    return response;
                }

                if (!string.IsNullOrEmpty(product.ImageLocalPath))
                {
                    var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), product.ImageLocalPath);
                    if (File.Exists(filePathDirectory))
                    {
                        File.Delete(filePathDirectory);
                    }
                }

                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
                response.Result = "Producto eliminado correctamente";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}