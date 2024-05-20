using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ScentsSymphonyWeb.Data;
using ScentsSymphonyWeb.Helpers;
using ScentsSymphonyWeb.Models;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ScentsSymphonyWeb.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly StripeSettings _stripeSettings;
        private readonly UserManager<IdentityUser> _userManager;

        public CartController(ApplicationDbContext db, IOptions<StripeSettings> stripeSettings, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _stripeSettings = stripeSettings.Value;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var cart = SessionHelper.GetObjectFromJson<List<CartItem>>(HttpContext.Session, "cart");
            if (cart == null)
            {
                cart = new List<CartItem>();
            }
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId)
        {
            var cart = SessionHelper.GetObjectFromJson<List<CartItem>>(HttpContext.Session, "cart");
            if (cart == null)
            {
                cart = new List<CartItem>();
            }

            var product = _db.Perfume.FirstOrDefault(p => p.ProductID == productId);
            if (product != null)
            {
                var cartItem = cart.FirstOrDefault(c => c.Product.ProductID == productId);
                if (cartItem == null)
                {
                    cart.Add(new CartItem { Product = product, Quantity = 1 });
                }
                else
                {
                    cartItem.Quantity++;
                }
            }

            SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            return RedirectToAction("Index");
        }

        public IActionResult RemoveFromCart(int productId)
        {
            var cart = SessionHelper.GetObjectFromJson<List<CartItem>>(HttpContext.Session, "cart");
            var cartItem = cart.FirstOrDefault(c => c.Product.ProductID == productId);

            if (cartItem != null)
            {
                cart.Remove(cartItem);
            }

            SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = SessionHelper.GetObjectFromJson<List<CartItem>>(HttpContext.Session, "cart");
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index");
            }

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmCheckout()
        {
            var service = new SessionService();

            
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Console.WriteLine("User not found");
                return RedirectToAction("Index");
            }

            var userId = user.Id;

            
            var cart = SessionHelper.GetObjectFromJson<List<CartItem>>(HttpContext.Session, "cart");

            if (cart == null || !cart.Any())
            {
                Console.WriteLine("Cart is empty or not found");
                return RedirectToAction("Index");
            }

            var order = new Order
            {
                UserID = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cart.Sum(item => item.Product.Price * item.Quantity),
                OrderItems = cart.Select(item => new OrderItem
                {
                    ProductID = item.Product.ProductID,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                }).ToList()
            };

            _db.Orders.Add(order);
            _db.SaveChanges();

            
            HttpContext.Session.Remove("cart");

            return RedirectToAction("OrderConfirmation", new { orderId = order.OrderID });
        }

        public IActionResult OrderConfirmation(int orderId)
        {
            var order = _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.OrderID == orderId);

            if (order == null)
            {
                return RedirectToAction("Index");
            }

            return View(order);
        }
    }
}
