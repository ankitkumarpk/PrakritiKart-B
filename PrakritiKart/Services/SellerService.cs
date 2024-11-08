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

        // #################################### Add Products Services ###########################################

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

        public async Task<IEnumerable<Product>> GetAllProducts(int sellerId)
        {
            var sql = @"Select * from products WHERE SellerId = @SellerId";
            return await _db.QueryAsync<Product>(sql, new { SellerId = sellerId });
        }







    }
}
