using ApiEcommerce.Repository.IRepository;
using ApiEcommerce.RepositoryM;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);
var dbConnectionString = builder.Configuration.GetConnectionString("ConexionSql");
// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options=>options.UseSqlServer(dbConnectionString));
builder.Services.AddScoped<ICategoryRepository,CategoryRepository>();
builder.Services.AddScoped<IProductRepository,ProductRepository>();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
