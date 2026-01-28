using Microsoft.EntityFrameworkCore;
using PharmacyManager.Data;
using PharmacyManager.Models;
using PharmacyManager.Models.ViewModels;
using PharmacyManager.Utilities;

namespace PharmacyManager.Repository
{
    public interface IActiveIngredientRepository
    {
        Task<bool> Add(IngredientVM model);
        Task<bool> Update(ActiveIngredient model);
        Task<bool> Delete(int Id);
        Task<IEnumerable<ActiveIngredient>> GetAllActiveIngredients();
        Task<ActiveIngredient?> GetActiveIngredientByIdAsync(int Id);
    }
    public class ActiveIngredientRepository : IActiveIngredientRepository
    {
        private readonly PharmacyDbContext _db;
        private readonly IMapper _mapper;
        public ActiveIngredientRepository(PharmacyDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<bool> Add(IngredientVM model)
        {
            try
            {
                var ingredient = _mapper.MapToActiveIngredientsModel(model);
                if (ingredient == null)
                {
                    return false;
                }
                _db.ActiveIngredients.Add(ingredient);
                await _db.SaveChangesAsync();
                
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public async Task<bool> Delete(int Id)
        {

            try
            {
                var ingredient = await _db.ActiveIngredients.FindAsync(Id);
                if (ingredient == null)
                {
                    return false;
                }
                _db.ActiveIngredients.Remove(ingredient);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public async Task<ActiveIngredient?> GetActiveIngredientByIdAsync(int id)
        {
            return await _db.ActiveIngredients
                           .AsNoTracking()
                           .FirstOrDefaultAsync(a => a.ActiveIngredientId == id);
        }

        public async Task<IEnumerable<ActiveIngredient>> GetAllActiveIngredients()
        {
            return await _db.ActiveIngredients
                           .OrderBy(a => a.IngredientName)
                           .AsNoTracking()
                           .ToListAsync();
        }


        public async Task<bool> Update(ActiveIngredient model)
        {
            var ingredient = await  _db.ActiveIngredients.FindAsync(model.ActiveIngredientId);

            if(ingredient == null)
            {
                return false;
            }           
            ingredient.IngredientName = model.IngredientName;
            _db.ActiveIngredients.Update(ingredient);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
