namespace MiniAmazonClone.Repositories
{
    public interface IProductRepository
    {
        Task<Product> GetProductById(int productId);
    }
}
