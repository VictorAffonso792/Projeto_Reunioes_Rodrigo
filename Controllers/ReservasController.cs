
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reunioes.Api.Data;
using Reunioes.Api.Models;

namespace Reunioes.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReservasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/reservas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reserva>>> ListarReservas()
        {
            var reservas = await _context.Reservas
                .AsNoTracking()
                .OrderBy(r => r.Inicio)
                .ThenBy(r => r.Id)
                .ToListAsync();

            return Ok(reservas);
        }

        // GET: api/reservas/3
        [HttpGet("{id}")]
        public async Task<ActionResult<Reserva>> ConsultarReserva(int id)
        {
            var reserva = await _context.Reservas
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
            {
                return NotFound("Reserva não encontrada.");
            }

            return Ok(reserva);
        }

        // POST: api/reservas
        [HttpPost]
        public async Task<ActionResult<Reserva>> CadastrarReserva(
            [FromBody] Reserva reserva)
        {
            if (reserva.Id != 0)
            {
                return BadRequest(
                    "O ID deve ser gerado pelo banco de dados.");
            }

            if (reserva.Inicio.Kind != DateTimeKind.Utc ||
                reserva.Fim.Kind != DateTimeKind.Utc)
            {
                return BadRequest(
                    "As datas devem ser informadas em UTC, com Z no final.");
            }

            var salaExiste = await _context.Salas
                .AnyAsync(s => s.Id == reserva.SalaId);

            if (!salaExiste)
            {
                return BadRequest("A sala informada não existe.");
            }

            if (reserva.Inicio >= reserva.Fim)
            {
                return BadRequest(
                    "O início deve ser anterior ao fim da reserva.");
            }

            var fusoBrasilia = TimeZoneInfo.FindSystemTimeZoneById(
                "America/Sao_Paulo");

            var inicioLocal = TimeZoneInfo.ConvertTimeFromUtc(
                reserva.Inicio, fusoBrasilia);

            var fimLocal = TimeZoneInfo.ConvertTimeFromUtc(
                reserva.Fim, fusoBrasilia);

            if (inicioLocal.Date != fimLocal.Date ||
                inicioLocal.TimeOfDay < TimeSpan.FromHours(8) ||
                fimLocal.TimeOfDay > TimeSpan.FromHours(19))
            {
                return BadRequest(
                    "As reservas devem ocorrer no mesmo dia, " +
                    "entre 08h e 19h, no horário de Brasília.");
            }

            var existeConflito = await _context.Reservas
                .AnyAsync(r =>
                    r.SalaId == reserva.SalaId &&
                    r.Inicio < reserva.Fim &&
                    reserva.Inicio < r.Fim);

            if (existeConflito)
            {
                return Conflict(
                    "Já existe uma reserva para essa sala nesse horário.");
            }

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            return Created(
                $"/api/reservas/{reserva.Id}",
                reserva);
        }

        // PUT: api/reservas/3
        [HttpPut("{id}")]
        public async Task<IActionResult> RemarcarReserva(
            int id,
            [FromBody] Reserva dadosAtualizados)
        {
            // Buscar a reserva existente
            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
            {
                return NotFound("Reserva não encontrada.");
            }

            // Impedir a remarcação de reservas passadas
            if (reserva.Inicio <= DateTime.UtcNow)
            {
                return BadRequest(
                    "Somente reservas futuras podem ser remarcadas.");
            }

            // Validar o ID informado no corpo
            if (dadosAtualizados.Id != 0 &&
                dadosAtualizados.Id != id)
            {
                return BadRequest(
                    "O ID informado não corresponde à reserva.");
            }

            // Exigir horários em UTC
            if (dadosAtualizados.Inicio.Kind != DateTimeKind.Utc ||
                dadosAtualizados.Fim.Kind != DateTimeKind.Utc)
            {
                return BadRequest(
                    "As datas devem ser informadas em UTC, com Z no final.");
            }

            // Verificar se a sala existe
            var salaExiste = await _context.Salas
                .AnyAsync(s => s.Id == dadosAtualizados.SalaId);

            if (!salaExiste)
            {
                return BadRequest("A sala informada não existe.");
            }

            // Validar a ordem dos horários
            if (dadosAtualizados.Inicio >= dadosAtualizados.Fim)
            {
                return BadRequest(
                    "O início deve ser anterior ao fim da reserva.");
            }

            // Impedir remarcação para uma data passada
            if (dadosAtualizados.Inicio <= DateTime.UtcNow)
            {
                return BadRequest(
                    "O novo início da reserva deve estar no futuro.");
            }

            // Converter os horários para Brasília
            var fusoBrasilia = TimeZoneInfo.FindSystemTimeZoneById(
                "America/Sao_Paulo");

            var inicioLocal = TimeZoneInfo.ConvertTimeFromUtc(
                dadosAtualizados.Inicio, fusoBrasilia);

            var fimLocal = TimeZoneInfo.ConvertTimeFromUtc(
                dadosAtualizados.Fim, fusoBrasilia);

            // Validar horário de funcionamento
            if (inicioLocal.Date != fimLocal.Date ||
                inicioLocal.TimeOfDay < TimeSpan.FromHours(8) ||
                fimLocal.TimeOfDay > TimeSpan.FromHours(19))
            {
                return BadRequest(
                    "As reservas devem ocorrer no mesmo dia, " +
                    "entre 08h e 19h, no horário de Brasília.");
            }

            // Verificar conflitos, ignorando a própria reserva
            var existeConflito = await _context.Reservas
                .AnyAsync(r =>
                    r.Id != id &&
                    r.SalaId == dadosAtualizados.SalaId &&
                    r.Inicio < dadosAtualizados.Fim &&
                    dadosAtualizados.Inicio < r.Fim);

            if (existeConflito)
            {
                return Conflict(
                    "Já existe uma reserva para essa sala nesse horário.");
            }

            // Atualizar os dados da reserva
            reserva.SalaId = dadosAtualizados.SalaId;
            reserva.Inicio = dadosAtualizados.Inicio;
            reserva.Fim = dadosAtualizados.Fim;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
