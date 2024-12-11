using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PrakritiKart.Interfaces;
using PrakritiKart.Models;
using Razorpay.Api;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using System.Web.Providers.Entities;
using Customer = PrakritiKart.Models.Customer;
using Product = PrakritiKart.Models.Product;


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
            var affectedRows = await _db.ExecuteAsync(sql, new { CustomerId = customerId, AddressId = addressId, });
            return affectedRows > 0;
        }


        //######################### Homepage Product access ##############################
        public async Task<IEnumerable<Product>> GetHomeProductsAsync()
        {
            var categories = new List<string> { "Arishtam", "Bhasmam", "Churnam", "Ghritam", "Granules", "Tailam" };
            var products = new List<Product>();

            foreach (var category in categories)
            {
                var query = @"
        SELECT 
            p.ProductId, p.SellerId, p.ProductName, p.Category, p.Price, p.Quantity, p.Description, 
            p.Ingredients, p.DosageInstructions, p.IsActive, p.CreatedAt, p.UpdatedAt, 
            pi.ImageId, pi.ImageUrl, pi.ImageType, pi.ProductId
        FROM Products p
        LEFT JOIN ProductImages pi ON p.ProductId = pi.ProductId    
        WHERE p.Category = @Category
        ORDER BY p.CreatedAt DESC 
        LIMIT 8";

                var productDictionary = new Dictionary<int, Product>();

                var categoryProducts = await _db.QueryAsync<Product, ProductImage, Product>(
                    query,
                    (product, productImage) =>
                    {
                        if (!productDictionary.TryGetValue(product.ProductId, out var currentProduct))
                        {
                            currentProduct = product;
                            productDictionary.Add(currentProduct.ProductId, currentProduct);
                        }

                        if (productImage != null)
                        {
                            currentProduct.Images.Add(productImage);
                        }

                        return currentProduct;
                    },
                    new { Category = category },  // Pass the category to filter results
                    splitOn: "ImageId"
                );

                products.AddRange(productDictionary.Values);
            }

            return products;
        }

        public async Task<GetProductById> GetProductByIdAsync(int productId)
        {
            var query = @"SELECT 
        p.ProductId, p.SellerId, p.ProductName, p.Category, p.Price, p.Quantity, 
        p.Description, p.Ingredients, p.DosageInstructions, p.IsActive, p.CreatedAt, 
        p.UpdatedAt, si.StoreName, pi.ImageId, pi.ImageUrl, pi.ImageType
    FROM Products p
    LEFT JOIN ProductImages pi ON p.ProductId = pi.ProductId
    LEFT JOIN SellerInfo si ON p.SellerId = si.SellerId
    WHERE p.ProductId = @ProductId";
            var productDictionary = new Dictionary<int, GetProductById>();
            var result = await _db.QueryAsync<GetProductById, ProductImage, GetProductById>(query,
                (product, productImage) =>
            {
                if (!productDictionary.TryGetValue(product.ProductId, out var currentProduct))
                {
                    currentProduct = product;
                    productDictionary.Add(currentProduct.ProductId, currentProduct);
                }
                if (productImage != null)
                {
                    currentProduct.Images.Add(productImage);
                }
                return currentProduct;

            }, new { ProductId = productId }, splitOn: "ImageId");

            return productDictionary.Values.FirstOrDefault();

        }

        public async Task<ProductSearchResult> GetProductsWithImagesAsync(
    string name, string? category, decimal? minPrice, decimal? maxPrice, int? rating, int page, int pageSize)
        {
            var parameters = new DynamicParameters();
            parameters.Add("Name", name);
            parameters.Add("Category", category);
            parameters.Add("MinPrice", minPrice);
            parameters.Add("MaxPrice", maxPrice);
            parameters.Add("Rating", rating);
            parameters.Add("Page", page);
            parameters.Add("PageSize", pageSize);

            using var multi = await _db.QueryMultipleAsync("GetProductsWithImages", parameters, commandType: CommandType.StoredProcedure);

            // First result set: Products with images
            var products = await multi.ReadAsync<SearchProduct>();

            // Second result set: Total product count
            var totalCount = await multi.ReadFirstOrDefaultAsync<int>();

            return new ProductSearchResult
            {
                Products = products,
                TotalCount = totalCount
            };
        }

        //#######################  Page Cart Functionality ######################

        public async Task<int> AddToCartAsync(AddToCartRequest request, int customerId)
        {
            // Check if the item already exists in the cart

            var existingCartItem = await _db.QueryFirstOrDefaultAsync<CartItem>(
    @"SELECT * FROM Cart WHERE CustomerId = @CustomerId AND ProductId = @ProductId",
    new { ProductId = request.productId, CustomerId = customerId });


            if (existingCartItem != null)
            {
                // Update the quantity of the existing item
                var updateQuantityOfProduct = await _db.ExecuteAsync(
                    @"UPDATE Cart SET Quantity = Quantity + @Quantity 
              WHERE CustomerId = @CustomerId AND productId = @productId",
                    new { Quantity = request.quantity, request.productId, CustomerId = customerId });

                return updateQuantityOfProduct;
            }
            else
            {
                // Add the new item to the cart
                var addedItemInCart = await _db.ExecuteAsync(
                    @"INSERT INTO Cart (CustomerId, productId, SellerId, Quantity) 
              VALUES (@CustomerId, @productId, @SellerId, @Quantity)",
                    new
                    {
                        Quantity = request.quantity,
                        CustomerId = customerId,
                        ProductId = request.productId,
                        SellerId = request.sellerId
                    });

                return addedItemInCart;
            }
        }

        public async Task<IEnumerable<CartItem>> GetAllCartItemAsync(int customerId)
        {
            var sql = @"SELECT * FROM Cart WHERE CustomerId = @CustomerId";
            var result = await _db.QueryAsync<CartItem>(sql, new { CustomerId = customerId });
            return result;
        }

        public async Task<IEnumerable<CartItemWithDetails>> GetAllCartItemsWithDetails(int customerId)
        {
            var sql = @"
                        SELECT 
                                c.CartId,
                                c.ProductId,
                                c.CustomerId,
                                c.Quantity,
                                c.SellerId,
                                p.ProductName,
                                p.Price,
                                p.Rating,
                                pi.ImageUrl
                            FROM 
                                Cart c
                            JOIN 
                                Products p ON c.ProductId = p.ProductId
                            LEFT JOIN 
                                (
                                    SELECT 
                                        ProductId, MIN(ImageUrl) AS ImageUrl
                                    FROM 
                                        ProductImages
                                    GROUP BY 
                                        ProductId
                                ) pi ON p.ProductId = pi.ProductId
                            WHERE 
                                c.CustomerId = @CustomerId;
                            ";

            var sqlDelete = @"
                                DELETE FROM Cart
                                WHERE Quantity = 0";
            await _db.ExecuteAsync(sqlDelete);

            var result = await _db.QueryAsync<CartItemWithDetails>(sql, new { CustomerId = customerId });

            return result;


        }

        //##########################  Delete Cart Item #################################

        public async Task<int> RemoveItemFromCart(int customerId, int cartId)
        {
            var sql = @"Delete from Cart WHERE CartId=@CartId AND CustomerId = @CustomerId;";
            var result = await _db.ExecuteAsync(sql, new { CartId = cartId, CustomerId = customerId });
            return result;
        }

        //########################## RazorPay Payment Api ########################################
        public async Task<PaymentDetails> CreateRazorpayOrderAsync(int customerId, decimal amount)
        {
            try
            {
                var keyId = "rzp_test_T484g6IpAEzDYC";
                var keySecret = "rBJPIJc6rjizNlE9QanMCJUu";

                // Initialize Razorpay client
                var client = new RazorpayClient(keyId, keySecret);

                // Create order options
                var orderOption = new Dictionary<string, object>
                    {
                        {"amount", amount * 100 },  // Amount in paisa
                        {"currency", "INR" },
                        {"receipt", Guid.NewGuid().ToString() },
                        {"payment_capture", 1 }
                    };

                // Create Razorpay order
                var order = client.Order.Create(orderOption);
                var razorpayOrderId = order["id"].ToString();

               

                // Prepare payment details object
                var paymentDetails = new PaymentDetails
                {
                    CustomerId = customerId,
                    Amount = amount,
                    PaymentStatus = "Pending",
                    PaymentMethod = "Razorpay",
                    TransactionId = razorpayOrderId,
                    RazorpayOrderId = razorpayOrderId,
                    CreatedAt = DateTime.Now,
                };

                // Save to database
                var sql = @"INSERT INTO Paymentdetails (CustomerId, Amount, PaymentStatus, PaymentMethod, TransactionId, CreatedAt) 
                    VALUES(@CustomerId, @Amount, @PaymentStatus, @PaymentMethod, @TransactionId, @CreatedAt);  
                    SELECT LAST_INSERT_ID();";

                var paymentId = await _db.ExecuteScalarAsync<int>(sql, paymentDetails);
                
                paymentDetails.PaymentId = paymentId;
                

                return paymentDetails;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating Razorpay order", ex);
            }
        }



        //public async Task<bool> VerifyPaymentAsync(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature)
        //{
        //    try
        //    {
        //        string keySecret = "rBJPIJc6rjizNlE9QanMCJUu"; // Razorpay key secret

        //        // Manual signature verification
        //        if (!IsSignatureValid(razorpayOrderId, razorpayPaymentId, razorpaySignature, keySecret))
        //        {
        //            // Mark payment as failed
        //            var failSql = @"UPDATE paymentdetails 
        //                    SET PaymentStatus = 'Failed', UpdatedAt = CURRENT_TIMESTAMP 
        //                    WHERE TransactionId = @TransactionId AND PaymentStatus != 'Complete'";

        //            await _db.ExecuteAsync(failSql, new { TransactionId = razorpayOrderId });
        //            return false;
        //        }

        //        // Fetch payment details from the database
        //        var paymentDetailsSql = @"SELECT PaymentId, CustomerId, Amount 
        //                          FROM paymentdetails 
        //                          WHERE TransactionId = @TransactionId AND PaymentStatus != 'Complete'";

        //        var paymentDetails = await _db.QueryFirstOrDefaultAsync<dynamic>(paymentDetailsSql, new { TransactionId = razorpayOrderId });

        //        if (paymentDetails == null) return false;

        //        // Begin database transaction
        //        using (var connection = _db) // Assuming _db is an instance of IDbConnection
        //        {
        //            if (connection.State != System.Data.ConnectionState.Open)
        //            {
        //                connection.Open();
        //            }
        //            using (var transaction = _db.BeginTransaction())
        //            {
        //                try
        //                {
        //                    // Update payment status to 'Complete'
        //                    var updatePaymentSql = @"UPDATE paymentdetails 
        //                                     SET PaymentStatus = 'Complete', UpdatedAt = CURRENT_TIMESTAMP 
        //                                     WHERE TransactionId = @TransactionId";

        //                    await _db.ExecuteAsync(updatePaymentSql, new { TransactionId = razorpayOrderId }, transaction);

        //                    // Insert order details
        //                    var insertOrderSql = @"INSERT INTO orders 
        //                                   (CustomerId, totalAmount, orderDate, status, PaymentId) 
        //                                   VALUES (@CustomerId, @TotalAmount, CURRENT_TIMESTAMP, 'Processing', @PaymentId); SELECT LAST_INSERT_ID();";

        //                    await _db.ExecuteAsync(insertOrderSql, new
        //                    {
        //                        CustomerId = paymentDetails.CustomerId,
        //                        TotalAmount = paymentDetails.Amount,
        //                        PaymentId = paymentDetails.PaymentId
        //                    }, transaction);

        //                    // Commit the transaction
        //                    transaction.Commit();
        //                    return true;
        //                }
        //                catch
        //                {
        //                    transaction.Rollback();
        //                    throw;  // Re-throw to trigger error handling
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error verifying Razorpay payment", ex);
        //    }
        //}


        public async Task<bool> VerifyPaymentAsync(string razorpayOrderId,string razorpayPaymentId,string razorpaySignature,List<ProductOrderDto> products,int customerIdFromClaims)
        {
            try
            {
                string keySecret = "rBJPIJc6rjizNlE9QanMCJUu"; // Razorpay key secret

                // Manual signature verification
                if (!IsSignatureValid(razorpayOrderId, razorpayPaymentId, razorpaySignature, keySecret))
                {
                    var failSql = @"UPDATE paymentdetails 
                            SET PaymentStatus = 'Failed', UpdatedAt = CURRENT_TIMESTAMP 
                            WHERE TransactionId = @TransactionId AND PaymentStatus != 'Complete'";

                    await _db.ExecuteAsync(failSql, new { TransactionId = razorpayOrderId });
                    return false;
                }

                using (var connection = _db)
                {
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    using (var transaction = _db.BeginTransaction())
                    {
                        try
                        {
                            // Update payment status
                            var updatePaymentSql = @"UPDATE paymentdetails 
                                             SET PaymentStatus = 'Complete', UpdatedAt = CURRENT_TIMESTAMP 
                                             WHERE TransactionId = @TransactionId";

                            await _db.ExecuteAsync(updatePaymentSql, new { TransactionId = razorpayOrderId }, transaction);

                            // Insert into orders
                            var insertOrderSql = @"INSERT INTO orders 
                                           (CustomerId, orderDate, status) 
                                           VALUES (@CustomerId, CURRENT_TIMESTAMP, 'Processing'); 
                                           SELECT LAST_INSERT_ID();";

                            int orderId = await _db.QuerySingleAsync<int>(insertOrderSql, new
                            {
                                CustomerId = customerIdFromClaims
                            }, transaction);

                            // Fetch product details
                            string productIds = string.Join(",", products.Select(p => p.ProductId));

                            var productDetailsSql = @$"
                        SELECT p.ProductId, p.SellerId, p.Price 
                        FROM products p 
                        WHERE p.ProductId IN ({productIds})";

                            var productDetails = await _db.QueryAsync<dynamic>(productDetailsSql, transaction);

                            if (!productDetails.Any()) throw new Exception("No matching products found");

                            // Insert into orderitems
                            var insertOrderItemsSql = @"
                        INSERT INTO orderitems 
                        (orderId, productId, sellerId, quantity, price, CustomerId,CreatedAt) 
                        VALUES (@OrderId, @ProductId, @SellerId, @Quantity, @Price, @CustomerId, CURRENT_TIMESTAMP)";

                            foreach (var product in products)
                            {
                                var details = productDetails.FirstOrDefault(d => d.ProductId == product.ProductId);
                                if (details == null) continue;

                                await _db.ExecuteAsync(insertOrderItemsSql, new
                                {
                                    OrderId = orderId,
                                    ProductId = product.ProductId,
                                    SellerId = details.SellerId,
                                    Quantity = product.Quantity,
                                    Price = details.Price*product.Quantity,
                                    CustomerId = customerIdFromClaims
                                }, transaction);
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;  // Re-throw to trigger error handling
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error verifying Razorpay payment", ex);
            }
        }


        private bool IsSignatureValid(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature, string keySecret)
        {
            try
            {
                // Prepare payload string for signature validation
                var payload = $"{razorpayOrderId}|{razorpayPaymentId}";

                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keySecret)))
                {
                    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                    var generatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();
                    return generatedSignature == razorpaySignature;
                }
            }
            catch
            {
                return false;
            }
        }


        //###########

        public async Task<IEnumerable<OrderItemDetails>> GetOrderItemsByCustomerId(int customerId)
        {
            var sql = @"SELECT 
                            oi.OrderItemId, 
                            oi.OrderId, 
                            oi.CustomerId, 
                            oi.SellerId, 
                            oi.Quantity, 
                            oi.Price, 
                            oi.CreatedAt, 
                            oi.ProductId, 
                            p.ProductName,
                            p.ProductId,
                            o.Status, 
                            COALESCE(
                                (SELECT ImageUrl 
                                 FROM ProductImages 
                                 WHERE ProductId = oi.ProductId 
                                 ORDER BY ImageId ASC 
                                 LIMIT 1), 
                                'default-image.jpg'
                            ) AS ProductImage
                        FROM OrderItems oi
                        JOIN Products p ON p.ProductId = oi.ProductId
                        JOIN Orders o ON o.OrderId = oi.OrderId
                        WHERE oi.CustomerId = @CustomerId
                        ORDER BY oi.CreatedAt DESC;";

            var result = await _db.QueryAsync<OrderItemDetails>(sql, new { CustomerId = customerId });
            return  result;
        }


    }

}
