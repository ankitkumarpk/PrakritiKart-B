using System.Security.Cryptography.X509Certificates;

namespace PrakritiKart.Models
{
    public class Seller
    {
        public int SellerId { get; set; }            // Unique ID for each user
        public string FirstName { get; set; }      // User's first name
        public string LastName { get; set; }       // User's last name
        public string Email { get; set; }           // User's email (must be unique)
        public string PasswordHash { get; set; }    // Hashed password
        public string Salt { get; set; }            // User's Salt
        public string PhoneNumber { get; set; }     // Optional phone number
        public bool EmailVerified { get; set; }     // Email verification status
        public string UserType { get; set; }
        public DateTime CreatedAt { get; set; }     // When the account was created
        public DateTime UpdatedAt { get; set; }
    }

    public class SellerInfo
    {
        public int SellerId { get; set; }
        public string StoreName { get; set; }
        public string Description { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string BusinessRegistrationNumber { get; set; }
        public string AyushLicense { get; set; }
        public string GstNumber { get; set; }
        public int Rating { get; set; }
        public int TotalSales { get; set; }
        public string ProfileImg { get; set; }
        public string ProfileImgType { get; set; }
    }

    public class sellerImg
    {
        public int ImageId { get; set; }

        public int SellerId { get; set; } // Foreign key to the Sellers table

        public string ImageData { get; set; } // Base64 image string

        public string ContentType { get; set; } // MIME type (e.g., "image/jpeg")

        public DateTime UploadDate { get; set; } // Date when the image was uploaded
    }


    public class Product
    {
        public int ProductId { get; set; }
        public int SellerId { get; set; }
        
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public string Ingredients { get; set; }
        public string DosageInstructions { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        

        // Collection to store multiple images
        public List<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
    
    public class ProductImage
    {
        public int ImageId { get; set; }
        public int ProductId { get; set; }   // Foreign key to Product
        public string ImageUrl { get; set; } // URL or path to the image
        public string ImageType { get; set; } // MIME type (e.g., "image/jpeg")
        public DateTime CreatedAt { get; set; }
    }

    public class ProductDto
    {
        
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public string Ingredients { get; set; }
        public string DosageInstructions { get; set; }
        public List<string> ImageUrl { get; set; }  // For multiple image upload
    }

}
