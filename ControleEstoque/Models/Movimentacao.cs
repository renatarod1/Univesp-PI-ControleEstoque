using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControleEstoque.Models {
    public class Movimentacao {
        public int Id { get; set; }

        [Required]
        public int ProdutoId { get; set; }

        [ForeignKey(nameof(ProdutoId))]
        public Produto? Produto { get; set; }

        [Required]
        [Display(Name = "Preço")]
        [Range(0.01, 999999.99)]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Preco { get; set; } = 0.00m;

        [Required]
        public TipoMovimentacao Tipo { get; set; }

        [Range(1, 999999, ErrorMessage = "O campo Quantidade deve estar entre 1 e 999999.")]
        public int Quantidade { get; set; }

        public DateTime Data { get; set; } = DateTime.Now;
    }
}
