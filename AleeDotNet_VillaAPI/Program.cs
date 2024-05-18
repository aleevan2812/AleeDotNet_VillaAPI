using Alee_VillaAPI.Data;
using AleeDotNet_VillaAPI;
using AleeDotNet_VillaAPI.Repository;
using AleeDotNet_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IVillaRepository, VillaRepository>();
builder.Services.AddAutoMapper(typeof(MappingConfig));

// AddNewtonsoftJson() is used for MVC.NewtonsoftJson package
// AddXmlDataContractSerializerFormatters() is used for supporting XML formating
builder.Services.AddControllers(option =>
{
	// option.ReturnHttpNotAcceptable = true;
	// if a format is not acceptable, return the appropriate error message
}).AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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