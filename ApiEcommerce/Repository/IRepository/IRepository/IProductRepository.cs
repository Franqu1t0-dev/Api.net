/*
=============
🏆 Ejercicio 
=============
*/
// 1. Crear una interfaz llamada IProductRepository.
//
// 2. Incluir los siguientes métodos en la interfaz:
//
//    - GetProducts
//        → Devuelve todos los productos
//          en ICollection del tipo Product.
//
//    - GetProductsForCategory
//        → Recibe un categoryId y devuelve los productos
//          de esa categoría en ICollection del tipo Product.
//
//    - SearchProduct
//        → Recibe un nombre y devuelve los productos
//          que coincidan en ICollection del tipo Product.
//
//    - GetProduct
//        → Recibe un id y 
//          devuelve un solo objeto Product
//          o null si no se encuentra.
//
//    - BuyProduct
//        → Recibe el nombre del producto y una cantidad,
//          y devuelve un bool indicando si la compra fue exitosa.
//
//    - ProductExists (por id)
//        → Recibe un id y devuelve un bool
//          indicando si existe el producto.
//
//    - ProductExists (por nombre)
//        → Recibe un nombre y devuelve un bool
//          indicando si existe el producto.
//
//    - CreateProduct
//        → Recibe un objeto Product 
//          y devuelve un bool indicando si la creación fue exitosa.
//
//    - UpdateProduct
//        → Recibe un objeto Product
//          y devuelve un bool indicando si la actualización fue exitosa.
//
//    - DeleteProduct
//        → Recibe un objeto Product
//          y devuelve un bool indicando si la eliminación fue exitosa.
//
//    - Save
//        → Devuelve un bool indicando
//          si los cambios se guardaron correctamente.
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
    ICollection <Product> SearchProduct (string name);
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