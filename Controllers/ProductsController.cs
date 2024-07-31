using HPlusSport.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HPlusSport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        // Puxando o context e armazenando em uma propriedade para ser usado em todos os verbos da API
        private readonly ShopContext _context;

        // Agora vou passar ele pra dentro do construtor (Usamos injeção de dependência)
        public ProductsController(ShopContext context)
        {
            _context = context;

            // Rodar a seed e garantir que foi criado
            _context.Database.EnsureCreated();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts([FromQuery] ProductQueryParameters queryParameters)
        {

            IQueryable<Product> products = _context.Products;


            // Remover os que não se encaixam nos limites do Min e Max
            if (queryParameters.MinPrice != null)
            {
                products = products.Where(
                    p => p.Price >= queryParameters.MinPrice.Value
                );
            }

            if (queryParameters.MaxPrice != null)
            {
                products = products.Where(
                    p => p.Price <= queryParameters.MaxPrice.Value
                );
            }
            if (!string.IsNullOrEmpty(queryParameters.SKU))
            {
                products = products.Where(
                    p => p.Sku == queryParameters.SKU);
            }

            if (!string.IsNullOrEmpty(queryParameters.Name))
            {
                products = products.Where(
                    p => p.Name.ToLower().Contains(
                        queryParameters.Name.ToLower()));
            }

            // Iniciando o Sort
            // Verificando se tem algo para ordenarmos
            if (!string.IsNullOrEmpty(queryParameters.SortBy))
            {
                if(typeof(Product).GetProperty(queryParameters.SortBy) != null)
                {
                    // Aqui o método de extensão pode brilhar
                    products = products.OrderByCustom(
                        queryParameters.SortBy,
                        queryParameters.SortOrder
                    );
                }
            }

            products = products
            .Skip(queryParameters.Size * (queryParameters.Page - 1))
            .Take(queryParameters.Size);

            return Ok(await products.ToListAsync());
        }

        [HttpGet, Route("{id}")]
        public async Task<ActionResult> GetProduct(int id)
        {
            // Busca
            var product = await _context.Products.FindAsync(id); // Vai encontrar o produto pelo ID

            // Validações
            if (product == null)
            { // Se o produto não existir, retorne 404
                return NotFound();
            }

            // Retorno
            return Ok(product); // Vai retornar o produto encontrado
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<Product>>> GetAvailabeProducts()
        {
            var product = await _context.Products
            .OrderBy(p => p.Name) // Ordena, neste caso por nome (A a Z)
            .Where(p => p.IsAvailable) // Encontra todos os IsAvailable que são true
            .Select(p => new { p.Id, p.Name, p.Price }) // Serve para retornar apenas estes valores
            .ToArrayAsync(); // Coloca em formato de lista (Materializar a Consulta)

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult> PostProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutProduct(int id, Product product)
        {
            // Verificando se é o mesmo item
            if (id != product.Id)
            {
                return BadRequest();
            }

            // Substituindo a informação
            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) // Caso a API esteja sendo atualizada por outra requisição
            {
                if (!_context.Products.Any(p => p.Id == id)) // Procurando em (Qualquer) Any Products que tem o ID fornecido
                {
                    return NotFound();
                }
            }


            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(product);
        }

        // Exercício de Deletar mais de 1 item.
        [HttpDelete("Delete")]
        public async Task<ActionResult> DeleteManyProducts(int[] ids)
        {
            var products = new List<Product>();
            foreach (var id in ids)
            {
                var product = await _context.Products.FindAsync(id);

                // Tratamento para quando não houver o ID
                if (product == null)
                {
                    return NotFound();
                }

                // Caso o id exista
                products.Add(product);
            }

            // Removendo o Range dos IDs
            _context.RemoveRange(products);
            await _context.SaveChangesAsync();

            return Ok(products);
        }

    }
}
