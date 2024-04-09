using RacerBooksAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace RacerBooksAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : Controller
    {
        private readonly RacerbooksContext _context;

        public CartsController(RacerbooksContext context)
        {
            _context = context;
        }

        // POST: api/Carts/AddToCart
        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart([FromBody] string itemcode)
        {
            if (itemcode == null)
            {
                return NotFound();
            }

            var email = HttpContext.Session.GetString("LoggedInUser");
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(); // Use Unauthorized for API
            }

            var item = await _context.Items.FindAsync(itemcode);
            if (item == null)
            {
                return NotFound("Item not found.");
            }

            if (item.Stock <= 0)
            {
                return BadRequest("Item is out of stock.");
            }

            var existingCartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.ItemCode == itemcode && c.Email == email);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += 1;
                _context.Carts.Update(existingCartItem);
            }
            else
            {
                var newCartItem = new Cart
                {
                    ItemCode = itemcode,
                    Email = email,
                    Quantity = 1,
                    ItemCodeNavigation = item,
                    EmailNavigation = await _context.Users.FindAsync(email)
                };
                await _context.Carts.AddAsync(newCartItem);
            }

            item.Stock -= 1;
            _context.Items.Update(item);

            await _context.SaveChangesAsync();
            return Ok("Item added to the cart.");
        }

        // GET: api/Carts/ViewCart
        [HttpGet("ViewCart")]
        public async Task<IActionResult> ViewCart()
        {
            var email = HttpContext.Session.GetString("LoggedInUser");
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(); // Use Unauthorized for API
            }

            var cartItems = await _context.Carts
                .Where(c => c.Email == email)
                .Include(c => c.ItemCodeNavigation)
                .ToListAsync();

            decimal totalPrice = cartItems.Sum(item => item.Quantity * item.ItemCodeNavigation.ItemPrice);

            return Ok(new { CartItems = cartItems, TotalPrice = totalPrice });
        }

        // GET: api/Carts
        [HttpGet]
        public async Task<IActionResult> GetCarts()
        {
            var carts = await _context.Carts.Include(c => c.ItemCodeNavigation).Include(c => c.EmailNavigation).ToListAsync();
            return Ok(carts);
        }

        // GET: api/Carts/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCart(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.ItemCodeNavigation)
                .Include(c => c.EmailNavigation)
                .FirstOrDefaultAsync(m => m.Email == id);
            if (cart == null)
            {
                return NotFound();
            }

            return Ok(cart);
        }

        // POST: api/Carts
        [HttpPost]
        public async Task<IActionResult> CreateCart([FromBody] Cart cart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cart);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetCart), new { id = cart.Email }, cart);
            }
            return BadRequest(ModelState);
        }

        // PUT: api/Carts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCart(string id, [FromBody] Cart cart)
        {
            if (id != cart.Email)
            {
                return BadRequest();
            }

            _context.Entry(cart).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Carts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(string id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CartExists(string id)
        {
            return _context.Carts.Any(e => e.Email == id);
        }

    }
}
