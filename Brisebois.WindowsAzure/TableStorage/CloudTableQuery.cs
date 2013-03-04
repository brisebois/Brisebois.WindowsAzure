using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace Brisebois.WindowsAzure.TableStorage
{
    public abstract class CloudTableQuery<TEntity> : IModelQuery<ICollection<TEntity>, CloudTable>
    {
        public abstract ICollection<TEntity> Execute(CloudTable model);
    }
}