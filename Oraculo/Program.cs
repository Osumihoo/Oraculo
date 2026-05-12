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

builder.Services.AddScoped<IBackupRepository, BackupRepository>();
builder.Services.AddScoped<IBranchManagersRepository, BranchManagersRepository>();
builder.Services.AddScoped<IITRepository, ITRepository>();
builder.Services.AddScoped<IKioskRepository, KioskRepository>();
builder.Services.AddScoped<IR2QInvoicesRepository, R2QInvoicesRepository>();
builder.Services.AddScoped<IRRHHRepository, RRHHRepository>();
builder.Services.AddScoped<IWoocommerceRepository, WoocommerceRepository>();

////////////////////////////////////////////////////////////////////////////////////////

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

////////////////////////////////////////////////////////////////////////////////////////

var app = builder.Build();

app.UseCors("AllowAll");

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
