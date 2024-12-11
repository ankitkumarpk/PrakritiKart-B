using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using MySql.Data.MySqlClient;
using PrakritiKart.Interfaces;
using PrakritiKart.Models;
using System.Collections;
using System.Data;

using static System.Net.Mime.MediaTypeNames;

namespace PrakritiKart.Services
{
    public class SellerService:ISellerService
    {
        private readonly IDbConnection _db;
        private readonly IWebHostEnvironment _environment;

        public SellerService(string connectionString, IWebHostEnvironment environment)
        {
            _db=new MySqlConnection(connectionString);
            _environment = environment;
        }
//############################  Basic Seller CRUD ###################################################
        public async Task<Seller> GetSellerByIdAsync(int sellerId)
        {
            var query = @"SELECT * FROM seller WHERE SellerId = @SellerId";
            return await _db.QueryFirstOrDefaultAsync<Seller>(query, new { SellerId = sellerId });
        }

        public async Task<Seller> GetSellerByEmailAsync(string email)
        {
            var query = @"SELECT * FROM seller WHERE Email = @Email";
            return await _db.QueryFirstOrDefaultAsync<Seller>(query, new { Email = email });
        }

        public async Task<int> RegisterSellerAsync(Seller seller)
        {
            var query = @"INSERT INTO  seller (FirstName, LastName, Email, PasswordHash,Salt, PhoneNumber, EmailVerified,  UserType) 
                    VALUES (@FirstName, @LastName, @Email, @PasswordHash, @Salt, @PhoneNumber, @EmailVerified, @UserType);
                    SELECT LAST_INSERT_ID();";
            return await _db.ExecuteScalarAsync<int>(query,seller);
        }

        //####################### Seller Info Services ###############################################

        public async Task<SellerInfo> GetSellerRequiredInfo(int sellerId)
        {
            var query = @"SELECT * FROM sellerinfo WHERE SellerId = @SellerId";
            return await _db.QuerySingleOrDefaultAsync<SellerInfo>(query, new { SellerId = sellerId });
        }

        public async Task<int> AddSellerInfo(int sellerId, SellerInfo sellerinfo)
        {
            // Assign the seller ID
            sellerinfo.SellerId = sellerId;

            // Convert IFormFile to Base64
           

            // Database query
            var query = @"INSERT INTO sellerinfo (SellerId, StoreName, Description, ContactEmail, ContactPhone, BusinessRegistrationNumber, AyushLicense, GstNumber, ProfileImg, ProfileImgType)
                VALUES (@SellerId, @StoreName, @Description, @ContactEmail, @ContactPhone, @BusinessRegistrationNumber, @AyushLicense, @GstNumber, @ProfileImg, @ProfileImgType);
                SELECT LAST_INSERT_ID();";

            return await _db.ExecuteScalarAsync<int>(query, sellerinfo);
        }


        public async Task<int> UpdateProfileImageAsync(int sellerId, IFormFile profileImg)
        {
            // Convert IFormFile to Base64 string
            string base64Image;
            using (var memoryStream = new MemoryStream())
            {
                await profileImg.CopyToAsync(memoryStream);
                base64Image = Convert.ToBase64String(memoryStream.ToArray());
            }

            var query = "UPDATE sellerinfo SET ProfileImG = @ProfileImg WHERE SellerId = @SellerId";
            var parameters = new { SellerId = sellerId, ProfileImg = base64Image };
            var result = await _db.ExecuteAsync(query, parameters);
            return result;

            
        }

        public async Task<int> UploadSellerImg(int sellerID, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid image file.");

            // Convert image to Base64 string
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                var imageBytes = ms.ToArray();
                var base64String = Convert.ToBase64String(imageBytes);

                var image = new sellerImg
                {
                    SellerId = sellerID,
                    ImageData = base64String,
                    ContentType = file.ContentType,
                    UploadDate = DateTime.UtcNow
                };

                // Save image to the database
                
                    var query = "INSERT INTO sellerprofileimg (SellerId, ImageData, ContentType, UploadDate) VALUES (@SellerId, @ImageData, @ContentType, @UploadDate)";
                    var rowsAffected = await _db.ExecuteAsync(query, image);
                    return rowsAffected;
                
            }
        }

        // #################################### CRUD Products Services ###########################################

