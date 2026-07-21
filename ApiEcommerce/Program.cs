using ApiEcommerce.Repository.IRepository;
using ApiEcommerce.RepositoryM;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using ApiEcommerce.Repository;
using ApiEcommerce.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var dbConnectionString = builder.Configuration.GetConnectionString("ConexionSql");
// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options=>options.UseSqlServer(dbConnectionString));
builder.Services.AddScoped<ICategoryRepository,CategoryRepository>();
builder.Services.AddScoped<IProductRepository,ProductRepository>();
builder.Services.AddScoped<IUserRepository,UserRepository>();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

var secretKey = builder.Configuration.GetValue<string>("ApiSettings:SecretKey");
if(string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("SecretKey no esta configurada");
}
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}
).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;//desactiva necesidad de activar https
    options.SaveToken = true;//guarda el token en el contexto de autenticacion
    options.TokenValidationParameters = new TokenValidationParameters // defino paarametros
    {
        ValidateIssuerSigningKey = true,//verifico que el token este firmado con clave válida
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),//verifico que sea UTF8, estableciendo la clave secreta para validar el token
        ValidateIssuer = false,//No valido el emisor del token
        ValidateAudience = true//No se valida el publico del token si no se necesita restringir a ciertos clientes
    };
});
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options=>
    {
        options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            //puedo especificar el http o con * permito todos
            builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();

        }
        );
    }
);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(PolicyNames.AllowSpecificOrigin);
app.UseAuthorization();
app.MapControllers();

app.Run();
