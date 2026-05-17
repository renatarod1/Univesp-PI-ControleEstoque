using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Models {
    public class Produto {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        public int QtdEstoqueAlerta { get; set; }

        public ICollection<Movimentacao>? Movimentacoes { get; set; }
    }
}