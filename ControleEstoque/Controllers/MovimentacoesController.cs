using ControleEstoque.Data;
using ControleEstoque.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ControleEstoque.Controllers
{
    public class MovimentacoesController : Controller
    {
        private readonly AppDbContext _context;

        public MovimentacoesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Movimentacoes
        public async Task<IActionResult> Index(
            int? produtoId,
            TipoMovimentacao? tipo,
            DateTime? dataInicio,
            DateTime? dataFim) {
            var movimentacoes = _context.Movimentacoes
                .Include(m => m.Produto)
                .AsQueryable();

            // FILTRO PRODUTO
            if (produtoId.HasValue) {
                movimentacoes = movimentacoes
                    .Where(m => m.ProdutoId == produtoId.Value);
            }

            // FILTRO TIPO
            if (tipo.HasValue) {
                movimentacoes = movimentacoes
                    .Where(m => m.Tipo == tipo.Value);
            }

            // FILTRO DATA INICIAL
            if (dataInicio.HasValue) {
                movimentacoes = movimentacoes
                    .Where(m => m.Data >= dataInicio.Value);
            }

            // FILTRO DATA FINAL
            if (dataFim.HasValue) {
                movimentacoes = movimentacoes
                    .Where(m => m.Data <= dataFim.Value);
            }

            var lista = await movimentacoes
                .OrderByDescending(m => m.Data)
                .ToListAsync();

            // =========================================
            // RESUMO FINANCEIRO FILTRADO
            // =========================================
            decimal valorEntradas = lista
                .Where(m => m.Tipo == TipoMovimentacao.Entrada)
                .Sum(m => m.Quantidade * m.Preco);

            decimal valorSaidas = lista
                .Where(m => m.Tipo == TipoMovimentacao.Saida)
                .Sum(m => m.Quantidade * m.Preco);

            decimal valorTotalEstoque = valorEntradas - valorSaidas;

            int qtdEntradas = lista
                .Where(m => m.Tipo == TipoMovimentacao.Entrada)
                .Sum(m => m.Quantidade);

            int qtdSaidas = lista
                .Where(m => m.Tipo == TipoMovimentacao.Saida)
                .Sum(m => m.Quantidade);

            int saldoEstoque = qtdEntradas - qtdSaidas;

            // =========================================
            // RESUMO POR PRODUTO
            // =========================================

            var resumoProdutos = lista
                .GroupBy(m => m.Produto!.Nome)
                .Select(g => new {
                    Produto = g.Key,

                    Quantidade = g.Sum(m =>
                        m.Tipo == TipoMovimentacao.Entrada
                            ? m.Quantidade
                            : -m.Quantidade),

                    Valor = g.Sum(m =>
                        m.Tipo == TipoMovimentacao.Entrada
                            ? m.Quantidade * m.Preco
                            : -m.Quantidade * m.Preco)
                })
                .ToList();

            // VIEWBAG
            ViewBag.ValorTotalEstoque = valorTotalEstoque;
            ViewBag.SaldoEstoque = saldoEstoque;
            ViewBag.ResumoProdutos = resumoProdutos;

            // DROPDOWN PRODUTOS
            ViewBag.Produtos = new SelectList(
                _context.Produtos,
                "Id",
                "Nome",
                produtoId
            );

            // VIEWDATA
            ViewData["TipoFiltro"] = tipo;
            ViewData["DataInicio"] = dataInicio?.ToString("yyyy-MM-dd");
            ViewData["DataFim"] = dataFim?.ToString("yyyy-MM-dd");

            return View(lista);
        }

        // GET: Movimentacoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimentacao = await _context.Movimentacoes
                .Include(m => m.Produto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movimentacao == null)
            {
                return NotFound();
            }

            return View(movimentacao);
        }

        // GET: Movimentacoes/Create
        public IActionResult Create()
        {
            ViewData["ProdutoId"] = new SelectList(_context.Produtos, "Id", "Nome");
            return View(new Movimentacao());
        }

        // POST: Movimentacoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProdutoId,Preco,Tipo,Quantidade,Observacao,Data")] Movimentacao movimentacao)
        {
            // =========================================
            // SAÍDA → BUSCA ÚLTIMO PREÇO AUTOMATICAMENTE
            // =========================================

            if (movimentacao.Tipo == TipoMovimentacao.Saida) {
                var ultimaEntrada = await _context.Movimentacoes
                    .Where(m =>
                        m.ProdutoId == movimentacao.ProdutoId &&
                        m.Tipo == TipoMovimentacao.Entrada)
                    .OrderByDescending(m => m.Data)
                    .FirstOrDefaultAsync();

                if (ultimaEntrada == null) {
                    ModelState.AddModelError(
                        "",
                        "Estoque insuficiente.");

                    ViewData["ProdutoId"] = new SelectList(
                        _context.Produtos,
                        "Id",
                        "Nome",
                        movimentacao.ProdutoId);

                    return View(movimentacao);
                }

                movimentacao.Preco = ultimaEntrada.Preco;
            }
            else {
                // ENTRADA (preço digitado pelo usuário)

                var valor = Request.Form["Preco"]
                    .ToString()
                    .Replace(".", ",");

                movimentacao.Preco = decimal.Parse(
                    valor,
                    new CultureInfo("pt-BR"));
            }

            // =========================================
            // VALIDA ESTOQUE
            // =========================================

            bool estoqueValido = await ValidarSaidaAsync(movimentacao);

            if (!estoqueValido) {
                ModelState.AddModelError("", "Estoque insuficiente.");

                ViewData["ProdutoId"] = new SelectList(
                    _context.Produtos,
                    "Id",
                    "Nome",
                    movimentacao.ProdutoId);

                return View(movimentacao);
            }

            // =========================================
            // SALVA
            // =========================================

            _context.Add(movimentacao);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        

        private bool MovimentacaoExists(int id)
        {
            return _context.Movimentacoes.Any(e => e.Id == id);
        }

        private async Task<bool> ValidarSaidaAsync(Movimentacao movimentacao) {
            if (movimentacao.Tipo != TipoMovimentacao.Saida)
                return true;

            var produto = await _context.Produtos
                .Include(p => p.Movimentacoes)
                .FirstOrDefaultAsync(p => p.Id == movimentacao.ProdutoId);

            if (produto == null)
                return false;

            int entradas = produto.Movimentacoes?
                .Where(m => m.Tipo == TipoMovimentacao.Entrada)
                .Sum(m => m.Quantidade) ?? 0;

            int saidas = produto.Movimentacoes?
                .Where(m => m.Tipo == TipoMovimentacao.Saida)
                .Sum(m => m.Quantidade) ?? 0;

            int estoqueAtual = entradas - saidas;

            return movimentacao.Quantidade <= estoqueAtual;
        }
    }
}
