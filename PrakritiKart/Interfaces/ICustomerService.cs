using Microsoft.AspNetCore.Mvc;
using PrakritiKart.Models;
namespace PrakritiKart.Interfaces
{
    public interface ICustomerService
    {
        //###################  Basic CRUD ############
        Task<Customer> GetCustomerByIdAsync(int customerId);
        Task<Customer> GetCustomerByEmailAsync(string email);
        Task<int> RegisterCustomerAsync(Customer customer);
        Task<bool> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int customerId);


        //################   Address CRUD   ##################

        Task<IEnumerable<Address>> GetCustomerAddressesAsync(int customerId);
        Task<int> AddAddressAsync(int customerId, Address address);
        Task<Address?> UpdateAddressAsync(int customerId, int addressId, Address updatedAddress);
        Task<bool> DeleteAddressAsync(int customerId, int addressId);
        //Task GetUserByEmailAsync(object email);

        //################   Home page Products ####################

        Task<IEnumerable<Product>> GetHomeProductsAsync();

        //######################### Product Page With Id ###########################
        Task<GetProductById> GetProductByIdAsync(int productId);


        //###############################  Serch Page Product ######################
        Task<ProductSearchResult> GetProductsWithImagesAsync(string name, string? category, decimal? minPrice, decimal? maxPrice, int? rating, int page, int pageSize);

        //##################### Cart page and add to catd functionalitiy #############

        Task<int> AddToCartAsync(AddToCartRequest addToCartRequest, int customerId);
        Task<IEnumerable<CartItem>> GetAllCartItemAsync(int customerId);

        Task<IEnumerable<CartItemWithDetails>> GetAllCartItemsWithDetails(int customerId);
        Task<int> RemoveItemFromCart(int cartId, int customerId);

        //############## Payment Api ##################S
        Task<PaymentDetails> CreateRazorpayOrderAsync(int customerId, decimal amount);
        Task<bool> VerifyPaymentAsync(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature, List<ProductOrderDto> products, int customerIdFromClaims);
        Task<IEnumerable<OrderItemDetails>> GetOrderItemsByCustomerId(int customerId);
    }

}
