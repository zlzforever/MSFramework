using System.Collections.Generic;
using System.Threading.Tasks;
using MicroserviceFramework.Audit;
using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Ef
{
	/// <summary>
	/// 工作单元管理器
	/// </summary>
	public class EfUnitOfWork : IUnitOfWork
	{
		private readonly DbContextFactory _dbContextFactory;

		/// <summary>
		/// 初始化工作单元管理器
		/// </summary>
		public EfUnitOfWork(DbContextFactory dbContextFactory)
		{
			_dbContextFactory = dbContextFactory;
		}

		public IEnumerable<AuditEntity> GetAuditEntities()
		{
			foreach (var dbContextBase in _dbContextFactory.GetAllDbContexts())
			{
				foreach (var auditEntity in dbContextBase.GetAuditEntities())
				{
					yield return auditEntity;
				}
			}
		}

		/// <summary>
		/// 提交
		/// </summary>
		public async Task CommitAsync()
		{
			foreach (var dbContext in _dbContextFactory.GetAllDbContexts())
			{
				await dbContext.CommitAsync();
			}
		}

		// /// <summary>
		// /// 注册工作单元
		// /// </summary>
		// /// <param name="unitOfWork">工作单元</param>
		// public void Register(IUnitOfWork unitOfWork)
		// {
		// 	if (unitOfWork == null)
		// 	{
		// 		throw new ArgumentNullException(nameof(unitOfWork));
		// 	}
		//
		// 	_unitOfWorks.GetOrAdd(unitOfWork.Id, unitOfWork);
		// }

		// public IReadOnlyCollection<IUnitOfWork> GetUnitOfWorks()
		// {
		// 	return _unitOfWorks.Values;
		// }
	}
}