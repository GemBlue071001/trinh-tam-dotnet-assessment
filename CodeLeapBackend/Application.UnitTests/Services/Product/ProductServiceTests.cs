using System.Linq.Expressions;
using Application.Models.Product;
using Application.Repositories;
using Application.Services.Product;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.UnitTests.ProductTests;

public sealed class ProductServiceTests
{
    [Fact]
    public async Task Create_TrimsValuesAndPersistsProduct()
    {
        var repository = new Mock<IProductRepository>();
        Domain.Entities.Product? addedProduct = null;
        repository.Setup(repo => repo.Add(It.IsAny<Domain.Entities.Product>()))
            .Callback<Domain.Entities.Product>(product => addedProduct = product)
            .Returns(Task.CompletedTask);
        repository.Setup(repo => repo.Save()).ReturnsAsync(1);
        var service = new ProductService(repository.Object);

        var result = await service.Create(new CreateProductRequest
        {
            Name = "  Keyboard  ",
            Description = "  Mechanical keyboard  ",
            Price = 99.99m,
            StockQuantity = 8,
            Category = "  Accessories  ",
            ImageUrl = "  https://example.com/keyboard.jpg  "
        });

        Assert.NotNull(addedProduct);
        Assert.Equal("Keyboard", addedProduct!.Name);
        Assert.Equal("Mechanical keyboard", addedProduct.Description);
        Assert.Equal("Accessories", addedProduct.Category);
        Assert.False(addedProduct.IsDeleted);
        Assert.Equal(addedProduct.Id, result.Id);
        repository.Verify(repo => repo.Save(), Times.Once);
    }

    [Fact]
    public async Task GetById_WhenProductExists_ReturnsMappedProduct()
    {
        var product = CreateProduct();
        var repository = new Mock<IProductRepository>();
        repository.Setup(repo => repo.GetBy(It.IsAny<Expression<Func<Domain.Entities.Product, bool>>>() ))
            .ReturnsAsync(product);
        var service = new ProductService(repository.Object);

        var result = await service.GetById(product.Id);

        Assert.NotNull(result);
        Assert.Equal(product.Id, result!.Id);
        Assert.Equal(product.Name, result.Name);
        Assert.Equal(product.Price, result.Price);
    }

    [Fact]
    public async Task GetById_WhenProductDoesNotExist_ReturnsNull()
    {
        var repository = new Mock<IProductRepository>();
        repository.Setup(repo => repo.GetBy(It.IsAny<Expression<Func<Domain.Entities.Product, bool>>>() ))
            .ReturnsAsync((Domain.Entities.Product?)null);
        var service = new ProductService(repository.Object);

        var result = await service.GetById(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task Search_ReturnsMappedPagedProducts()
    {
        var firstProduct = CreateProduct("Keyboard");
        var secondProduct = CreateProduct("Mouse");
        var request = new SearchProductsRequest
        {
            Keyword = "key",
            Category = "Accessories",
            PageNumber = 2,
            PageSize = 5
        };
        var repository = new Mock<IProductRepository>();
        repository.Setup(repo => repo.Search(request))
            .ReturnsAsync(([firstProduct, secondProduct], 12));
        var service = new ProductService(repository.Object);

        var result = await service.Search(request);

        Assert.Equal(2, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(12, result.TotalCount);
        Assert.Collection(result.Items,
            item => Assert.Equal(firstProduct.Id, item.Id),
            item => Assert.Equal(secondProduct.Id, item.Id));
    }

    [Fact]
    public async Task Update_WhenProductExists_UpdatesAndPersistsProduct()
    {
        var product = CreateProduct();
        var repository = new Mock<IProductRepository>();
        repository.Setup(repo => repo.GetBy(It.IsAny<Expression<Func<Domain.Entities.Product, bool>>>() ))
            .ReturnsAsync(product);
        repository.Setup(repo => repo.Update(product)).Returns(Task.CompletedTask);
        repository.Setup(repo => repo.Save()).ReturnsAsync(1);
        var service = new ProductService(repository.Object);

        var result = await service.Update(product.Id, new UpdateProductRequest
        {
            Name = "  Updated keyboard  ",
            Description = "  Updated description  ",
            Price = 129.99m,
            StockQuantity = 3,
            Category = "  Updated category  ",
            ImageUrl = "  https://example.com/updated.jpg  "
        });

        Assert.NotNull(result);
        Assert.Equal("Updated keyboard", product.Name);
        Assert.Equal("Updated description", product.Description);
        Assert.Equal(129.99m, product.Price);
        Assert.Equal(3, product.StockQuantity);
        Assert.Equal("Updated category", product.Category);
        Assert.Equal("https://example.com/updated.jpg", product.ImageUrl);
        repository.Verify(repo => repo.Update(product), Times.Once);
        repository.Verify(repo => repo.Save(), Times.Once);
    }

    [Fact]
    public async Task Update_WhenProductDoesNotExist_DoesNotPersist()
    {
        var repository = new Mock<IProductRepository>();
        repository.Setup(repo => repo.GetBy(It.IsAny<Expression<Func<Domain.Entities.Product, bool>>>() ))
            .ReturnsAsync((Domain.Entities.Product?)null);
        var service = new ProductService(repository.Object);

        var result = await service.Update(Guid.NewGuid(), CreateUpdateRequest());

        Assert.Null(result);
        repository.Verify(repo => repo.Update(It.IsAny<Domain.Entities.Product>()), Times.Never);
        repository.Verify(repo => repo.Save(), Times.Never);
    }

    [Fact]
    public async Task Delete_WhenProductExists_SoftDeletesAndPersistsProduct()
    {
        var product = CreateProduct();
        var repository = new Mock<IProductRepository>();
        repository.Setup(repo => repo.GetBy(It.IsAny<Expression<Func<Domain.Entities.Product, bool>>>() ))
            .ReturnsAsync(product);
        repository.Setup(repo => repo.Update(product)).Returns(Task.CompletedTask);
        repository.Setup(repo => repo.Save()).ReturnsAsync(1);
        var service = new ProductService(repository.Object);

        var deleted = await service.Delete(product.Id);

        Assert.True(deleted);
        Assert.True(product.IsDeleted);
        Assert.NotNull(product.DeletedAt);
        Assert.Equal(product.DeletedAt, product.UpdatedAt);
        repository.Verify(repo => repo.Save(), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenProductDoesNotExist_DoesNotPersist()
    {
        var repository = new Mock<IProductRepository>();
        repository.Setup(repo => repo.GetBy(It.IsAny<Expression<Func<Domain.Entities.Product, bool>>>() ))
            .ReturnsAsync((Domain.Entities.Product?)null);
        var service = new ProductService(repository.Object);

        var deleted = await service.Delete(Guid.NewGuid());

        Assert.False(deleted);
        repository.Verify(repo => repo.Update(It.IsAny<Domain.Entities.Product>()), Times.Never);
        repository.Verify(repo => repo.Save(), Times.Never);
    }

    private static Domain.Entities.Product CreateProduct(string name = "Keyboard")
    {
        var now = DateTimeOffset.UtcNow;
        return new Domain.Entities.Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "Description",
            Price = 99.99m,
            StockQuantity = 10,
            Category = "Accessories",
            ImageUrl = "https://example.com/product.jpg",
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static UpdateProductRequest CreateUpdateRequest()
    {
        return new UpdateProductRequest
        {
            Name = "Keyboard",
            Price = 99.99m,
            StockQuantity = 10,
            Category = "Accessories"
        };
    }
}
