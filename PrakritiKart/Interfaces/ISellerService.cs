using Microsoft.AspNetCore.Mvc;
using PrakritiKart.Models;

namespace PrakritiKart.Interfaces
{
    public interface ISellerService
    {
        Task<Seller> GetSellerByIdAsync(int id);
        Task<Seller> GetSellerByEmailAsync(string email);
        Task<int> RegisterSellerAsync(Seller seller);
        //Task<int> UpdateSellerAsync(Seller seller,int id);
        //Task<int> DeleteSellerAsync(int id);

        //#################### Seller Extra Info Addition ##########################
        Task<SellerInfo> GetSellerRequiredInfo(int sellerId);
        Task<int> AddSellerInfo(int sellerId, SellerInfo sellerinfo);
       Task<int> UpdateProfileImageAsync(int sellerId, IFormFile profileImage);
        Task<int> UploadSellerImg(int sellerId, IFormFile file);


        //################## Seller Poduct CRUD ###################################
        
            Task<int> AddProductAsync(ProductDto productDto, int sellerId);
        

    }
}
