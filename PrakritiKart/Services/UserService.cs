using Dapper;
using MySql.Data.MySqlClient;
using PrakritiKart.Interfaces;
using PrakritiKart.Models;
using System.Data;


namespace PrakritiKart.Services
{

    public class UserService : IUserService
    {
        private readonly IDbConnection _db;

        public UserService(string connectionString)
        {
            _db = new MySqlConnection(connectionString);
        }

        public async Task<Customer> GetUserByIdAsync(int userId)
        {
            var sql = "SELECT * FROM user WHERE UserId = @UserId";
            return await _db.QueryFirstOrDefaultAsync<Customer>(sql, new { UserId = userId });
        }

        public async Task<Customer> GetUserByEmailAsync(string email)
        {
            var sql = "SELECT * FROM user WHERE Email = @Email";
            return await _db.QueryFirstOrDefaultAsync<Customer>(sql, new { Email = email });
        }

        public async Task<int> RegisterUserAsync(Customer user)
        {
            var sql = @"INSERT INTO User (FirstName, LastName, Email, PasswordHash,Salt, PhoneNumber, EmailVerified, UserType) 
                    VALUES (@FirstName, @LastName, @Email, @PasswordHash, @Salt, @PhoneNumber, @EmailVerified, @UserType);
                    SELECT LAST_INSERT_ID();";
            return await _db.ExecuteScalarAsync<int>(sql, user);
        }

        public async Task<bool> UpdateUserAsync(Customer user)
        {
            var sql = @"UPDATE User 
                    SET FirstName = @FirstName, LastName = @LastName, 
                        PhoneNumber = @PhoneNumber, EmailVerified = @EmailVerified, 
                        UpdatedAt = CURRENT_TIMESTAMP 
                    WHERE UserId = @UserId";
            var result = await _db.ExecuteAsync(sql, user);
            return result > 0; // return true if any row was affected
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var sql = "DELETE FROM User WHERE UserId = @UserId";
            var result = await _db.ExecuteAsync(sql, new { UserId = userId });
            return result > 0; // return true if any row was affected
        }
    }

}
