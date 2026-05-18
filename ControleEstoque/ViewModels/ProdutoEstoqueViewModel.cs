namespace ControleEstoque.ViewModels {
    public class ProdutoEstoqueViewModel {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public int EstoqueAtual { get; set; }

        public int QtdEstoqueAlerta { get; set; }

        public bool Alerta { get; set; }
    }
}
