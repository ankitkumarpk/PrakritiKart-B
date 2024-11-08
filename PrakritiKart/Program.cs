using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PrakritiKart.Interfaces;
using PrakritiKart.Services;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// Add Dapper connection string settings
builder.Services.AddScoped<ICustomerService>(provider =>
    new CustomerService(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISellerService>(provider =>
new SellerService(builder.Configuration.GetConnectionString("DefaultConnection"),  provider.GetRequiredService<IWebHostEnvironment>()));

// Configure JWT authentication
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline and use swagger
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Ensure exception handler is in place for production
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyHeader()
          .AllowAnyMethod()
);

// Use authentication and authorization middleware
app.UseAuthentication(); // Add this line
app.UseAuthorization();

app.MapControllers();

app.Run();
