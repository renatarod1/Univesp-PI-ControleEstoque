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

        [Required(ErrorMessage = "O preço é obrigatório.")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        [Range(0.01, 9999999999999999.99, ErrorMessage = "O preço deve estar entre 0,01 e 9999999999999999,99.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Preço")]
        [Precision(18, 2)]
        public decimal Preco { get; set; }

        [Required]
        public TipoMovimentacao Tipo { get; set; }

        [Range(1, 999999)]
        public int Quantidade { get; set; }

        public DateTime Data { get; set; } = DateTime.Now;
    }
}
