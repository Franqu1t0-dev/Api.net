using System; 
using System.Collections.Generic;
using ApiEcommerce.Models;

namespace ApiEcommerce.Repository.IRepository;
public interface IProductRepository
{
  // Tu código aquí
    ICollection<Product> GetProducts();
    
    ICollection<Product> GetProductsForCategory(int categoryId);
//        → Recibe un categoryId y devuelve los productos
//          de esa categoría en ICollection del tipo Product.
    ICollection <Product> SearchProducts (string searchTerm);
//        → Recibe un nombre y devuelve los productos
//          que coincidan en ICollection del tipo Product.

    Product? GetProduct(int ProductId);
    bool BuyProduct (string name, int quantity);
//        → Recibe el nombre del producto y una cantidad,
//          y devuelve un bool indicando si la compra fue exitosa.
    bool ProductExists(int id);
    bool ProductExists(string name);

    bool CreateProduct(Product product);

    bool UpdateProduct (Product product);
    bool DeleteProduct (Product product);
    
    bool Save();  
}