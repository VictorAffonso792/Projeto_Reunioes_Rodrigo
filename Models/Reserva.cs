
using System;
using System.ComponentModel.DataAnnotations;

namespace Reunioes.Api.Models
{
    public class Reserva
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "A sala é obrigatória.")]
        public int SalaId { get; set; }

        [Required]
        public DateTime Inicio { get; set; }

        [Required]
        public DateTime Fim { get; set; }

        // Sala associada à reserva
        public Sala? Sala { get; set; }
    }
}
