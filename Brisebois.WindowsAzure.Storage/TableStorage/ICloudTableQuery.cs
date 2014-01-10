using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Brisebois.WindowsAzure.Storage.TableStorage
{
    public interface ICloudTableQuery<TEntity> where TEntity : ITableEntity, new()
    {
        Task<ICollection<TEntity>> Execute(CloudTable model);
        TableRequestOptions MakeTableRequestOptions();
        string UniqueIdentifier();
    }
}