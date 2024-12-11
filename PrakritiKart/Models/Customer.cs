namespace PrakritiKart.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }            // Unique ID for each user
        public string FirstName { get; set; }      // User's first name
        public string LastName { get; set; }       // User's last name
        public string Email { get; set; }           // User's email (must be unique)
        public string PasswordHash { get; set; }    // Hashed password
        public string Salt { get; set; }            // User's Salt
        public string PhoneNumber { get; set; }     // Optional phone number
        public bool EmailVerified { get; set; }     // Email verification status
        public string UserType { get; set; }
        public DateTime CreatedAt { get; set; }     // When the account was created
        public DateTime UpdatedAt { get; set; }     // Last updated timestamp
    }

    public class Address
    {
        public int AddressId { get; set; }
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }

    }
    public class GetProductById
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
        public string StoreName { get; set; }

        // Collection to store multiple images
        public List<ProductImage> Images { get; set; } = new List<ProductImage>();
    }

    public class SearchProduct
    {
        public int ProductId { get; set; }
        public int SellerId { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int Rating { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; } 
    }
    public class ProductSearchResult
    {
        public IEnumerable<SearchProduct> Products { get; set; } = Enumerable.Empty<SearchProduct>();
        public int TotalCount { get; set; }
    }



    //###############################  Cart System And cart page functinality ################################

    public class CartItem
    {
        public int CartId { get; set; }
        public int customerId { get; set; }
        public int productId { get; set; }
        public int sellerId { get; set; }
        public int quantity { get; set; }
    }
    public class AddToCartRequest
    {
        public int productId { get; set; }
        public int quantity { get; set; }
        public int sellerId { get; set; }
    }


    public class CartItemWithDetails
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public int Quantity { get; set; }
        public int SellerId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public double Rating { get; set; }
        public string ImageUrl { get; set; }
    }

    public class PaymentDetails
    {
        public int PaymentId { get; set; }
        public string RazorpayOrderId { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }

    public class CreateOrderRequest
    {
       
        public decimal Amount { get; set; }

    }

    public class VerifyPaymentRequest
    {
        public string RazorpayOrderId { get; set; }
        public string RazorpayPaymentId { get; set; }
        public string RazorpaySignature { get; set; }
        public List<ProductOrderDto> products { get; set; }
    }

    public class ProductOrderDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderItemDetails
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int SellerId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductId { get; set; }
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public string Status {get; set;}
        
    }



}
