using Microsoft.EntityFrameworkCore;
using Omni.Models.Entidades;
using Omnimarket.Api.Models;
using Omnimarket.Api.Models.Entidades;

namespace Omnimarket.Api.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        // Cada DbSet representa uma tabela manipulada pelo Entity Framework.
        public DbSet<Usuario> TBL_USUARIO { get; set; }
        public DbSet<Endereco> TBL_ENDERECO { get; set; }
        public DbSet<Telefone> TBL_TELEFONE { get; set; }
        public DbSet<Produto> TBL_PRODUTO { get; set; }
        public DbSet<ProdutoMidia> ProdutoMidia => Set<ProdutoMidia>();
        public DbSet<Pedido> TBL_PEDIDO { get; set; }
        public DbSet<ItensPedido> TBL_ITENS_PEDIDO { get; set; }
        public DbSet<Carrinho> TBL_CARRINHO { get; set; }
        public DbSet<ItemCarrinho> TBL_ITEM_CARRINHO { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mapeia explicitamente as entidades para as tabelas do banco.
            modelBuilder.Entity<Usuario>().ToTable("TBL_USUARIO");
            modelBuilder.Entity<Endereco>().ToTable("TBL_ENDERECO");
            modelBuilder.Entity<Telefone>().ToTable("TBL_TELEFONE");
            modelBuilder.Entity<Produto>().ToTable("TBL_PRODUTOS");
            modelBuilder.Entity<ProdutoMidia>().ToTable("TBL_PRODUTOS_MIDIA");
            modelBuilder.Entity<Pedido>().ToTable("TBL_PEDIDO");
            modelBuilder.Entity<ItensPedido>().ToTable("TBL_ITENS_PEDIDO");

            // Define relacionamentos e comportamento de exclusao em cascata.
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Telefones)
                .WithOne(t => t.Usuario)
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Enderecos)
                .WithOne(e => e.Usuario)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Garante unicidade para os principais identificadores do usuario.
            modelBuilder.Entity<Usuario>().HasIndex(x => x.Cpf).IsUnique();
            modelBuilder.Entity<Usuario>().HasIndex(x => x.Email).IsUnique();

            // Salva enums de logradouro como texto no banco para leitura mais clara.
            modelBuilder.Entity<Endereco>()
                .Property(e => e.TipoLogradouro)
                .HasConversion<string>();

            // Se o produto for removido, suas midias tambem sao removidas.
            modelBuilder.Entity<Produto>()
                .HasMany(p => p.Midias)
                .WithOne(m => m.Produto)
                .HasForeignKey(m => m.ProdutoId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Convencao padrao para strings no banco.
            configurationBuilder.Properties<string>()
                .HaveColumnType("varchar")
                .HaveMaxLength(200);
        }
    }
}
