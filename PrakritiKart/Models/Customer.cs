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

}
