using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrakritiKart.Interfaces;
using PrakritiKart.Models;

namespace PrakritiKart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {

        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        //############################# Address Controller ############################################

       

        [Authorize]
        [HttpGet("address/all")]

        public async Task<IActionResult> GetUserAddresses()
        {
            // Retrieve the userId from the JWT claims

            var customerIdClaim = User.FindFirst("customerid")?.Value;

            if (customerIdClaim == null)
            {
                return Unauthorized("Customer ID is not available in the token.");
            }

            // Directly convert userIdClaim to int
            int customerId = Convert.ToInt32(customerIdClaim);


            // Pass the extracted userId to the service
            var addresses = await _customerService.GetCustomerAddressesAsync(customerId);

            // Return the addresses as a response
            return Ok(addresses);
        }


        [HttpPost("address/add")]

        public async Task<IActionResult> AddAddress([FromBody] Address address)
        {
            var customerIdClaim = User.FindFirst("customerid")?.Value;

            if (customerIdClaim == null)
            {
                return Unauthorized("Customer ID is not available in the token.");
            }
            int customerId = Convert.ToInt32(customerIdClaim);

            var result = await _customerService.AddAddressAsync(customerId, address);

            return Ok(result);
        }

        [Authorize]
        [HttpPut("address/edit/{addressId}")]
        public async Task<IActionResult> EditAddress(int addressId, [FromBody] Address address)
        {
            var customerIdClaim = User.FindFirst("customerid")?.Value;
            if (customerIdClaim == null)
            {
                return Unauthorized("Customer ID is not available in the token.");
            }
            int customerId = Convert.ToInt32(customerIdClaim);

            var result = await _customerService.UpdateAddressAsync(customerId, addressId, address);
            if (result == null)
            {
                return NotFound("Address not found or access is unauthorized.");
            }
            return Ok(result);
        }


        [HttpDelete("address/delete/{addressId}")]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            var customerIdClaim = User.FindFirst("customerid")?.Value;
            if (customerIdClaim == null)
            {
                return Unauthorized("Customer ID is not available in the token.");
            }

            if (!int.TryParse(customerIdClaim, out int customerId))
            {
                return BadRequest("Invalid Customer ID.");
            }

            var success = await _customerService.DeleteAddressAsync(customerId, addressId);
            if (!success)
            {
                return NotFound("Address not found or access is unauthorized.");
            }

            return NoContent(); // Consider returning NoContent for successful deletion
        }

        //######################### Home Page product ############################
        [HttpGet("get-home-product")]
        public async Task<IActionResult> GetHomeProduct()
        {
            var result = await _customerService.GetHomeProductsAsync();
            return Ok(result);
        }

        [HttpGet("get-product/{productId}")]
        public async Task<IActionResult> GetProductById(int productId)
        {
            try 
            {
                var product = await _customerService.GetProductByIdAsync(productId);
                if(product == null)
                {
                    return NotFound("Product not found");
                }
                return Ok(product);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }

        [HttpGet("search-products")]
        public async Task<IActionResult> SearchProducts(
            [FromQuery] string? name,
            [FromQuery] string? category,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int? rating,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            // Validate input if necessary
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Page and PageSize must be greater than 0.");
            }

            var result = await _customerService.GetProductsWithImagesAsync(name, category, minPrice, maxPrice, rating, page, pageSize);

            return Ok(result);
        }

        //################  Cart Handling Page #######################
        [Authorize]
        [HttpPost("cart/add-to-cart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                var customerIdClaim = User.FindFirst("customerid")?.Value;

                if (customerIdClaim == null)
                {
                    return Unauthorized("Customer ID is not available in the token.");
                }

                // Directly convert userIdClaim to int
                int customerId = Convert.ToInt32(customerIdClaim);
                var affectedRows = await _customerService.AddToCartAsync(request, customerId);

                // Return success response
                return Ok(new { Message = "Cart updated successfully", AffectedRows = affectedRows });
            }
            catch (Exception ex)
            {
                // Return error response
                return BadRequest(new { Message = "An error occurred while updating the cart", Error = ex.Message });
            }
        }
        [Authorize]
        [HttpGet("cart/get-all-cart-items")]
        public async Task<IActionResult> GetAllCartItems()
        {
            try
            {
                var customerIdClaim = User.FindFirst("customerid").Value;
                var customerId = Convert.ToInt32(customerIdClaim);

                var result = await _customerService.GetAllCartItemAsync(customerId);
                return Ok(result);  
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);

            }
            
        }

        [Authorize]
        [HttpGet("cart/all-cart-items-with-details")]
        public async Task<IActionResult> GetAllCartItemsWithDetails()
        {
            try
            {
                var customerIdClaim = User.FindFirst("customerid").Value;
                var customerId = Convert.ToInt32(customerIdClaim);
                if (customerIdClaim == null)
                {
                    return Unauthorized();
                }

                var result = await _customerService.GetAllCartItemsWithDetails(customerId);
                return Ok(result);

            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("cart/remove-item-from-cart/{cartId}")]
        public async Task<IActionResult> RemoveItemFromCart(int cartId)
        {
            try
            {
                var customerIdClaim = User.FindFirst("customerid").Value;
                var customerId = Convert.ToInt32(customerIdClaim);
                

                var result = await _customerService.RemoveItemFromCart(customerId, cartId);
                return Ok(result);
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }


        //##########################  Payment apis #################################
        [Authorize]
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var customerIdClaim = User.FindFirst("customerid").Value;
                var customerId = Convert.ToInt32(customerIdClaim);


                if (customerIdClaim == null)
                {
                    return Unauthorized();
                }


                var paymentDetails = await _customerService.CreateRazorpayOrderAsync(customerId, request.Amount);

                // Return Razorpay order ID and Payment ID to the frontend
                return Ok(new { razorpayOrderId = paymentDetails.TransactionId, paymentId = paymentDetails.PaymentId});
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error creating Razorpay order: " + ex.Message);
            }
        }
        [Authorize]
        [HttpPost("verify-payment")]
        public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentRequest request)
        {
            try
            {
                var customerIdClaim = User.FindFirst("customerid").Value;
                var customerId = Convert.ToInt32(customerIdClaim);
                if (customerIdClaim == null)
                {
                    return Unauthorized();
                }
                // Verify the payment and update the payment status
                var isVerified = await _customerService.VerifyPaymentAsync(request.RazorpayOrderId, request.RazorpayPaymentId, request.RazorpaySignature, request.products,customerId);

                if (isVerified)
                {
                    return Ok(new { success = true, message = "Payment verified and completed" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Payment verification failed" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error verifying payment: " + ex.Message);
            }
        }
        [Authorize]
        [HttpGet("order/get-order-items")]
        public async Task<IActionResult> GetOrderItemsDetailsAsync()
        {
            var customerClaimId = User.FindFirst("customerid").Value;
            var customerId = Convert.ToInt32(customerClaimId);
            var result = await _customerService.GetOrderItemsByCustomerId(customerId);
            return Ok(result);
        }


    }
}
