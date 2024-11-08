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
    }

}
