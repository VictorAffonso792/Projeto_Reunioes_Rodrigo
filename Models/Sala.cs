
using System.ComponentModel.DataAnnotations;

namespace Reunioes.Api.Models
{
    public class Sala
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da sala é obrigatório.")]
        public string Nome { get; set; } = "";

        [Range(0, int.MaxValue,
            ErrorMessage = "O andar não pode ser negativo.")]
        public int Andar { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "A quantidade de assentos deve ser maior que zero.")]
        public int QuantidadeAssentos { get; set; }
    }
}
