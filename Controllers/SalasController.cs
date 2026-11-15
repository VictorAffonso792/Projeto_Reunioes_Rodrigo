
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

        // GET: api/salas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sala>>> ListarSalas(
            [FromQuery] string? nome,
            [FromQuery] int pagina = 1)
        {
            if (pagina < 1)
            {
                return BadRequest("A página deve ser maior que zero.");
            }

            var consulta = _context.Salas
                .AsNoTracking()
                .AsQueryable();

            // Filtrar pelo nome da sala
            if (!string.IsNullOrWhiteSpace(nome))
            {
                consulta = consulta.Where(s =>
                    EF.Functions.ILike(s.Nome, $"%{nome.Trim()}%"));
            }

            // Ordenar por andar e depois por nome
            consulta = consulta
                .OrderBy(s => s.Andar)
                .ThenBy(s => s.Nome)
                .ThenBy(s => s.Id);

            // Paginar os resultados de 10 em 10
            const int tamanhoPagina = 10;

            var salas = await consulta
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return Ok(salas);
        }
    }
}
