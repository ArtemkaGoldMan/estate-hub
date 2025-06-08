using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace EstateHub.SharedKernel.Execution
{
    public interface IUnitOfWork
    {
        Task<Result<bool>> BeginTransactionAsync();

        Task<Result<bool>> CommitAsync();

        Task<Result<bool>> RollbackAsync();
    }
}
