using System.Text;
using AleeDotNet_VillaNumberAPI.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;
using Villa_API;
using Villa_API.Data;
using Villa_API.Models;
using Villa_API.Repository;
using Villa_API.Repository.IRepository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddResponseCaching();

builder.Services.AddScoped<IVillaRepository, VillaRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVillaNumberRepository, VillaNumberRepository>();
builder.Services.AddAutoMapper(typeof(MappingConfig));

// add versioning to services
builder.Services.AddApiVersioning(options =>
{
    // Thiết lập này giả định rằng phiên bản API mặc định sẽ được sử dụng khi một client không chỉ định phiên bản
    options.AssumeDefaultVersionWhenUnspecified = true;
    // Thiết lập này xác định phiên bản API mặc định là 1.0.
    options.DefaultApiVersion = new ApiVersion(1, 0);
    // show the available API version in response header
    options.ReportApiVersions = true;
});
builder.Services.AddVersionedApiExplorer(options =>
{
    // Thiết lập này định dạng tên nhóm cho các phiên bản API. 'v'VVV sẽ định dạng các phiên bản API dưới dạng "v1", "v2",...
    options.GroupNameFormat = "'v'VVV";
    // Auto choose default version
    options.SubstituteApiVersionInUrl = true;
});

var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");

builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            // ValidateIssuer = false,
            // ValidateAudience = false,
            ValidateIssuer = true,
            ValidIssuer = "https://magicvilla-api.com",
            ValidAudience = "ABC-company.com",
            ValidateAudience = true,
            ClockSkew = TimeSpan.Zero // so important
        };
    });
;

// AddNewtonsoftJson() is used for MVC.NewtonsoftJson package
// AddXmlDataContractSerializerFormatters() is used for supporting XML formating
builder.Services.AddControllers(option =>
{
    // option.CacheProfiles.Add("Default30",
    //     new CacheProfile
    //     {
    //         Duration = 30
    //     });

    // option.ReturnHttpNotAcceptable = true;
    // if a format is not acceptable, return the appropriate error message
}).AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, AleeConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Alee_VillaV2");
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Alee_VillaV1");
    });
}

app.UseStaticFiles(); // render wwwroot

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

ApplyMigration();

app.Run();

void ApplyMigration()
{
    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (_db.Database.GetPendingMigrations().Count() > 0)
        {
            _db.Database.Migrate();
        }
    }
}