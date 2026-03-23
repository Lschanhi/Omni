using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Omnimarket.Api.Data;
using Omnimarket.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔗 BANCO DE DADOS
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexaoLocal"));
});

// 🔐 JWT AUTH
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"], // 🔥 corrigido (sem espaço)
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        )
    };
});

// ❌ REMOVIDO (estava duplicado)
// builder.Services.AddAuthentication();

// 📦 SERVICES
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<PedidoService>();

// 🔥 CPF SERVICE (DESCOMENTE QUANDO USAR)
builder.Services.AddHttpClient<ICpfService, CpfService>();

// 🎯 CONTROLLERS + ENUM COMO STRING
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// 📄 SWAGGER / OPENAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🔐 ORDEM IMPORTANTE
app.UseHttpsRedirection();

app.UseAuthentication(); // 🔥 primeiro
app.UseAuthorization();

// 📄 SWAGGER EM DEV
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🎯 CONTROLLERS
app.MapControllers();

app.Run();