
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reunioes.Api.Data;
using Reunioes.Api.Models;

namespace Reunioes.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SalasController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/salas
        [HttpPost]
        public async Task<ActionResult<Sala>> CadastrarSala(
            [FromBody] Sala sala)
        {
            if (string.IsNullOrWhiteSpace(sala.Nome))
            {
                return BadRequest("O nome da sala é obrigatório.");
            }

            if (sala.Id != 0)
            {
                return BadRequest(
                    "O ID deve ser gerado pelo banco de dados.");
            }

            _context.Salas.Add(sala);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(BuscarSalaPorId),
                new { id = sala.Id },
                sala
            );
        }

        // GET: api/salas/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Sala>> BuscarSalaPorId(int id)
        {
            var sala = await _context.Salas.FindAsync(id);

            if (sala == null)
            {
                return NotFound("Sala não encontrada.");
            }

            return Ok(sala);
        }
    }
}
