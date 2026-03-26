using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Omnimarket.Api.Data;
using Omnimarket.Api.Services;
using Omnimarket.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Chave usada para assinar e validar os tokens JWT da aplicacao.
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);

// Registra o contexto do Entity Framework apontando para o banco SQL Server.
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexaoLocal"));
});

// Configura a autenticacao da API para usar JWT em todos os endpoints protegidos.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Servicos de negocio que serao injetados nos controllers.
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<RegistrarService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();

// Cliente HTTP usado pelo servico externo de validacao de CPF.
builder.Services.AddHttpClient<ICpfService, CpfService>();

// Configura os controllers e serializa enums como texto no JSON.
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Swagger facilita a exploracao e o teste dos endpoints em desenvolvimento.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Pipeline HTTP da aplicacao.
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Mapeia as rotas declaradas nos controllers.
app.MapControllers();

app.Run();
