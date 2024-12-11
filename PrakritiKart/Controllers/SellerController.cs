using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrakritiKart.Interfaces;
using PrakritiKart.Models;
using System.Web.Providers.Entities;



namespace PrakritiKart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SellerController : ControllerBase
    {

        private readonly ISellerService _sellerService;

        public SellerController(ISellerService sellerService)
        {
            _sellerService = sellerService;
            
        }
        //#############################  Basic Seller Crud Controller ###########################
        [HttpGet("info/{id}")]
        public async Task<IActionResult> GetSellerAsync(int id)
        {
            var seller = await _sellerService.GetSellerByIdAsync(id);

            if (seller == null)
            {
                return Ok("no seller found");
            }

            return Ok(seller);
        }

        [HttpPost("register")]
        public async Task<IActionResult> AddSellerAsync(Seller seller)
        {
            var result = _sellerService.RegisterSellerAsync(seller);
            if (result == null)
            {
                return Unauthorized("Seller not registred");
            }
            return Ok(new { meassage = "registration successfull", result });


        }

        //########################  Seller Info Controller ##################

        [HttpGet("requiredinfo")]
        public async Task<IActionResult> GetSellerRequiredInfo()
        {
            var sellerIdClaim = User.FindFirst("sellerid")?.Value;
            if(sellerIdClaim ==null)
            {
                return Unauthorized("Not able to Identify the seller");
            }
            var sellerId = Convert.ToInt32(sellerIdClaim);
            var result = await _sellerService.GetSellerRequiredInfo(sellerId);
            return Ok(result);

        }

        [HttpPost("add/sellerinfo")]
       public async Task<IActionResult> AddSellerInfo([FromBody] SellerInfo sellerinfo)
        {
            var sellerIdClaim = User.FindFirst("sellerid")?.Value;
            if (sellerIdClaim == null)
            {
                return Unauthorized("Not able to Identify the seller");
            }
            var sellerId = Convert.ToInt32(sellerIdClaim);
            var result = await _sellerService.AddSellerInfo(sellerId, sellerinfo);
            return Ok(result);

        }

        [HttpPatch("profile-image")]
        public async Task<IActionResult> UpdateProfileImage( IFormFile profileImg)
        {
            if (profileImg == null || profileImg.Length == 0)
            {
                return BadRequest(new { message = "Profile image is required." });
            }
            var sellerIdClaim = User.FindFirst("sellerid")?.Value;
            if (sellerIdClaim == null)
            {
                return Unauthorized("Not able to Identify the seller");
            }
            var sellerId = Convert.ToInt32(sellerIdClaim);

            var success = await _sellerService.UpdateProfileImageAsync(sellerId, profileImg);
            if(success>0)
            {
                return Ok("Profile Image saved Succesfully");
            }

            return NotFound("Failed! Profile Image saved failed");
            

            
        }

        [HttpPost("upload/sellerImg")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {

            var sellerIdClaim = User.FindFirst("sellerid")?.Value;
            if (sellerIdClaim == null)
            {
                return Unauthorized("Not able to Identify the seller");
            }
            var sellerId = Convert.ToInt32(sellerIdClaim);
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a valid image.");

            try
            {
                var isSaved = await _sellerService.UploadSellerImg(sellerId,file);
                if (isSaved>1)
                    return Ok(new { message = "Image uploaded successfully." });
                else
                    return StatusCode(500, "Image upload failed.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while uploading the image.");
            }
        }

        //############################ Product Controller ###################
        [HttpPost("product/add")]
        public async Task<IActionResult> AddProduct([FromForm] ProductDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid product data.");
            }

            var sellerIdClaim = User.FindFirst("sellerid")?.Value;
            var sellerId = Convert.ToInt32(sellerIdClaim);
            

            try
            {
                int productId = await _sellerService.AddProductAsync(productDto,sellerId);
         
                return Ok(new { ProductId = productId, Message = "Product added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("product/getall")]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            var sellerIdClaim = User.FindFirst("sellerid")?.Value;
            var sellerId = Convert.ToInt32(sellerIdClaim);
            var products = await _sellerService.GetAllProductsAsync(sellerId);
            return Ok(products);
        }

        [HttpDelete("product/delete/{productId}")]
        public async Task<IActionResult>DeleteProduct(int productId)
        {
            //var sellerIdClaim = User.FindFirst("sellerid")?.Value;
            var sellerId = 4;
            var success = _sellerService.DeleteProductAsync(productId,sellerId);
            return Ok(new {Message = "Product deleted Succesfully"});
        }

        [HttpPut("product/edit/{productId}")]
        public async Task<IActionResult> EditProduct(int productId, [FromForm] ProductDto productDto)
        {
            var sellerIdClaim = User.FindFirst("sellerid")?.Value;
            var sellerId = Convert.ToInt32(sellerIdClaim);

            if (productDto == null || productId <= 0)
            {
                return BadRequest("Invalid product data.");
            }

            try
            {
                bool isUpdated = await _sellerService.EditProductAsync(productId, productDto, sellerId);

                if (isUpdated)
                {
                    return Ok(new { Message = "Product updated successfully." });
                }
                else
                {
                    return NotFound(new { Message = "Product not found or you are not authorized to edit this product." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception here if necessary
                return StatusCode(500, new { Message = "An error occurred while updating the product.", Error = ex.Message });
            }
        }
    }
}


    


