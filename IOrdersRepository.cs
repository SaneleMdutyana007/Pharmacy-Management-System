using PharmacyManager.Data;
using PharmacyManager.Models;
using Microsoft.EntityFrameworkCore;

namespace PharmacyManager.Repositories
{
    public interface IOrdersRepository
    {
        Task<Order> CreateAsync(Order order);
        Task<bool> UpdateAsync(Order order);
        Task<bool> DeleteAsync(int orderId);
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<IEnumerable<Order>> GetPendingOrdersAsync();
        Task<IEnumerable<Order>> GetCompletedOrdersAsync();
        Task UpdateOrderAsync(Order order);
        Task<bool> ReceiveOrderAndUpdateStockAsync(int orderId);

    }

    public class OrdersRepository : IOrdersRepository
    {
        private readonly PharmacyDbContext _context;

        public OrdersRepository(PharmacyDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateAsync(Order order)
        {
            try
            {
                // Ensure Supplier exists
                var supplier = await _context.Suppliers.FindAsync(order.SupplierId);
                if (supplier == null)
                {
                    throw new Exception($"Supplier with ID {order.SupplierId} not found");
                }

                // Validate medications exist
                foreach (var orderMed in order.OrderMedications)
                {
                    var medication = await _context.Medications.FindAsync(orderMed.MedicationId);
                    if (medication == null)
                    {
                        throw new Exception($"Medication with ID {orderMed.MedicationId} not found");
                    }
                }

                // Add order (this will also add OrderMedications due to cascade)
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Explicitly reload with related data
                var createdOrder = await _context.Orders
              .Include(o => o.Supplier)
              .Include(o => o.OrderMedications)
                  .ThenInclude(om => om.Medication)
              .FirstOrDefaultAsync(o => o.OrdersId == order.OrdersId);

                return createdOrder;
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception($"Database error while creating order: {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating order: {ex.Message}", ex);
            }
        }


        public async Task<bool> UpdateAsync(Order order)
        {
            try
            {
                var existingOrder = await _context.Orders
                    .Include(o => o.OrderMedications)
                    .FirstOrDefaultAsync(o => o.OrdersId == order.OrdersId);

                if (existingOrder == null)
                    return false;

                existingOrder.Status = order.Status;
                existingOrder.ReceiveDate = order.ReceiveDate;
                existingOrder.TotalQuantity = order.TotalQuantity;
                existingOrder.TotalCost = order.TotalCost;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception($"Database error while updating order: {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating order: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAsync(int orderId)
        {
            try
            {
                // Get the order with its related medications AND notifications
                var order = await _context.Orders
                    .Include(o => o.OrderMedications)
                    .FirstOrDefaultAsync(o => o.OrdersId == orderId);

                if (order == null)
                {
                    return false;
                }

                // Remove related Notifications first
                var notifications = await _context.Notifications
                    .Where(n => n.OrderId == orderId)
                    .ToListAsync();

                if (notifications.Any())
                {
                    _context.Notifications.RemoveRange(notifications);
                }

                // Remove related OrderMedications
                if (order.OrderMedications != null && order.OrderMedications.Any())
                {
                    _context.OrderMedications.RemoveRange(order.OrderMedications);
                }

                // Then remove the order
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database error in DeleteAsync: {dbEx.InnerException?.Message}");
                throw new Exception($"Database error while deleting order: {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteAsync: {ex.Message}");
                throw new Exception($"Error deleting order: {ex.Message}", ex);
            }
        }
        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Supplier)
                .Include(o => o.OrderMedications)
                    .ThenInclude(om => om.Medication)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrdersId == orderId);
        }
       

        // Also, make sure GetCompletedOrdersAsync is defined only once:

        public async Task<IEnumerable<Order>> GetCompletedOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Supplier)
                .Include(o => o.OrderMedications)
                .Where(o => o.Status == "Completed")
                .AsNoTracking()
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
        public async Task<bool> ReceiveOrderAndUpdateStockAsync(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderMedications)
                    .FirstOrDefaultAsync(o => o.OrdersId == orderId);

                if (order == null || order.Status != "Pending")
                    return false;

                // Update order
                order.Status = "Completed";
                order.ReceiveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Update medications without tracking conflicts
                var medicationIds = order.OrderMedications.Select(om => om.MedicationId).Distinct();
                var medications = await _context.Medications
                    .Where(m => medicationIds.Contains(m.MedicationId))
                    .ToListAsync();

                foreach (var orderMed in order.OrderMedications)
                {
                    var medication = medications.FirstOrDefault(m => m.MedicationId == orderMed.MedicationId);
                    if (medication != null)
                    {
                        medication.Quantity += orderMed.Quantity;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ReceiveOrderAndUpdateStockAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Supplier)
                .Include(o => o.OrderMedications)
                .AsNoTracking()
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Supplier)
                .Include(o => o.OrderMedications)
                .Where(o => o.Status == "Pending")
                .AsNoTracking()
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

      
        public async Task UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
    }
}