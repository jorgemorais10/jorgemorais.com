using Hub.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Memory Cache
builder.Services.AddMemoryCache();

// HttpContext Accessor
builder.Services.AddHttpContextAccessor();

// Register services
builder.Services.AddScoped<IConfigAplicacaoProvider, ConfigAplicacaoProvider>();
builder.Services.AddScoped<IApprovalHubService, ApprovalHubService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=ApprovalHub}/{action=Index}/{id?}");

app.Run();
