using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Models {
    public class Produto {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [Range(1, 999999, ErrorMessage = "O campo Quantidade Mínima deve estar entre 1 e 999999.")]
        public int QtdEstoqueAlerta { get; set; }

        public ICollection<Movimentacao>? Movimentacoes { get; set; }
    }
}