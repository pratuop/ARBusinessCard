using ARBusinessCard.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".ARCard.Session";
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<DatabaseHelper>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// ✅ Default MVC route PEHLE — Card/Create, Card/Edit sab theek kaam karega
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ✅ QR Public card route BAAD MEIN — /c/{slug} use karo, /card/ se conflict nahi
app.MapControllerRoute(
    name: "card-view",
    pattern: "c/{slug}",
    defaults: new { controller = "Card", action = "ARView" });

app.Run();