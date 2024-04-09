using RacerBooksAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RacerBooksAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : Controller
    {
        private readonly RacerbooksContext _context;

        public ItemsController(RacerbooksContext context)
        {
            _context = context;
        }

        // GET: api/Items
        [HttpPost("GetItems")]
        public async Task<ActionResult<IEnumerable<Item>>> GetItems(string searchBy, string search)
        {
            if (searchBy == "ItemPrice")
            {
                return Ok(await _context.Items.Where(x => x.ItemPrice.ToString() == search || search == null).ToListAsync());
            }
            else
            {
                return Ok(await _context.Items.Where(x => x.ItemName.StartsWith(search) || search == null).ToListAsync());
            }

        }

        // GET: api/Items/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItem(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FirstOrDefaultAsync(m => m.ItemCode == id);
            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        // POST: api/Items
        [HttpPost]
        public async Task<ActionResult<Item>> CreateItem([FromBody] Item item)
        {
            if (ModelState.IsValid)
            {
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetItem), new { id = item.ItemCode }, item);
            }
            return BadRequest(ModelState);
        }

        private bool ItemExists(string id)
        {
            return _context.Items.Any(e => e.ItemCode == id);
        }

    }
}
