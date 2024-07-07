using HPlusSport.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
// Código abaixo ativa a validação da ModelState (Módulo 3 aula 4)
// .ConfigureApiBehaviorOptions(options => {
//     options.SuppressModelStateInvalidFilter = true;
// })
;


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ShopContext>(options =>
{
    options.UseInMemoryDatabase("Shop"); // Nome arbitrário para o banco de dados, vai chamar de Shop por enquanto.
});

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

// Chamando o EnsureCreate (A Seed) para criar o banco de dados
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopContext>();
    await db.Database.EnsureCreatedAsync();
}

// Iniciando Minimal API (Uso do MapGet)
app.MapGet("/products", async (ShopContext _context) =>
{
    return Results.Ok(await _context.Products.ToListAsync());
});

app.MapGet("/products/{id}", async (int id, ShopContext _context) =>
{
    var product = await _context.Products.FindAsync(id);
    if (product == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(await _context.Products.FindAsync(id));
}).WithName("GetProduct");

// Retorna com condição, onde retorna apenas os "Disponíveis"
app.MapGet("products/available", async (ShopContext _context) =>
{
    return Results.Ok(await _context.Products.Where(p => p.IsAvailable).ToArrayAsync());
}
);

app.MapPost("products", async (ShopContext _context, Product product) =>
{
    _context.Products.Add(product);
    await _context.SaveChangesAsync();

    return Results.CreatedAtRoute(
        "GetProduct",
        new { id = product.Id },
        product);
});

app.MapPut("products", async (ShopContext _context, int id, Product product) =>
{
    // Verificar o item
    if (id != product.Id)
    {
        return Results.BadRequest();
    }

    // Modifica o Item
    _context.Entry(product).State = EntityState.Modified;

    try
    {
        // Salva o Item
        await _context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        if (!_context.Products.Any(p => p.Id == id))
        {
            return Results.NotFound();
        }
    }
    return Results.NoContent();
});

app.MapDelete("products/{id}", async (ShopContext _context, int id) =>
{
    var product = await _context.Products.FindAsync(id);
    if (product == null)
    {
        return Results.NotFound();
    }

    _context.Products.Remove(product);
    await _context.SaveChangesAsync();

    return Results.Ok(product);
});

app.MapDelete("products/DeleteMany", async (ShopContext _context, [FromQuery] int[] ids) =>
{
    var productsList = new List<Product>();

    foreach (var id in ids)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return Results.NotFound();
        }
        productsList.Add(product);
    }

    _context.RemoveRange(productsList);
    await _context.SaveChangesAsync();

    return Results.Ok(productsList);

});



app.Run();
