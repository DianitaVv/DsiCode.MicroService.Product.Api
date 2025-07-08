using AutoMapper;
using DsiCode.MicroService.Product.API.Data;
using DsiCode.MicroService.Product.API.Models.Dto;
using DsiCode.MicroService.Product.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace DsiCode.MicroService.Product.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _db;
        private ResponseDto _response;
        private IMapper _mapper;

        public ProductController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
            _response = new ResponseDto();
        }

        [HttpGet]
        public ResponseDto Get()
        {
            try
            {
                // ✅ Usar el nombre correcto del DbSet
                IEnumerable<DsiCode.MicroService.Product.API.Models.Product> objList = _db.Products.ToList();
                _response.Result = _mapper.Map<List<ProductDto>>(objList);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                Console.WriteLine($"❌ Error en Get Products: {ex.Message}");
            }
            return _response;
        }

        [HttpPost]
        [Authorize(Roles = "ADMINISTRATOR")]
        public ResponseDto Post(ProductDto productDto)
        {
            try
            {
                DsiCode.MicroService.Product.API.Models.Product product = _mapper.Map<DsiCode.MicroService.Product.API.Models.Product>(productDto);

                // ✅ ASIGNAR ImageUrl e ImageLocalPath ANTES del primer SaveChanges
                if (productDto.Image != null)
                {
                    product.ImageUrl = "temp"; // Temporal
                    product.ImageLocalPath = "temp"; // Temporal
                }
                else
                {
                    product.ImageUrl = "https://placehold.co/600x400";
                    product.ImageLocalPath = null;
                }

                // ✅ Usar el nombre correcto del DbSet
                _db.Products.Add(product);
                _db.SaveChanges(); // Aquí se obtiene el ProductId

                // ✅ AHORA procesar la imagen con el ProductId real
                if (productDto.Image != null)
                {
                    try
                    {
                        string fileName = product.ProductId + Path.GetExtension(productDto.Image.FileName);
                        var filePath = Path.Combine("wwwroot", "images", fileName); // ✅ Usar Path.Combine
                        var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                        // ✅ Crear directorio si no existe con mejor manejo
                        var directoryPath = Path.GetDirectoryName(filePathDirectory);
                        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
                        {
                            productDto.Image.CopyTo(fileStream);
                        }

                        // ✅ Para Docker: usar URL relativa más robusta
                        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                        product.ImageUrl = baseUrl + "/ProductImages/" + fileName;
                        product.ImageLocalPath = filePath;

                        // ✅ Actualizar el producto con la info real de la imagen
                        _db.Products.Update(product);
                        _db.SaveChanges();

                        Console.WriteLine($"✅ Imagen guardada: {product.ImageUrl}");
                    }
                    catch (Exception imageEx)
                    {
                        Console.WriteLine($"❌ Error procesando imagen: {imageEx.Message}");

                        // En caso de error, mantener placeholder
                        product.ImageUrl = "https://placehold.co/600x400";
                        product.ImageLocalPath = null;

                        _db.Products.Update(product);
                        _db.SaveChanges();
                    }
                }

                _response.Result = _mapper.Map<ProductDto>(product);
                Console.WriteLine("✅ Producto creado exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en ProductController.Post: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpGet]
        [Route("{id:int}")]
        public ResponseDto Get(int id)
        {
            try
            {
                // ✅ Usar el nombre correcto del DbSet
                DsiCode.MicroService.Product.API.Models.Product obj = _db.Products.FirstOrDefault(u => u.ProductId == id);
                if (obj == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Producto no encontrado";
                    return _response;
                }
                _response.Result = _mapper.Map<ProductDto>(obj);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                Console.WriteLine($"❌ Error en Get Product by ID: {ex.Message}");
            }
            return _response;
        }

        [HttpPut]
        [Authorize(Roles = "ADMINISTRATOR")]
        public ResponseDto Put(ProductDto productoDto)
        {
            try
            {
                // ✅ Usar el nombre correcto del DbSet
                var existingProduct = _db.Products.FirstOrDefault(p => p.ProductId == productoDto.ProductId);
                if (existingProduct == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Producto no encontrado";
                    return _response;
                }

                // Actualizar propiedades básicas
                existingProduct.Name = productoDto.Name;
                existingProduct.Price = productoDto.Price;
                existingProduct.Description = productoDto.Description;
                existingProduct.CategoryName = productoDto.CategoryName;

                // Solo procesar imagen si se envió una nueva
                if (productoDto.Image != null && productoDto.Image.Length > 0)
                {
                    // Eliminar imagen anterior si existe
                    if (!string.IsNullOrEmpty(existingProduct.ImageLocalPath))
                    {
                        var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), existingProduct.ImageLocalPath);
                        FileInfo archivo = new FileInfo(oldFilePathDirectory);
                        if (archivo.Exists)
                        {
                            archivo.Delete();
                        }
                    }

                    // ✅ Crear nueva imagen con mejor manejo de rutas
                    string fileName = existingProduct.ProductId + Path.GetExtension(productoDto.Image.FileName);
                    string archivoPath = Path.Combine("wwwroot", "images", fileName);
                    var archivoPathDirectory = Path.Combine(Directory.GetCurrentDirectory(), archivoPath);

                    var directoryPath = Path.GetDirectoryName(archivoPathDirectory);
                    if (!string.IsNullOrEmpty(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    using (var fileStream = new FileStream(archivoPathDirectory, FileMode.Create))
                    {
                        productoDto.Image.CopyTo(fileStream);
                    }

                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    existingProduct.ImageUrl = baseUrl + "/ProductImages/" + fileName;
                    existingProduct.ImageLocalPath = archivoPath;
                }

                _db.SaveChanges();
                _response.Result = _mapper.Map<ProductDto>(existingProduct);
                Console.WriteLine("✅ Producto actualizado exitosamente");
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                Console.WriteLine($"❌ Error en Put Product: {ex.Message}");
            }
            return _response;
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "ADMINISTRATOR")]
        public ResponseDto Delete(int id)
        {
            try
            {
                // ✅ Usar el nombre correcto del DbSet
                DsiCode.MicroService.Product.API.Models.Product producto = _db.Products.FirstOrDefault(u => u.ProductId == id);
                if (producto == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Producto no encontrado";
                    return _response;
                }

                if (!string.IsNullOrEmpty(producto.ImageLocalPath))
                {
                    var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), producto.ImageLocalPath);
                    FileInfo archivo = new FileInfo(oldFilePathDirectory);
                    if (archivo.Exists)
                    {
                        archivo.Delete(); // Elimina el archivo antiguo
                    }
                }

                _db.Products.Remove(producto); // Elimina el producto
                _db.SaveChanges();
                _response.Result = "Producto eliminado exitosamente";
                Console.WriteLine("✅ Producto eliminado exitosamente");
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                Console.WriteLine($"❌ Error en Delete Product: {ex.Message}");
            }
            return _response;
        }
    }
}