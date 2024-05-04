var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// AddNewtonsoftJson() is used for MVC.NewtonsoftJson package
// AddXmlDataContractSerializerFormatters() is used for supporting XML formating
builder.Services.AddControllers(option =>
{
	option.ReturnHttpNotAcceptable = true;
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
