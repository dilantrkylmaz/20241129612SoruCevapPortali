using System.Linq.Expressions;

namespace _20241129612SoruCevapPortalı.Repositories.Abstract
{
    public interface IRepository<T> where T : class
    {
        List<T> GetAll();
        List<T> GetAll(Expression<Func<T, bool>> filter);

        // YENİ EKLENEN: İlişkili verileri (User, Answers) getirmek için
        List<T> GetAll(params Expression<Func<T, object>>[] includes);
        T Get(Expression<Func<T, bool>> filter, params string[] includeProperties);

        T GetById(int id);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        void Save();
    }
}