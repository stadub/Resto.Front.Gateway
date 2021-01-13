using System.Threading.Tasks;

namespace SqlBase
{
    public interface IDbProvider<TDatabase>
    {
        Task InitAsync(TDatabase database);
    }
}