using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions;

namespace MicroserviceFramework.Domain
{
	/// <summary>
	/// 工作单元管理器
	/// </summary>
	public class UnitOfWorkManager
	{
		/// <summary>
		/// 工作单元集合
		/// </summary>
		private readonly Dictionary<Guid, IUnitOfWork> _unitOfWorks;

		/// <summary>
		/// 初始化工作单元管理器
		/// </summary>
		public UnitOfWorkManager()
		{
			_unitOfWorks = new Dictionary<Guid, IUnitOfWork>();
		}

		/// <summary>
		/// 提交
		/// </summary>
		public async Task CommitAsync()
		{
			foreach (var unitOfWork in _unitOfWorks)
			{
				await unitOfWork.Value.CommitAsync();
			}
		}

		/// <summary>
		/// 注册工作单元
		/// </summary>
		/// <param name="unitOfWork">工作单元</param>
		public void Register(IUnitOfWork unitOfWork)
		{
			if (unitOfWork == null)
			{
				throw new ArgumentNullException(nameof(unitOfWork));
			}

			_unitOfWorks.GetOrAdd(unitOfWork.Id, unitOfWork);
		}

		public IReadOnlyCollection<IUnitOfWork> GetUnitOfWorks()
		{
			return _unitOfWorks.Values;
		}
	}
}