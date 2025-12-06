using System.Linq.Expressions;

namespace _20241129612SoruCevapPortalı.Repositories.Abstract
{
    public interface IRepository<T> where T : class
    {
        // 1. İlişkili verileri getiren (veya boş çağrılınca düz getiren) metod
        List<T> GetAll(params Expression<Func<T, object>>[] includes);

        // 2. Filtreli getirme
        List<T> GetAll(Expression<Func<T, bool>> filter);

        // 3. Tekil veri getirme (ilişkilerle)
        T Get(Expression<Func<T, bool>> filter, params string[] includeProperties);

        T GetById(int id);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        void Save();
    }
}