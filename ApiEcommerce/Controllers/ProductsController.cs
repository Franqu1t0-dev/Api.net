using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Models.Dtos.Responses;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [ApiVersionNeutral]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository= productRepository;
            _categoryRepository= categoryRepository;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public IActionResult GetProducts()
        {
            var products = _productRepository.GetProducts();
            var productsDto = products.Adapt<List<ProductDto>>();
            return Ok(productsDto);
        }
        [AllowAnonymous]
        [HttpGet("{productId:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetProduct(int productId)
        {
            var product = _productRepository.GetProduct(productId);

            if (product == null)
            {
                return NotFound($"El producto con el id {productId} no existe.");
            }

            var productsDto = product.Adapt<ProductDto>();

            return Ok(productsDto);
        }

        [AllowAnonymous]
        [HttpGet("Paged", Name = "GetProductsInPage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetProductsInPage([FromQuery] int pageNumber=1, [FromQuery] int pageSize=5 )
        {
            if(pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("Los paramétros de paginación no son válidos");
            }

            var totalProducts = _productRepository.GetTotalProducts();
            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            if(pageNumber > totalPages)
            {
                return NotFound("No hay más páginas disponibles");
            }
            var products = _productRepository.GetProductsInPages(pageNumber, pageSize);
            var productsDto = products.Adapt<List<ProductDto>>();
            var paginationResponse = new PaginationResponse<ProductDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages= totalPages,
                Items = productsDto
            };
            return Ok(paginationResponse);
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateProduct([FromForm] CreateProductDto createProductDto)
        {
            if(createProductDto == null)
            {
                return BadRequest(ModelState);
            }
            if(_productRepository.ProductExists(createProductDto.Name))
            {
                ModelState.AddModelError("CustomError","El producto ya existe");
                return BadRequest(ModelState);
            }
            if(!_categoryRepository.CategoryExists(createProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError",$"La categoría con el id {createProductDto.CategoryId} no existe");
                return BadRequest(ModelState);
            }
            var product = createProductDto.Adapt<Product>();
            //agregando imagen
            if(createProductDto.Image != null)
            {
                UploadProductImage(createProductDto, product);
            } 
            else
            {
                product.ImgUrl ="https://placehold.co/600x400";
            }
            if(!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError("CustomError",$"Algo salió mal al guardar el registro {product.Name}");
                return StatusCode(500,ModelState);
            }
            var createdProduct= _productRepository.GetProduct(product.ProductId);
            var productDto= createdProduct.Adapt<ProductDto>();
            return CreatedAtRoute("GetProduct", new { productId = product.ProductId},productDto);

        } 
        [AllowAnonymous]
        [HttpGet("searchProductByCategory/{categoryId:int}", Name = "GetProductForCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetProductsForCategory(int categoryId)
        {
            var products = _productRepository.GetProductsForCategory(categoryId);

            if (products.Count == 0)
            {
                return NotFound($"Los productos con la categoría {categoryId} no existe.");
            }

            var productsDto = products.Adapt<List<ProductDto>>();

            return Ok(productsDto);
        }
        [AllowAnonymous]
        [HttpGet("searchProductByNameDescription/{searchTerm}", Name = "SearchProducts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult SearchProducts(string searchTerm)
        {
            var products = _productRepository.SearchProducts(searchTerm);

            if (products.Count == 0)
            {
                return NotFound($"Los productos con el nombre '{searchTerm}' o descripcion no existen.");
            }

            var productsDto = products.Adapt<List<ProductDto>>();

            return Ok(productsDto);
        }
        [HttpPatch("buyProduct/{name}/{quantity:int}", Name = "BuyProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult BuyProduct(string name, int quantity)
        {
            //var products = _productRepository.BuyProduct(name);
            if(string.IsNullOrWhiteSpace(name) || quantity <=0)
            {
                return BadRequest("El nombre del producto o la cantidad no son válidos");
            }
            var foundProduct= _productRepository.ProductExists(name);

            if(!foundProduct)
            {
                return NotFound($"El producto con el nombre {name} no existe");
            }
            if(!_productRepository.BuyProduct(name,quantity))
            {
                ModelState.AddModelError("CustomError",$"No se pudo comprar el producto {name} o la cantidad solicitada es mayor al stock dispnible");
                return BadRequest(ModelState);
            }
            var units= quantity == 1? "unidad":"unidades";
            return Ok($"Se compro {quantity} {units} del producto: '{name}'");
        }
        [HttpPut("{productId:int}", Name="UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateProduct(int productId,[FromForm] UpdateProductDto updateProductDto)
        {
            if(updateProductDto == null)
            {
                return BadRequest(ModelState);
            }
            if(!_productRepository.ProductExists(productId))
            {
                ModelState.AddModelError("CustomError","El producto no existe");
                return BadRequest(ModelState);
            }
            if(!_categoryRepository.CategoryExists(updateProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError",$"La categoría con el id {updateProductDto.CategoryId} no existe");
                return BadRequest(ModelState);
            }
            var product = updateProductDto.Adapt<Product>();
            product.ProductId = productId;
            //agregando imagen
            if(updateProductDto.Image != null)
            {
                UploadProductImage(updateProductDto, product);
            } 
            else
            {
                product.ImgUrl ="https://placehold.co/600x400";
            }
            if(!_productRepository.UpdateProduct(product))
            {
                ModelState.AddModelError("CustomError",$"Algo salió mal al actualizar el registro {product.Name}");
                return StatusCode(500,ModelState);
            }
            
            return NoContent();

        } 
        private void UploadProductImage(dynamic productDto, Product product)
        {
        string fileName = product.ProductId + Guid.NewGuid().ToString() + Path.GetExtension(productDto.Image.FileName);
        var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProductsImages");
        if (!Directory.Exists(imagesFolder))
        {
            Directory.CreateDirectory(imagesFolder);
        }
        var filePath = Path.Combine(imagesFolder, fileName);
        FileInfo file = new FileInfo(filePath);
        if (file.Exists)
        {
            file.Delete();
        }
        using var fileStream = new FileStream(filePath, FileMode.Create);
        productDto.Image.CopyTo(fileStream);
        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
        product.ImgUrl = $"{baseUrl}/ProductsImages/{fileName}";
        product.ImgUrlLocal = filePath;
        }
        
        [HttpDelete("{productId:int}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteProduct(int productId,[FromBody] UpdateProductDto deleteProductDto)
        {
            if(productId==0)
            {
                return BadRequest(ModelState);
            }
            var product = _productRepository.GetProduct(productId);

            if (product == null)
            {
                return NotFound($"El producto con el id {productId} no existe.");
            }
            if(!_productRepository.DeleteProduct(product))
            {
                ModelState.AddModelError("CustomError",$"Algo salió mal al eliminar el registro {product.Name}");
                return StatusCode(500,ModelState);
            }

            return NoContent();
        }
    } 
}