        public async Task<int> AddProductAsync(ProductDto productDto, int sellerId)
        {
            // SQL query to insert the product data
            var sqlProduct = @"INSERT INTO Products (SellerId, ProductName, Category, Price, Quantity, Description, Ingredients, DosageInstructions, IsActive, CreatedAt, UpdatedAt) 
                           VALUES (@SellerId, @ProductName, @Category, @Price, @Quantity, @Description, @Ingredients, @DosageInstructions, 1, NOW(), NOW());
                           SELECT LAST_INSERT_ID();";

            // Insert product and get ProductId
            int productId = await _db.ExecuteScalarAsync<int>(sqlProduct, new
            {
                SellerId=sellerId,
                productDto.ProductName,
                productDto.Category,
                productDto.Price,
                productDto.Quantity,
                productDto.Description,
                productDto.Ingredients,
                productDto.DosageInstructions
            });

            // SQL query for inserting images
            var sqlImage = "INSERT INTO ProductImages (ProductId, ImageUrl, CreatedAt) VALUES (@ProductId, @ImageUrl, NOW());";

            foreach (var image in productDto.ImageUrl)
            {
                await _db.ExecuteAsync(sqlImage, new
                {
                    ProductId = productId,
                    ImageUrl = image, // Assuming Base64String contains the image data in base64 format
                      // Image MIME type, e.g., "image/png"
                });
            }

            return productId;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync(int sellerId)
        {
            var query = @"
            SELECT 
                p.ProductId, p.ProductName, p.Category, p.Price, p.Quantity, p.Description, p.Ingredients, p.DosageInstructions, p.IsActive, p.CreatedAt, p.UpdatedAt, 
                pi.ImageId, pi.ImageUrl, pi.ImageType, pi.ProductId
            FROM Products p
            LEFT JOIN ProductImages pi ON p.ProductId = pi.ProductId    
            WHERE p.SellerId = @SellerId";

            var productDictionary = new Dictionary<int, Product>();
            var products = await _db.QueryAsync<Product, ProductImage, Product>(
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
                new { SellerId = sellerId },  // Pass the sellerId to the query
                splitOn: "ImageId"
            );

            return products.Distinct().ToList();
        }

        public async Task<bool> DeleteProductAsync(int productId, int sellerId)
        {
            // SQL query to delete associated product images
            var sqlDeleteImages = "DELETE FROM ProductImages WHERE ProductId = @ProductId;";

            // SQL query to delete the product
            var sqlDeleteProduct = @"DELETE FROM Products 
                             WHERE ProductId = @ProductId AND SellerId = @SellerId;";

            // Ensure connection is open
            if (_db.State == ConnectionState.Closed)
            {
                _db.Open();
            }

            using (var transaction = _db.BeginTransaction())
            {
                try
                {
                    // Delete product images
                    await _db.ExecuteAsync(sqlDeleteImages, new { ProductId = productId }, transaction);

                    // Delete the product (only if it belongs to the given seller)
                    var rowsAffected = await _db.ExecuteAsync(sqlDeleteProduct, new { ProductId = productId, SellerId = sellerId }, transaction);

                    // Commit transaction if deletion was successful
                    if (rowsAffected > 0)
                    {
                        transaction.Commit();
                        return true;
                    }
                    else
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
                catch (Exception)
                {
                    // Rollback the transaction in case of an error
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    // Close the database connection
                    _db.Close();
                }
            }
        }

        public async Task<bool> EditProductAsync(int productId, ProductDto productDto, int sellerId)
        {
            var sqlUpdateProduct = @"UPDATE Products SET ProductName=@ProductName, Category=@Category, Price=@Price, Quantity = @Quantity, Description = @Description, Ingredients = @Ingredients, DosageInstructions = @DosageInstructions, UpdatedAt = NOW() WHERE ProductId = @ProductId AND SellerID = @SellerId;";
            var sqlDeleteImages = "DELETE FROM ProductImages WHERE ProductId = @ProductId";
            var sqlInsertImages = "INSERT INTO ProductImages (ProductId, ImageUrl, CreatedAt) VALUES(@ProductId, @ImageUrl, NOW()); ";

            //now open database connection if it's open
            if(_db.State == ConnectionState.Closed)
            {
                _db.Open();
            }

            using (var transaction = _db.BeginTransaction())
            {
                try
                {
                    //Update product
                    var rowsAffected = await _db.ExecuteAsync(sqlUpdateProduct, new
                    {
                        ProductId = productId,
                        SellerId = sellerId,
                        productDto.ProductName,
                        productDto.Category,
                        productDto.Price,
                        productDto.Quantity,
                        productDto.Description,
                        productDto.Ingredients,
                        productDto.DosageInstructions,


                    }, transaction);

                    if(rowsAffected == 0)
                    {
                        transaction.Rollback();
                        return false;
                    }

                    //Delete images of product

                    await _db.ExecuteAsync(sqlDeleteImages, new { ProductId = productId }, transaction);

                    //Now insert images in the database using the reference of productId
                    foreach(var imageUrl in productDto.ImageUrl)
                    {
                        await _db.ExecuteAsync(sqlInsertImages,
                        new
                        {
                            ProductId = productId,
                            ImageUrl = imageUrl,
                        },transaction);

                    }

                    transaction.Commit();
                    return true;

                }catch(Exception)
                {
                    transaction.Rollback();
                    return false;

                }
                finally
                {
                    _db.Close();

                }
            }

        }


    }
}
