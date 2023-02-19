using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSqlServer<ApplicationDBContext>(builder.Configuration["Database:SqlServer"]);
var app = builder.Build();
var config = app.Configuration;
ProductRepository.Init(config);


app.MapGet("/", () => {
    return "Hello World";
});

app.MapPost("/products", (ProductRequest req, ApplicationDBContext context) => {
    var category = context.Categories.Where(c => c.Id == req.CategoryId).First();
    var product = new Product {
        Code = req.Code,
        Name = req.Name,
        Description = req.Description,
        Category = category
    };

    if(req.Tags != null){
        product.Tags = new List<Tags>();
        foreach (var t in req.Tags){
            product.Tags.Add(new Tags{Name = t});
        }
    }

    context.Products.Add(product);
    context.SaveChanges();
    return Results.Created($"/products/{product.Id}", product.Id);
});

app.MapGet("/products/{id}", ([FromRoute] int id, ApplicationDBContext context) => {
    var product =  context.Products
        .Include(p => p.Category)
        .Include(p => p.Tags)
        .Where(p => p.Id == id).First();
    if(product == null){
        return Results.NotFound();
    }
    return Results.Ok(product);
});

app.MapPut("/products/{id}", ([FromRoute] int id, ProductRequest req, ApplicationDBContext context) => {
    var product =  context.Products
        .Include(p => p.Tags)
        .Where(p => p.Id == id).First();

    product.Code = req.Code;
    product.Name = req.Name;
    product.Description = req.Description;

    var category = context.Categories.Where(c => c.Id == req.CategoryId).First();
    product.Category = category;

    if(req.Tags != null){
        product.Tags = new List<Tags>();
        foreach (var t in req.Tags){
            product.Tags.Add(new Tags{Name = t});
        }
    }
    
    context.SaveChanges();
    return Results.Ok(product);
});

app.MapDelete("/products/{id}", ([FromRoute] int id, ApplicationDBContext context) => {
    var product =  context.Products.Where(p => p.Id == id).First();
    context.Products.Remove(product);
    context.SaveChanges();
    return Results.Ok();
});

app.MapGet("/configuration/database", (IConfiguration config) => {
    return Results.Ok(config["database:connection"]);
});

app.Run();
