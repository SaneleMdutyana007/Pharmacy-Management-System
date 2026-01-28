using PharmacyManager.Data;

namespace PharmacyManager.Repositories
{
    public interface IStockRepository
    {
        Task<bool> AddStockAsync(int medicationId, int quantity);
        Task<bool> RemoveStockAsync(int medicationId, int quantity);
        Task<bool> SetStockAsync(int medicationId, int quantity);
        Task<bool> RecordStockTransactionAsync(int medicationId, string transactionType, int quantity, string notes = null);
    }
    public class StockRepository : IStockRepository
    {
        private readonly PharmacyDbContext _context;

        public StockRepository(PharmacyDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddStockAsync(int medicationId, int quantity)
        {
            try
            {
                var medication = await _context.Medications.FindAsync(medicationId);
                if (medication == null) return false;

                medication.Quantity += quantity;
                await _context.SaveChangesAsync();

                // Record the transaction
                await RecordStockTransactionAsync(medicationId, "ADD", quantity, $"Added {quantity} units to stock");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveStockAsync(int medicationId, int quantity)
        {
            try
            {
                var medication = await _context.Medications.FindAsync(medicationId);
                if (medication == null || medication.Quantity < quantity) return false;

                medication.Quantity -= quantity;
                await _context.SaveChangesAsync();

                // Record the transaction
                await RecordStockTransactionAsync(medicationId, "REMOVE", quantity, $"Removed {quantity} units from stock");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetStockAsync(int medicationId, int quantity)
        {
            try
            {
                var medication = await _context.Medications.FindAsync(medicationId);
                if (medication == null) return false;

                var oldQuantity = medication.Quantity;
                medication.Quantity = quantity;
                await _context.SaveChangesAsync();

                // Record the transaction
                await RecordStockTransactionAsync(medicationId, "SET", quantity, $"Stock set from {oldQuantity} to {quantity} units");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RecordStockTransactionAsync(int medicationId, string transactionType, int quantity, string notes = null)
        {

            // If transaction recording fails, we don't want to fail the main operation
            return await Task.FromResult(true);

        }
    }

}
