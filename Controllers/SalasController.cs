
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

            // Filtrar pelo nome
            if (!string.IsNullOrWhiteSpace(nome))
            {
                consulta = consulta.Where(s =>
                    EF.Functions.ILike(s.Nome, $"%{nome.Trim()}%"));
            }

            // Ordenar por andar e nome
            consulta = consulta
                .OrderBy(s => s.Andar)
                .ThenBy(s => s.Nome)
                .ThenBy(s => s.Id);

            // Paginação de 10 em 10
            const int tamanhoPagina = 10;

            var salas = await consulta
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return Ok(salas);
        }

        // GET: api/salas/horas-livres?inicio=2026-11-16&fim=2026-11-16
        [HttpGet("horas-livres")]
        public async Task<IActionResult> ConsultarHorasLivres(
            [FromQuery] DateOnly inicio,
            [FromQuery] DateOnly fim)
        {
            if (inicio == default || fim == default)
            {
                return BadRequest(
                    "Informe as datas de início e fim.");
            }

            if (inicio > fim)
            {
                return BadRequest(
                    "A data inicial não pode ser posterior à final.");
            }

            // Limitar o período para evitar consultas excessivas
            if (fim.DayNumber - inicio.DayNumber > 365)
            {
                return BadRequest(
                    "O período máximo permitido é de 366 dias.");
            }

            // Horário de funcionamento: 08h às 19h
            const double horasPorDia = 11;

            var quantidadeDias = fim.DayNumber - inicio.DayNumber + 1;
            var totalHorasPeriodo = quantidadeDias * horasPorDia;

            var fusoBrasilia = TimeZoneInfo.FindSystemTimeZoneById(
                "America/Sao_Paulo");

            var inicioLocal = inicio.ToDateTime(
                TimeOnly.MinValue,
                DateTimeKind.Unspecified);

            var fimExclusivoLocal = fim.AddDays(1).ToDateTime(
                TimeOnly.MinValue,
                DateTimeKind.Unspecified);

            var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(
                inicioLocal, fusoBrasilia);

            var fimUtc = TimeZoneInfo.ConvertTimeToUtc(
                fimExclusivoLocal, fusoBrasilia);

            // Buscar salas cadastradas
            var salas = await _context.Salas
                .AsNoTracking()
                .OrderBy(s => s.Andar)
                .ThenBy(s => s.Nome)
                .ToListAsync();

            // Buscar reservas que intersectam o período
            var reservas = await _context.Reservas
                .AsNoTracking()
                .Where(r =>
                    r.Inicio < fimUtc &&
                    r.Fim > inicioUtc)
                .ToListAsync();

            var resultado = new List<object>();

            foreach (var sala in salas)
            {
                double horasOcupadas = 0;

                foreach (var reserva in reservas
                    .Where(r => r.SalaId == sala.Id))
                {
                    var inicioReserva = TimeZoneInfo.ConvertTimeFromUtc(
                        reserva.Inicio, fusoBrasilia);

                    var fimReserva = TimeZoneInfo.ConvertTimeFromUtc(
                        reserva.Fim, fusoBrasilia);

                    // Percorrer cada dia solicitado
                    for (var dia = inicio;
                         dia <= fim;
                         dia = dia.AddDays(1))
                    {
                        var abertura = dia.ToDateTime(
                            new TimeOnly(8, 0));

                        var fechamento = dia.ToDateTime(
                            new TimeOnly(19, 0));

                        var inicioOcupado =
                            inicioReserva > abertura
                                ? inicioReserva
                                : abertura;

                        var fimOcupado =
                            fimReserva < fechamento
                                ? fimReserva
                                : fechamento;

                        if (fimOcupado > inicioOcupado)
                        {
                            horasOcupadas +=
                                (fimOcupado - inicioOcupado).TotalHours;
                        }
                    }
                }

                var horasLivres = Math.Max(
                    0, totalHorasPeriodo - horasOcupadas);

                resultado.Add(new
                {
                    salaId = sala.Id,
                    nome = sala.Nome,
                    andar = sala.Andar,
                    horasLivres = Math.Round(horasLivres, 2)
                });
            }

            return Ok(resultado);
        }

        // PUT: api/salas/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> AtualizarSala(
            int id,
            [FromBody] Sala salaAtualizada)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido.");
            }

            if (salaAtualizada.Id != 0 &&
                salaAtualizada.Id != id)
            {
                return BadRequest(
                    "O ID informado não corresponde ao da sala.");
            }

            if (string.IsNullOrWhiteSpace(salaAtualizada.Nome))
            {
                return BadRequest(
                    "O nome da sala é obrigatório.");
            }

            var sala = await _context.Salas.FindAsync(id);

            if (sala == null)
            {
                return NotFound("Sala não encontrada.");
            }

            sala.Nome = salaAtualizada.Nome.Trim();
            sala.Andar = salaAtualizada.Andar;
            sala.QuantidadeAssentos =
                salaAtualizada.QuantidadeAssentos;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/salas/11
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> ExcluirSala(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido.");
            }

            var sala = await _context.Salas.FindAsync(id);

            if (sala == null)
            {
                return NotFound("Sala não encontrada.");
            }

            _context.Salas.Remove(sala);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
