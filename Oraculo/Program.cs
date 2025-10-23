using Oraculo.Data.Repositories;
using Sap.Data.Hana;

var builder = WebApplication.CreateBuilder(args);

//////////////////////////////////////////////////////////////////////////////////////
///

builder.Services.AddScoped<Func<int, HanaConnection>>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return env =>
    {
        string connString = env == 0
            ? config.GetConnectionString("HanaConnection_Prod")
            : config.GetConnectionString("HanaConnection_Test");

        return new HanaConnection(connString);
    };
});

//////////////////////////////////////////////////////////////////////////////////////

// Obtener la cadena de conexión desde appsettings.json
//string hanaConnectionString = builder.Configuration.GetConnectionString("HanaConnection");
// Inyectar la conexión en los servicios
//builder.Services.AddScoped<HanaConnection>(_ => new HanaConnection(hanaConnectionString));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

////////////////////////////////////////////////////////////////////////////////////////
///

builder.Services.AddScoped<IWoocommerceRepository, WoocommerceRepository>();
builder.Services.AddScoped<IR2QInvoicesRepository, R2QInvoicesRepository>();
builder.Services.AddScoped<IRRHHRepository, RRHHRepository>();
builder.Services.AddScoped<IBranchManagersRepository, BranchManagersRepository>();

////////////////////////////////////////////////////////////////////////////////////////


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
