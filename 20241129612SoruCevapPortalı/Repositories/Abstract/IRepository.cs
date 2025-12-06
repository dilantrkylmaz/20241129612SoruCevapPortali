using System.Linq.Expressions;

namespace _20241129612SoruCevapPortalı.Repositories.Abstract
{
    public interface IRepository<T> where T : class
    {
        // Tüm verileri getir
        List<T> GetAll();

        // Şarta göre filtreleyerek getir (örneğin: sadece bir kullanıcının soruları)
        List<T> GetAll(Expression<Func<T, bool>> filter);

        // ID'ye göre tek veri getir
        T GetById(int id);

        // Ekleme, Güncelleme, Silme
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);

        // Değişiklikleri kaydet (Save)
        void Save();
    }
}