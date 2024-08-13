using HPlusSport.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using Microsoft.Extensions.Options;


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


// Versioning API
builder.Services.AddApiVersioning(options => {
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'vvv";
        options.SubstituteApiVersionInUrl = true;
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
app.MapGet("/products", async (ShopContext _context, [AsParameters] ProductQueryParameters queryParameters) =>
{
    IQueryable<Product> products = _context.Products;


            // Remover os que não se encaixam nos limites do Min e Max
            if (queryParameters.MinPrice != null)
            {
                products = products.Where(
                    p => p.Price >= queryParameters.MinPrice.Value
                );
            }

            if (queryParameters.MaxPrice != null)
            {
                products = products.Where(
                    p => p.Price <= queryParameters.MaxPrice.Value
                );
            }
            if (!string.IsNullOrEmpty(queryParameters.SKU))
            {
                products = products.Where(
                    p => p.Sku == queryParameters.SKU);
            }

            if (!string.IsNullOrEmpty(queryParameters.Name))
            {
                products = products.Where(
                    p => p.Name.ToLower().Contains(
                        queryParameters.Name.ToLower()));
            }

            // Iniciando o Sort
            // Verificando se tem algo para ordenarmos
            if (!string.IsNullOrEmpty(queryParameters.SortBy) && 
                typeof(Product).GetProperty(queryParameters.SortBy) != null)
            {
                products = queryParameters.SortOrder.ToLower() == "desc"
                ? products.OrderByDescending(p => p.GetType().GetProperty(queryParameters.SortBy)!.GetValue(p, null))
                : products.OrderBy(p => p.GetType().GetProperty(queryParameters.SortBy)!.GetValue(p, null));
            }

            products = products
            .Skip(queryParameters.Size * (queryParameters.Page - 1))
            .Take(queryParameters.Size);

    return Results.Ok(await products.ToArrayAsync());
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
