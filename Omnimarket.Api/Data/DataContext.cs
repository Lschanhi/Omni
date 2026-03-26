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

        public DbSet<Usuario> TBL_USUARIO { get; set; }
        public DbSet<Endereco> TBL_ENDERECO { get; set; }
        public DbSet<Telefone> TBL_TELEFONE { get; set; }
        public DbSet<Produto> TBL_PRODUTO { get; set; }
        public DbSet<ProdutoMidia> ProdutoMidia => Set<ProdutoMidia>();
        public DbSet<Pedido> TBL_PEDIDO { get; set; }
        public DbSet<ItensPedido> TBL_ITENS_PEDIDO { get; set; }
        public DbSet<Carrinho> TBL_CARRINHO { get; set; }
        public DbSet<ItemCarrinho> TBL_ITEM_CARRINHO { get; set; }
        public DbSet<Loja> TBL_LOJA { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>().ToTable("TBL_USUARIO");
            modelBuilder.Entity<Endereco>().ToTable("TBL_ENDERECO");
            modelBuilder.Entity<Telefone>().ToTable("TBL_TELEFONE");
            modelBuilder.Entity<Produto>().ToTable("TBL_PRODUTOS");
            modelBuilder.Entity<ProdutoMidia>().ToTable("TBL_PRODUTOS_MIDIA");
            modelBuilder.Entity<Pedido>().ToTable("TBL_PEDIDO");
            modelBuilder.Entity<ItensPedido>().ToTable("TBL_ITENS_PEDIDO");
            modelBuilder.Entity<Carrinho>().ToTable("TBL_CARRINHO");
            modelBuilder.Entity<ItemCarrinho>().ToTable("TBL_ITEM_CARRINHO");
            modelBuilder.Entity<Loja>().ToTable("TBL_LOJA");

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

            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Produtos)
                .WithOne(p => p.Usuario)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Loja)
                .WithOne(l => l.Usuario)
                .HasForeignKey<Loja>(l => l.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Usuario>().HasIndex(x => x.Cpf).IsUnique();
            modelBuilder.Entity<Usuario>().HasIndex(x => x.Email).IsUnique();
            modelBuilder.Entity<Carrinho>().HasIndex(c => c.UsuarioId).IsUnique();
            modelBuilder.Entity<Produto>().HasIndex(p => p.Sku).IsUnique();
            modelBuilder.Entity<Loja>().HasIndex(l => l.UsuarioId).IsUnique();
            modelBuilder.Entity<Loja>().HasIndex(l => l.Slug).IsUnique();

            modelBuilder.Entity<Endereco>()
                .Property(e => e.TipoLogradouro)
                .HasConversion<string>();

            modelBuilder.Entity<Produto>()
                .Property(p => p.StatusPublicacao)
                .HasConversion<string>();

            modelBuilder.Entity<Produto>()
                .HasMany(p => p.Midias)
                .WithOne(m => m.Produto)
                .HasForeignKey(m => m.ProdutoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Carrinho>()
                .HasMany(c => c.Itens)
                .WithOne(i => i.Carrinho)
                .HasForeignKey(i => i.CarrinhoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Pedido>()
                .HasMany(p => p.Itens)
                .WithOne(i => i.Pedido)
                .HasForeignKey(i => i.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<string>()
                .HaveColumnType("varchar")
                .HaveMaxLength(200);

            configurationBuilder.Properties<decimal>()
                .HaveColumnType("decimal(18,2)");

            configurationBuilder.Properties<decimal?>()
                .HaveColumnType("decimal(18,2)");
        }
    }
}
