using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Configuration;
using System.Text;
using WebApiTraining1;
using WebApiTraining1.Models;
using WebApiTraining1.Services;
using WebApiTraining1.ViewModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x =>
{
    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert token",
        Name = "Authorziation",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    x.AddSecurityRequirement(new OpenApiSecurityRequirement {
        { new OpenApiSecurityScheme {
           Reference = new OpenApiReference
           {
               Type =  ReferenceType.SecurityScheme,
               Id = "Bearer"
           }
       },
       new string[]{}
        }
    });
});

builder.Services.Configure<AppSetting>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:SecretKey"])),

        ClockSkew = TimeSpan.Zero
    };
});

var connectionString = builder.Configuration.GetConnectionString("MyDbConnection");
builder.Services.AddDbContext<MyDbContext>(x => x.UseSqlServer(connectionString));
builder.Services.AddScoped<IBookRepository, BookRepository>();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllersWithViews();

MapperConfiguration mappingConfig = new MapperConfiguration(mc =>
{
    mc.CreateMap<BookViewModel, Book>();
});

IMapper mapper = mappingConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MY API");
    });

}


app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());


app.UseHttpsRedirection();

app.UseMiddleware<JWTInHeaderMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
