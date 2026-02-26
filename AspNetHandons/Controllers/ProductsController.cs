using AspNetHandons.Entities;
using AspNetHandons.Filters;
using AspNetHandons.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AspNetHandons.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private static List<Product> products = new()
        {
            new Product { Id = 1, Name = "Laptop", Quantity = 1000 },
            new Product { Id = 2, Name = "Phone", Quantity = 500 }
        };

        private readonly IMemoryCache _cache;
        private readonly ILogger<ProductsController> _logger;
        private readonly ProductMapper _mapper;
        private const string CACHE_KEY = "all";
        private static readonly Lock _lock = new();
        public ProductsController(IMemoryCache cache, ILogger<ProductsController> logger, ProductMapper mapper)
        {
            _cache = cache;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet()]
        [ServiceFilter(typeof(AuthorizationFilter))]
        public IActionResult GetAll()
        {
            if (!_cache.TryGetValue(CACHE_KEY, out List<Product>? productcache))
            {
                lock (_lock)
                {
                    _logger.LogInformation("Cache MISS");

                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                        .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                    productcache = products.ToList();

                    _cache.Set(CACHE_KEY, productcache, cacheOptions);
                }
            }
            else
            {
                _logger.LogInformation("Cache HIT");
            }

            var productDtos = productcache!.Select(_mapper.ProductToProductDto).ToList();
            return Ok(productDtos);
        }

        [HttpPost()]
        public IActionResult Create([FromBody] Product product)
        {
            products.Add(product);
            return Ok(product);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Product updatedProduct)
        {
            var product = products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound($"Product with Id = {id} not found");
            }

            product.Name = updatedProduct.Name;
            product.Quantity = updatedProduct.Quantity;

            return Ok(product);
        }


        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var product = products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound($"Product {id} not found");
            }

            products.Remove(product);
            return Ok($"Product {id} deleted");
        }

    }
}
