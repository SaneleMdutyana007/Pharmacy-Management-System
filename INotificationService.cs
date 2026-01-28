using PharmacyManager.Data;
using PharmacyManager.Models;
using PharmacyManager.Models.ViewModels;
using Microsoft.EntityFrameworkCore;


namespace PharmacyManager.Services
{
    public interface INotificationService
    {
        Task<List<NotificationVM>> GetRecentNotificationsAsync(int count = 10);
        Task CreateNotificationAsync(string type, string message, string entityType = null, int? entityId = null);
        Task CheckAndCreateStockNotificationsAsync();
        Task MarkAsReadAsync(int notificationId);
        Task<int> GetUnreadCountAsync();
    }
    public class NotificationService : INotificationService
    {
        private readonly PharmacyDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(PharmacyDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<NotificationVM>> GetRecentNotificationsAsync(int count = 5)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => !n.IsRead)
                    .OrderByDescending(n => n.Timestamp)
                    .Take(count)
                    .ToListAsync();

                var result = notifications.Select(n => new NotificationVM
                {
                    Id = n.NotificationId,
                    Type = n.Type,
                    Message = n.Message,
                    Timestamp = n.Timestamp,
                    IsRead = n.IsRead,
                    EntityType = n.EntityType,
                    EntityId = GetEntityId(n)
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent notifications");
                return new List<NotificationVM>();
            }
        }

        private int? GetEntityId(Notification notification)
        {
            if (string.IsNullOrEmpty(notification.EntityType)) return null;

            return notification.EntityType.ToLower() switch
            {
                "medication" => notification.MedicationId,
                "doctor" => notification.DoctorId,
                "supplier" => notification.SupplierId,
                "order" => notification.OrderId,
                _ => null
            };
        }

        public async Task CreateNotificationAsync(string type, string message, string entityType = null, int? entityId = null)
        {
            try
            {
                var notification = new Notification
                {
                    Type = type,
                    Message = message,
                    Timestamp = DateTime.Now,
                    IsRead = false,
                    EntityType = entityType
                };

                // Set the appropriate foreign key based on entity type
                switch (entityType?.ToLower())
                {
                    case "medication":
                        notification.MedicationId = entityId;
                        break;
                    case "doctor":
                        notification.DoctorId = entityId;
                        break;
                    case "supplier":
                        notification.SupplierId = entityId;
                        break;
                    case "order":
                        notification.OrderId = entityId;
                        break;
                }

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Notification created: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
            }
        }

        public async Task CheckAndCreateStockNotificationsAsync()
        {
            try
            {
                var medications = await _context.Medications
                    .Include(m => m.Supplier)
                    .ToListAsync();

                foreach (var medication in medications)
                {
                    // Check for critical stock (at or below reorder level)
                    if (medication.Quantity <= medication.ReOrderLevel)
                    {
                        var existingNotification = await _context.Notifications
                            .Where(n => n.MedicationId == medication.MedicationId &&
                                      n.Type == "stock_critical" &&
                                      !n.IsRead)
                            .FirstOrDefaultAsync();

                        if (existingNotification == null)
                        {
                            await CreateNotificationAsync(
                                "stock_critical",
                                $"Critical stock alert: {medication.MedicationName} has only {medication.Quantity} units left (reorder level: {medication.ReOrderLevel})",
                                "Medication",
                                medication.MedicationId
                            );
                        }
                    }
                    // Check for low stock (within 10 units of reorder level)
                    else if (medication.Quantity <= medication.ReOrderLevel + 10)
                    {
                        var existingNotification = await _context.Notifications
                            .Where(n => n.MedicationId == medication.MedicationId &&
                                      n.Type == "stock_low" &&
                                      !n.IsRead)
                            .FirstOrDefaultAsync();

                        if (existingNotification == null)
                        {
                            await CreateNotificationAsync(
                                "stock_low",
                                $"Low stock: {medication.MedicationName} is running low with {medication.Quantity} units",
                                "Medication",
                                medication.MedicationId
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock notifications");
            }
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

                if (notification != null)
                {
                    notification.IsRead = true;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
            }
        }

        public async Task<int> GetUnreadCountAsync()
        {
            try
            {
                return await _context.Notifications
                    .Where(n => !n.IsRead)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count");
                return 0;
            }
        }
    }

}
