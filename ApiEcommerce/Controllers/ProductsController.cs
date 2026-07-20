using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository,IMapper mapper)
        {
            _productRepository= productRepository;
            _categoryRepository= categoryRepository;
            _mapper =mapper;

        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public IActionResult GetProducts()
        {
            var products = _productRepository.GetProducts();
            var productsDto = _mapper.Map<List<ProductDto>>(products);
            return Ok(productsDto);
        }

        [HttpGet("{productId:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetProduct(int productId)
        {
            var product = _productRepository.GetProduct(productId);

            if (product == null)
            {
                return NotFound($"El producto con el id {productId} no existe.");
            }

            var productsDto = _mapper.Map<ProductDto>(product);

            return Ok(productsDto);
        }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateProduct([FromBody] CreateProductDto createProductDto)
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
            var product =_mapper.Map<Product>(createProductDto);
            if(!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError("CustomError",$"Algo salió mal al guardar el registro {product.Name}");
                return StatusCode(500,ModelState);
            }
            var createdProduct= _productRepository.GetProduct(product.ProductId);
            var productDto= _mapper.Map<ProductDto>(createdProduct);
            return CreatedAtRoute("GetProduct", new { productId = product.ProductId},productDto);

        } 

        [HttpGet("searchByCategory/{categoryId:int}", Name = "GetProductForCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetProductsForCategory(int categoryId)
        {
            var products = _productRepository.GetProductsForCategory(categoryId);

            if (products.Count == 0)
            {
                return NotFound($"Los producto con la categoría {categoryId} no existe.");
            }

            var productsDto = _mapper.Map<List<ProductDto>>(products);

            return Ok(productsDto);
        }
    } 
}

