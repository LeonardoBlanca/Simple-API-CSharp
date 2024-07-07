using Microsoft.EntityFrameworkCore;

namespace HPlusSport.API.Models
{
    public class ShopContext : DbContext // Será chamada na Controller
    {

        // Começar pelo constructor
        public ShopContext(DbContextOptions<ShopContext> options) : base (options) 
        {
        }

        /* O método abaixo é chamado automaticamente quando passo o contexto, neste caso ShopContext. O método precisa obrigatoriamente
        ter o nome de OnModelCreating, porque ele vai passar essas configurações quando eu chamar a aplicação.
        É aqui aonde defino a estrutura do banco de dados.
        
            - Relacionamentos: Como as entidades se relacionam entre si.
            - Restrições: Chaves primárias, Chaves Estrangeira (ForeignKey), Índices, etc.
            - Configurações adicionais: Nomes de tabelas, mapeamento de propriedades para colunas, etc
            - Dados iniciais (Seed): Se você tiver um método Seed, em uma classe de extensão, como no meu exemplo, ela também
              será chamada para popular o banco de dados. (Meu Seed está dentro do ModelBuilderExtensions.cs)
        */
        protected override void OnModelCreating(ModelBuilder modelBuilder) // Este método está sendo herdado da DbContext
        {
            modelBuilder.Entity<Category>() // Configurando a entidade Category
                .HasMany(c => c.Products) // Configurando que a categoria pode ter muitos produtos (One to Many)
                .WithOne(c => c.Category) // Configurando que cada produuto deve ter apenas uma categoria
                .HasForeignKey(a => a.CategoryId); // A chave estrangeira CategoryId é usada para estabelecer o relacionamento.

            modelBuilder.Seed(); // Chamando o método do arquivo ModelBuilderExtensions.cs para popular o banco de dados
        }
        public DbSet<Product> Products { get; set;} // Cria um DbSet para a entidade product (usado para fazer CRUD)
        public DbSet<Category> Categories { get; set;}
    }
}
