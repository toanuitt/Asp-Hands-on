using AspNetHandons.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AspNetHandons.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private static List<Order> orders = new()
        {
            new Order { Id = 1, ProductId = 1, Quantity = 3},
            new Order { Id = 2, ProductId = 2, Quantity = 3},
        };

        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(orders);
        }

        [Authorize(Policy = "OrdersWritePolicy")]
        [HttpPost]
        public IActionResult Create([FromBody] Order order) 
        {
            order.Id = orders.Max(o => o.Id) + 1;
            orders.Add(order);
            return Ok(order); 
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Order updatedOrder)
        {
            var order = orders.FirstOrDefault(o => o.Id == id);

            if (order == null)
                return NotFound($"Order with Id = {id} not found");

            order.ProductId = updatedOrder.ProductId;
            order.Quantity = updatedOrder.Quantity;

            return Ok(updatedOrder);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var order = orders.FirstOrDefault(o => o.Id == id);

            if (order == null)
                return NotFound($"Order with Id = {id} not found");

            orders.Remove(order);

            return Ok($"Order {id} delete");
        }

    }
}
