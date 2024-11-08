using Dapper;
using MySql.Data.MySqlClient;
using PrakritiKart.Interfaces;
using PrakritiKart.Models;
using System.Data;


namespace PrakritiKart.Services
{

    public class CustomerService : ICustomerService
    {
        private readonly IDbConnection _db;

        public CustomerService(string connectionString)
        {
            _db = new MySqlConnection(connectionString);
        }

        // ###################### Customer Basic CRUD Service #########################
        public async Task<Customer> GetCustomerByIdAsync(int customerId)
        {
            var sql = "SELECT * FROM Customer WHERE  CustomerId = @CustomerId";
            return await _db.QueryFirstOrDefaultAsync<Customer>(sql, new { CustomerId = customerId });
        }

        public async Task<Customer> GetCustomerByEmailAsync(string email)
        {
            var sql = "SELECT * FROM Customer WHERE Email = @Email";
            return await _db.QueryFirstOrDefaultAsync<Customer>(sql, new { Email = email });
        }

        public async Task<int> RegisterCustomerAsync(Customer customer)
        {
            var sql = @"INSERT INTO  Customer (FirstName, LastName, Email, PasswordHash,Salt, PhoneNumber, EmailVerified,  UserType) 
                    VALUES (@FirstName, @LastName, @Email, @PasswordHash, @Salt, @PhoneNumber, @EmailVerified, @UserType);
                    SELECT LAST_INSERT_ID();";
            return await _db.ExecuteScalarAsync<int>(sql, customer);
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            var sql = @"UPDATE  Customer 
                    SET FirstName = @FirstName, LastName = @LastName, 
                        PhoneNumber = @PhoneNumber, EmailVerified = @EmailVerified, 
                        UpdatedAt = CURRENT_TIMESTAMP 
                    WHERE  CustomerId = @CustomerId";
            var result = await _db.ExecuteAsync(sql, customer);
            return result > 0; // return true if any row was affected
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            var sql = "DELETE FROM  Customer WHERE  CustomerId = @CustomerId";
            var result = await _db.ExecuteAsync(sql, new { CustomerId = customerId });
            return result > 0; // return true if any row was affected
        }






        //##########################  Customer Address CRUD ########################

        /// <summary>
        /// Retrieves all addresses for a specific customer based on their customer ID.
        /// </summary>
        public async Task<IEnumerable<Address>> GetCustomerAddressesAsync(int customerId)
        {
            var sql = "SELECT * FROM customeraddress WHERE CustomerId = @CustomerId";
            return await _db.QueryAsync<Address>(sql, new { CustomerId = customerId });
        }

        /// <summary>
        /// Adds a new address for the specified customer.
        /// </summary>
        public async Task<int> AddAddressAsync(int customerId, Address address)
        {
            address.CustomerId = customerId;
            var sql = @"INSERT INTO customeraddress (CustomerId, FullName, PhoneNumber, AddressLine1, AddressLine2, City, State, ZipCode, Country) 
                        VALUES (@CustomerId, @FullName, @PhoneNumber, @AddressLine1, @AddressLine2, @City, @State, @ZipCode, @Country);
                        SELECT LAST_INSERT_ID();";
            return await _db.ExecuteScalarAsync<int>(sql, address);
        }

        /// <summary>
        /// Updates an existing address for the specified customer and address ID.
        /// </summary>
        public async Task<Address?> UpdateAddressAsync(int customerId, int addressId, Address updatedAddress)
        {
            var sql = @"UPDATE customeraddress 
                        SET FullName=@FullName, PhoneNumber= @PhoneNumber, AddressLine1 = @AddressLine1, AddressLine2 = @AddressLine2, 
                            City = @City, State = @State, ZipCode = @ZipCode, Country = @Country 
                        WHERE AddressId = @AddressId AND CustomerId = @CustomerId";

            var affectedRows = await _db.ExecuteAsync(sql, new
            {
                AddressId = addressId,
                CustomerId = customerId,
                updatedAddress.FullName,
                updatedAddress.PhoneNumber,
                updatedAddress.AddressLine1,
                updatedAddress.AddressLine2,
                updatedAddress.City,
                updatedAddress.State,
                updatedAddress.ZipCode,
                updatedAddress.Country
            });

            return affectedRows > 0 ? updatedAddress : null;
        }

        /// <summary>
        /// Deletes an address for the specified customer and address ID.
        /// </summary>
        public async Task<bool> DeleteAddressAsync(int customerId, int addressId)
        {
            var sql = "DELETE FROM customeraddress WHERE AddressId = @AddressId AND CustomerId = @CustomerId";
            var affectedRows = await _db.ExecuteAsync(sql, new { CustomerId = customerId, AddressId = addressId,  });
            return affectedRows > 0;
        }
    }

}
