using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManager.Data;
using PharmacyManager.Models;
using PharmacyManager.Repositories;
using PharmacyManager.Repository;
using PharmacyManager.Services;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PharmacyManager.Controllers
{
    public class OrdersController : BaseController
    {
        private readonly IMedicationRepository _medicationRepo;
        private readonly ISupplierRepository _supplierRepo;
        private readonly IOrdersRepository _ordersRepo;
        private readonly INotificationService _notificationService;
        private readonly EmailService _emailService;

        public OrdersController(
            IMedicationRepository medicationRepo,
            ISupplierRepository supplierRepo,
            IOrdersRepository ordersRepo,
            INotificationService notificationService,
            EmailService emailService)
        {
            _medicationRepo = medicationRepo;
            _supplierRepo = supplierRepo;
            _ordersRepo = ordersRepo;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index(string tab = "new", string searchString = "", int page = 1)
        {
            ViewBag.ActiveTab = tab;
            ViewBag.SearchString = searchString;
            ViewBag.CurrentPage = page;

            var suppliers = await _supplierRepo.GetAllSuppliers();
            ViewBag.Suppliers = suppliers;
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetMedications(string searchString = "", int page = 1, int pageSize = 14)
        {
            try
            {
                var medications = await _medicationRepo.GetAllMedications();

                if (medications == null || !medications.Any())
                {
                    return Json(new
                    {
                        success = true,
                        medications = new List<object>(),
                        totalCount = 0,
                        currentPage = 1,
                        totalPages = 1
                    });
                }

                // Apply search filter
                if (!string.IsNullOrEmpty(searchString))
                {
                    medications = medications.Where(m =>
                        m.MedicationName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                        m.Supplier.SupplierName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    );
                }

                var totalCount = medications.Count();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                // Apply pagination
                var pagedMedications = medications
                    .OrderBy(m => m.MedicationName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var result = pagedMedications.Select(m => new
                {
                    id = m.MedicationId,
                    name = m.MedicationName,
                    supplier = m.Supplier?.SupplierName ?? "No Supplier",
                    currentStock = m.Quantity,
                    reorderLevel = m.ReOrderLevel,
                    unitPrice = m.Price
                }).ToList();

                return Json(new
                {
                    success = true,
                    medications = result,
                    totalCount = totalCount,
                    currentPage = page,
                    totalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Failed to load medications." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new { success = false, error = "Invalid request data." });
                }

                if (string.IsNullOrEmpty(request.Supplier) || request.Supplier == "all")
                {
                    return Json(new { success = false, error = "Please select a valid supplier." });
                }

                if (request.Medications == null || !request.Medications.Any())
                {
                    return Json(new { success = false, error = "No medications selected." });
                }

                var suppliers = await _supplierRepo.GetAllSuppliers();
                var supplier = suppliers.FirstOrDefault(s => s.SupplierName == request.Supplier);
                if (supplier == null)
                {
                    return Json(new { success = false, error = $"Supplier '{request.Supplier}' not found." });
                }

                // Validate medications exist
                foreach (var item in request.Medications)
                {
                    var medication = await _medicationRepo.GetMedicationByIdAsync(item.MedicationId);
                    if (medication == null)
                    {
                        return Json(new { success = false, error = $"Medication with ID {item.MedicationId} not found." });
                    }
                }

                var order = new Order
                {
                    SupplierId = supplier.SupplierId,
                    OrderDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = "Pending",
                    TotalQuantity = request.TotalQuantity,
                    TotalCost = request.TotalCost
                };

                order.OrderMedications = request.Medications.Select(m => new OrderMedication
                {
                    MedicationId = m.MedicationId,
                    Quantity = m.Quantity,
                    UnitPrice = m.UnitPrice
                }).ToList();

                var createdOrder = await _ordersRepo.CreateAsync(order);

                if (createdOrder != null)
                {
                    // Send email to supplier
                    await SendOrderEmailToSupplier(createdOrder, supplier);

                    await _notificationService.CreateNotificationAsync(
                        "order_placed",
                        $"New order placed with {request.Supplier} for {request.Medications.Count} items",
                        "Order",
                        createdOrder.OrdersId
                    );

                    await _notificationService.CheckAndCreateStockNotificationsAsync();

                    return Json(new
                    {
                        success = true,
                        orderId = createdOrder.OrdersId,
                        message = "Order placed successfully"
                    });
                }
                else
                {
                    return Json(new { success = false, error = "Failed to create order." });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = "Failed to place order. Please try again.",
                });
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetPendingOrders()
        {
            try
            {
                var pendingOrders = await _ordersRepo.GetPendingOrdersAsync();
                var result = pendingOrders.Select(o => new
                {
                    orderId = o.OrdersId,
                    supplier = o.Supplier?.SupplierName ?? "Unknown",
                    orderDate = o.OrderDate,
                    items = o.OrderMedications?.Count ?? 0,
                    total = o.TotalCost,
                    status = o.Status
                }).ToList();

                return Json(new { success = true, orders = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Failed to load pending orders." });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetOrderDetails(int orderId)
        {
            try
            {
                var order = await _ordersRepo.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return Json(new { success = false, error = "Order not found." });
                }

                var medicationsList = new List<object>();

                if (order.OrderMedications != null && order.OrderMedications.Any())
                {
                    foreach (var om in order.OrderMedications)
                    {
                        medicationsList.Add(new
                        {
                            name = om.Medication?.MedicationName ?? "Unknown",
                            quantity = om.Quantity,
                            unitPrice = om.UnitPrice,
                            totalPrice = om.Quantity * om.UnitPrice
                        });
                    }
                }

                var orderDetails = new
                {
                    orderId = order.OrdersId,
                    orderNumber = $"ORD-{order.OrdersId:D4}",
                    supplier = order.Supplier?.SupplierName ?? "Unknown",
                    orderDate = order.OrderDate,
                    status = order.Status,
                    receiveDate = order.ReceiveDate ?? "Not received",
                    totalQuantity = order.TotalQuantity,
                    totalCost = order.TotalCost,
                    medications = medicationsList
                };

                return Json(new { success = true, order = orderDetails });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Failed to load order details." });
            }
        }


        [HttpPost]
        public async Task<JsonResult> ReceiveOrder([FromBody] ReceiveOrderRequest request)
        {
            try
            {
                var result = await _ordersRepo.ReceiveOrderAndUpdateStockAsync(request.OrderId);

                if (result)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Order received successfully and stock updated",
                        orderId = request.OrderId
                    });
                }
                else
                {
                    return Json(new { success = false, error = "Order not found or not in pending status." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ReceiveOrder: {ex.Message}");
                return Json(new { success = false, error = "Failed to receive order. Please try again." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> DeleteOrder([FromBody] DeleteOrderRequest request)
        {
            try
            {
                // Get order details before deletion for email
                var order = await _ordersRepo.GetOrderByIdAsync(request.OrderId);
                if (order == null)
                {
                    return Json(new { success = false, error = "Order not found." });
                }

                var supplier = order.Supplier;
                var orderItems = order.OrderMedications?.Select(om =>
                    (om.Medication?.MedicationName ?? "Unknown Medication", om.Quantity)).ToList() ?? new List<(string, int)>();

                // Delete the order
                var result = await _ordersRepo.DeleteAsync(request.OrderId);
                if (result)
                {
                    // Send cancellation email to supplier
                    await SendOrderCancellationEmail(order, supplier, orderItems);

                    return Json(new { success = true, message = "Order deleted successfully." });
                }
                else
                {
                    return Json(new { success = false, error = "Order not found." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteOrder: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = $"Failed to delete order: {ex.Message}",
                    details = ex.InnerException?.Message
                });
            }
        }
        private async Task SendOrderEmailToSupplier(Order order, Supplier supplier)
        {
            try
            {
                if (string.IsNullOrEmpty(supplier.Email))
                {
                    Console.WriteLine($"No email address for supplier: {supplier.SupplierName}");
                    return;
                }

                var orderItems = order.OrderMedications?.Select(om =>
                    (om.Medication?.MedicationName ?? "Unknown Medication", om.Quantity)).ToList() ?? new List<(string, int)>();

                var orderNumber = $"ORD-{order.OrdersId:D4}";
                var orderDate = DateTime.Parse(order.OrderDate);
                var emailBody = EmailBody.ManagerOrderEmail(supplier.SupplierName, orderNumber, orderDate, orderItems);
                var subject = $"New Order - {orderNumber} - IBhayi Pharmacy";

                await _emailService.SendEmailAsync(supplier.Email, subject, emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending order email to supplier: {ex.Message}");
                // Don't throw - email failure shouldn't prevent order creation
            }
        }

        private async Task SendOrderCancellationEmail(Order order, Supplier supplier, List<(string MedicationName, int Quantity)> orderItems)
        {
            try
            {
                if (string.IsNullOrEmpty(supplier.Email))
                {
                    Console.WriteLine($"No email address for supplier: {supplier.SupplierName}");
                    return;
                }

                var orderNumber = $"ORD-{order.OrdersId:D4}";
                var orderDate = DateTime.Parse(order.OrderDate);
                var emailBody = EmailBody.OrderCancellationEmail(supplier.SupplierName, orderNumber, orderDate, orderItems);
                var subject = $"Order Cancelled - {orderNumber} - IBhayi Pharmacy";

                await _emailService.SendEmailAsync(supplier.Email, subject, emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending cancellation email to supplier: {ex.Message}");
                // Don't throw - email failure shouldn't prevent order deletion
            }
        }


        [HttpGet]
        public async Task<JsonResult> GetCompletedOrders()
        {
            try
            {
                var completedOrders = await _ordersRepo.GetCompletedOrdersAsync();
                var result = completedOrders.Select(o => new
                {
                    orderId = o.OrdersId,
                    supplier = o.Supplier?.SupplierName ?? "Unknown",
                    orderDate = o.OrderDate,
                    receivedDate = o.ReceiveDate ?? "Not received",
                    items = o.OrderMedications?.Count ?? 0,
                    total = o.TotalCost,
                    status = o.Status
                }).ToList();

                return Json(new { success = true, orders = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCompletedOrders: {ex.Message}");
                return Json(new { success = false, error = "Failed to load completed orders." });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetOrderSummary(int orderId)
        {
            try
            {
                var order = await _ordersRepo.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return Json(new { success = false, error = "Order not found." });
                }

                var medicationsList = new List<object>();
                if (order.OrderMedications != null && order.OrderMedications.Any())
                {
                    foreach (var om in order.OrderMedications)
                    {
                        medicationsList.Add(new
                        {
                            name = om.Medication?.MedicationName ?? "Unknown",
                            quantity = om.Quantity,
                            unitPrice = om.UnitPrice,
                            totalPrice = om.Quantity * om.UnitPrice,
                            currentStock = om.Medication?.Quantity ?? 0
                        });
                    }
                }

                var orderSummary = new
                {
                    orderId = order.OrdersId,
                    orderNumber = $"ORD-{order.OrdersId:D4}",
                    supplier = order.Supplier?.SupplierName ?? "Unknown",
                    orderDate = order.OrderDate,
                    totalQuantity = order.TotalQuantity,
                    totalCost = order.TotalCost,
                    medications = medicationsList
                };

                return Json(new { success = true, order = orderSummary });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Failed to load order summary." });
            }
        }
    }

    public class ReceiveOrderRequest
    {
        public int OrderId { get; set; }
    }
    public class DeleteOrderRequest
    {
        public int OrderId { get; set; }
    }

    public class OrderRequest
    {
        public string Supplier { get; set; } = string.Empty;
        public List<OrderItem> Medications { get; set; } = new List<OrderItem>();
        public int TotalQuantity { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class OrderItem
    {
        public int MedicationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}