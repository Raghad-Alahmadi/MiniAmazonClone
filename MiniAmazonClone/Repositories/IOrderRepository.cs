namespace MiniAmazonClone.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetCustomerOrders(int userId);
        
    }
}
