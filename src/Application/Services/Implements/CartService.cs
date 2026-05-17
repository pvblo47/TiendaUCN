using Mapster;
using Serilog;
using TiendaUCN.src.Application.DTOs.CartDTO;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        public CartService(ICartRepository cartRepository, IProductRepository productRepository, IUserRepository userRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
        }

        public async Task<CartDTO> CreateOrGetCartAsync(string buyerId, int? userId = null)
        {
            // Inicializar el carrito como null
            Cart? cart = null;

            // En caso de que el usuario este autenticado
            if (userId.HasValue)
            {
                // Buscar el carrito del usuario autenticado
                cart = await _cartRepository.GetByUserIdAsync(userId.Value);

                // Si no se encuentra el carrito
                if (cart == null)
                {
                    Log.Information("No se encontró un carrito para userId: {UserId}", userId.Value);

                    // Buscar un carrito asociado al buyerId
                    cart = await _cartRepository.GetByBuyerIdAsync(buyerId);

                    if (cart == null)
                    {
                        // Crear un nuevo carrito vacio asociado al userId
                        var cartId = await CreateEmptyCartAsync(buyerId, userId);
                    }
                    else
                    {
                        // Crear un nuevo carrito para el usuario en base al carrito encontrado por buyerId
                        var newCart = new Cart
                        {
                            BuyerId = cart.BuyerId,
                            UserId = userId,
                            TotalPrice = cart.TotalPrice,
                            CartItems = cart.CartItems.Select(ci => new CartItem
                            {
                                ProductId = ci.ProductId,
                                Quantity = ci.Quantity,
                            }).ToList()
                        };

                        var isCreated = await _cartRepository.CreateAsync(newCart);
                        if (!isCreated)
                        {
                            Log.Error("Error al crear un nuevo carrito para userId: {UserId} basado en el carrito encontrado por buyerId: {BuyerId}.", userId.Value, buyerId);
                            throw new Exception($"Error al crear un nuevo carrito para userId: {userId.Value} basado en el carrito encontrado por buyerId: {buyerId}.");
                        }
                    }

                    // Obtener el carrito recién creado
                    cart = await _cartRepository.GetByUserIdAsync(userId.Value);
                }

                // Mapear el carrito a CartDTO
                return cart!.Adapt<CartDTO>();
            }

            // En caso de que el usuario no este autenticado
            cart = await _cartRepository.GetByBuyerIdAsync(buyerId);

            // Si no existe, crear uno nuevo
            if (cart == null)
            {
                Log.Information("No se encontró un carrito para buyerId: {BuyerId} y userId: {UserId}.", buyerId, userId);
                var cartId = await CreateEmptyCartAsync(buyerId, userId);

                // Obtener el carrito recién creado
                cart = await _cartRepository.GetByBuyerIdAsync(buyerId);
            }

            // Mapear el carrito a CartDTO
            return cart!.Adapt<CartDTO>();
        }

        public async Task<CartDTO> AddCartItemAsync(string buyerId, AddChangeCartItemDTO addCartItemDTO, int? userId = null)
        {
            // Crear u obtener el carrito actual para permitir el primer agregado sin una llamada previa a GET /api/cart
            await CreateOrGetCartAsync(buyerId, userId);
            var cart = await GetCartAsync(buyerId, userId);

            // Obtener el producto
            var product = await _productRepository.GetProductByIdForCustomerAsync(addCartItemDTO.ProductId);

            // Validar que el producto exista
            if (product == null)
            {
                Log.Error("Producto no encontrado para ID: {ProductId}", addCartItemDTO.ProductId);
                throw new Exception($"Producto no encontrado para ID: {addCartItemDTO.ProductId}");
            }

            // Verificar si el producto ya está en el carrito
            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == addCartItemDTO.ProductId);

            // Calcular la cantidad total (cantidad actual en el carrito + cantidad a agregar)
            var totalRequestedQuantity = addCartItemDTO.Quantity + (existingCartItem?.Quantity ?? 0);

            // Validar que la cantidad no exceda el stock disponible
            if (product.Stock < totalRequestedQuantity)
            {
                Log.Error("Stock insuficiente para el producto ID: {ProductId}. Stock disponible: {Stock}, cantidad solicitada: {Quantity}", addCartItemDTO.ProductId, product.Stock, totalRequestedQuantity);
                throw new Exception($"Stock insuficiente para el producto ID: {addCartItemDTO.ProductId}. Stock disponible: {product.Stock}, cantidad solicitada: {totalRequestedQuantity}");
            }

            // Verificar si el producto ya está en el carrito
            if (existingCartItem != null)
            {
                // Actualizar la cantidad del item existente
                await _cartRepository.UpdateItemQuantityAsync(cart.Id, existingCartItem.Id, totalRequestedQuantity);
                Log.Information("Cantidad del producto ID: {ProductId} actualizada en el carrito. Nueva cantidad: {Quantity}", addCartItemDTO.ProductId, totalRequestedQuantity);
            }
            else
            {
                // Si el producto no está en el carrito, agregarlo como un nuevo item
                var newCartItem = new CartItem
                {
                    ProductId = addCartItemDTO.ProductId,
                    Quantity = addCartItemDTO.Quantity,
                    CartId = cart.Id
                };

                var isAdded = await _cartRepository.AddItemAsync(newCartItem);
                if (!isAdded)
                {
                    Log.Error("Error al agregar el producto ID: {ProductId} al carrito para buyerId: {BuyerId} y userId: {UserId}", addCartItemDTO.ProductId, buyerId, userId);
                    throw new Exception($"Error al agregar el producto ID: {addCartItemDTO.ProductId} al carrito para buyerId: {buyerId} y userId: {userId}");
                }
            }

            // Actualizar el precio total del carrito
            var totalPrice = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);

            var newTotalPrice = totalPrice + (product.Price * addCartItemDTO.Quantity);
            await _cartRepository.UpdateTotalPriceAsync(cart.Id, newTotalPrice);
            Log.Information("Precio total del carrito actualizado. CartId: {CartId}", cart.Id);

            // Obtener el carrito actualizado
            cart = await GetCartAsync(buyerId, userId);

            // Mapear el carrito a CartDTO
            return cart.Adapt<CartDTO>();
        }

        public async Task<CartDTO> UpdateCartItemQuantityAsync(string buyerId, AddChangeCartItemDTO changeCartItemDTO, int? userId = null)
        {
            // Obtener el carrito actual
            var cart = await GetCartAsync(buyerId, userId);

            // Obtener el producto
            var product = await _productRepository.GetProductByIdForCustomerAsync(changeCartItemDTO.ProductId);

            // Validar que el producto exista
            if (product == null)
            {
                Log.Error("Producto no encontrado para ID: {ProductId}", changeCartItemDTO.ProductId);
                throw new Exception($"Producto no encontrado para ID: {changeCartItemDTO.ProductId}");
            }

            // Verificar si el producto está en el carrito
            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == changeCartItemDTO.ProductId);
            if (existingCartItem == null)
            {
                Log.Error("El producto ID: {ProductId} no se encuentra en el carrito para buyerId: {BuyerId} y userId: {UserId}", changeCartItemDTO.ProductId, buyerId, userId);
                throw new Exception($"El producto ID: {changeCartItemDTO.ProductId} no se encuentra en el carrito para buyerId: {buyerId} y userId: {userId}");
            }

            // Validar que la cantidad no exceda el stock disponible
            if (product.Stock < changeCartItemDTO.Quantity)
            {
                Log.Error("Stock insuficiente para el producto ID: {ProductId}. Stock disponible: {Stock}, cantidad solicitada: {Quantity}", changeCartItemDTO.ProductId, product.Stock, changeCartItemDTO.Quantity);
                throw new Exception($"Stock insuficiente para el producto ID: {changeCartItemDTO.ProductId}. Stock disponible: {product.Stock}, cantidad solicitada: {changeCartItemDTO.Quantity}");
            }

            // Actualizar la cantidad del item en el carrito
            await _cartRepository.UpdateItemQuantityAsync(cart.Id, existingCartItem.Id, changeCartItemDTO.Quantity);
            Log.Information("Cantidad del producto ID: {ProductId} actualizada en el carrito. Nueva cantidad: {Quantity}", changeCartItemDTO.ProductId, changeCartItemDTO.Quantity);

            // Actualizar el precio total del carrito
            var totalPrice = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);
            var itemPriceOld = existingCartItem.Quantity * product.Price;
            var itemPriceNew = changeCartItemDTO.Quantity * product.Price;

            var newTotalPrice = totalPrice - itemPriceOld + itemPriceNew;
            await _cartRepository.UpdateTotalPriceAsync(cart.Id, newTotalPrice);
            Log.Information("Precio total del carrito actualizado. CartId: {CartId}", cart.Id);

            // Obtener el carrito actualizado
            cart = await GetCartAsync(buyerId, userId);

            // Mapear el carrito a CartDTO
            return cart.Adapt<CartDTO>();
        }

        public async Task<CartDTO> RemoveCartItemAsync(string buyerId, int productId, int? userId = null)
        {
            // Obtener el carrito actual
            var cart = await GetCartAsync(buyerId, userId);

            // Validar que el producto exista
            var existingProduct = await _productRepository.ExistsByIdCustomerAsync(productId);
            if (!existingProduct)
            {
                Log.Error("Producto no encontrado para ID: {ProductId}", productId);
                throw new Exception($"Producto no encontrado para ID: {productId}");
            }

            // Verificar si el producto está en el carrito
            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (existingCartItem == null)
            {
                Log.Error("El producto ID: {ProductId} no se encuentra en el carrito para buyerId: {BuyerId} y userId: {UserId}", productId, buyerId, userId);
                throw new Exception($"El producto ID: {productId} no se encuentra en el carrito para buyerId: {buyerId} y userId: {userId}");
            }

            // Eliminar el item del carrito
            var isRemoved = await _cartRepository.RemoveItemAsync(cart.Id, existingCartItem.Id);
            if (!isRemoved)
            {
                Log.Error("Error al remover el producto ID: {ProductId} del carrito para buyerId: {BuyerId} y userId: {UserId}", productId, buyerId, userId);
                throw new Exception($"Error al remover el producto ID: {productId} del carrito para buyerId: {buyerId} y userId: {userId}");
            }
            Log.Information("Producto ID: {ProductId} removido del carrito. CartId: {CartId}", productId, cart.Id);

            // Actualizar el precio total del carrito
            var totalPrice = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);
            var itemRemovedPrice = existingCartItem.Quantity * existingCartItem.Product.Price;

            var newTotalPrice = totalPrice - itemRemovedPrice;
            await _cartRepository.UpdateTotalPriceAsync(cart.Id, newTotalPrice);
            Log.Information("Precio total del carrito actualizado. CartId: {CartId}", cart.Id);

            // Obtener el carrito actualizado
            cart = await GetCartAsync(buyerId, userId);

            // Mapear el carrito a CartDTO
            return cart.Adapt<CartDTO>();
        }

        public async Task<CartDTO> ClearCartAsync(string buyerId, int? userId = null)


        {
            // Obtener el carrito actual
            var cart = await GetCartAsync(buyerId, userId);

            // Validar que el carrito no esté vacío
            if (cart.CartItems.Count == 0)
            {
                Log.Information("El carrito ya está vacío para buyerId: {BuyerId} y userId: {UserId}", buyerId, userId);
                return cart.Adapt<CartDTO>();
            }

            // Eliminar todos los items del carrito
            var isCleared = await _cartRepository.ClearCartItemsAsync(cart.Id);
            if (!isCleared)
            {
                Log.Error("Error al limpiar el carrito para buyerId: {BuyerId} y userId: {UserId}", buyerId, userId);
                throw new Exception($"Error al limpiar el carrito para buyerId: {buyerId} y userId: {userId}");
            }
            Log.Information("Carrito limpiado. CartId: {CartId}", cart.Id);

            // Actualizar el precio total del carrito a 0
            var newTotalPrice = 0;
            await _cartRepository.UpdateTotalPriceAsync(cart.Id, newTotalPrice);

            // Obtener el carrito actualizado
            cart = await GetCartAsync(buyerId, userId);

            // Mapear el carrito a CartDTO
            return cart.Adapt<CartDTO>();
        }

        /// <summary>
        /// Realiza el proceso de checkout del carrito de compras.
        /// </summary>
        /// <param name="userId">Identificador del usuario autenticado.</param>
        /// <returns>Un objeto <see cref="CheckoutResultDTO"/> con la información del resultado del checkout.</returns>
        /// <exception cref="Exception">Se lanza si ocurre un error durante el proceso de checkout.</exception>
        public async Task<CheckoutResultDTO> CheckoutCartAsync(int userId)
        {
            // Obtener el carrito actual
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null)
            {
                Log.Information("No se encontró un carrito para userId: {UserId}", userId);
                throw new Exception($"No se encontró un carrito para userId: {userId}.");
            }

            // Validar que el carrito no esté vacío
            if (cart.CartItems.Count == 0)
            {
                Log.Information("El carrito está vacío para userId: {UserId}", userId);
                throw new Exception($"El carrito está vacío para userId: {userId}.");
            }

            // Listas para almacenar los items que se actualizarán o eliminarán
            var itemsToRemove = new List<CartItem>();
            var itemsToUpdate = new List<(CartItem item, int newQuantity)>();
            bool hasChanges = false;

            // Realizar el proceso de checkout
            foreach (var cartItem in cart.CartItems)
            {

                var product = await _productRepository.GetProductByIdForCustomerAsync(cartItem.ProductId);
                if (product == null || product!.Stock <= 0)
                {
                    Log.Information("Producto ID: {ProductId} no disponible para checkout. Será removido del carrito. CartId: {CartId}", cartItem.ProductId, cart.Id);
                    itemsToRemove.Add(cartItem);
                    hasChanges = true;
                    continue;
                }


                if (product!.Stock < cartItem.Quantity)
                {
                    Log.Information("Stock insuficiente para el producto ID: {ProductId} durante el checkout. Stock disponible: {Stock}, cantidad en carrito: {Quantity}. Se actualizará la cantidad en el carrito. CartId: {CartId}", cartItem.ProductId, product.Stock, cartItem.Quantity, cart.Id);
                    itemsToUpdate.Add((cartItem, product!.Stock));
                    hasChanges = true;
                }
            }

            // Si hay items a remover o actualizar, realizar los cambios en el carrito
            if (hasChanges)
            {
                foreach (var itemToRemove in itemsToRemove)
                {
                    await _cartRepository.RemoveItemAsync(cart.Id, itemToRemove.Id);
                }
                foreach (var (item, newQuantity) in itemsToUpdate)
                {
                    await _cartRepository.UpdateItemQuantityAsync(cart.Id, item.Id, newQuantity);
                }

                Log.Information("Carrito actualizado tras checkout. ItemsEliminados: {RemovedCount}, ItemsActualizados: {UpdatedCount}", itemsToRemove.Count, itemsToUpdate.Count);

                // Obtener el carrito actualizado después de los cambios
                cart = await _cartRepository.GetByUserIdAsync(userId);

                // Actualizar el precio total del carrito
                var newTotalPrice = cart!.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);
                await _cartRepository.UpdateTotalPriceAsync(cart.Id, newTotalPrice);
                Log.Information("Precio total actualizado. CartId: {CartId}", cart.Id);

                cart.TotalPrice = newTotalPrice;
            }
            else
            {
                Log.Information("No se requirieron ajustes en el carrito durante checkout. CartId: {CartId}", cart.Id);
            }

            // Mapear el resultado del checkout a CheckoutResultDTO
            var checkoutResult = new CheckoutResultDTO
            {
                CartUpdated = cart.Adapt<CartDTO>(),
                CartUpdatesDTO = new CartUpdatesDTO
                {
                    UpdatedItemsNames = itemsToUpdate.Select(i => i.item.Product.Name).ToList() ?? null!,
                    RemovedItemsNames = itemsToRemove.Select(i => i.Product.Name).ToList() ?? null!
                }
            };
            return checkoutResult;
        }
        private async Task<int> CreateEmptyCartAsync(string buyerId, int? userId)
        {
            // Crear un nuevo carrito vacío
            var newCart = new Cart
            {
                BuyerId = buyerId,
                UserId = userId,
            };

            // Guardar el nuevo carrito en la base de datos
            var isCreated = await _cartRepository.CreateAsync(newCart);
            if (!isCreated)
            {
                Log.Error("Error al crear un nuevo carrito para buyerId: {BuyerId} y userId: {UserId}.", buyerId, userId);
                throw new Exception($"Error al crear un nuevo carrito para buyerId: {buyerId} y userId: {userId}.");
            }

            Log.Information("Nuevo carrito creado para buyerId: {BuyerId}, userId: {UserId}", buyerId, userId);
            return newCart.Id;
        }

        private async Task<Cart> GetCartAsync(string buyerId, int? userId)
        {
            Cart? cart;

            // Buscar el carrito por userId
            if (userId.HasValue)
            {
                cart = await _cartRepository.GetByUserIdAsync(userId.Value);
                if (cart == null)
                {
                    Log.Information("No se encontró un carrito para userId: {UserId}", userId.Value);
                    throw new Exception($"No se encontró un carrito para userId: {userId.Value}.");
                }
            }
            // Buscar el carrito por buyerId
            else
            {
                cart = await _cartRepository.GetByBuyerIdAsync(buyerId);
                if (cart == null)
                {
                    Log.Information("No se encontró un carrito para buyerId: {BuyerId}", buyerId);
                    throw new Exception($"No se encontró un carrito para buyerId: {buyerId}.");
                }
            }

            return cart;
        }
    }
}
