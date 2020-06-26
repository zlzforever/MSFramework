using System;
using System.Threading.Tasks;
using MSFramework.DependencyInjection;
using Template.Application.DTO;

namespace Template.Application.Service
{
	public interface IProductService : IScopeDependency
	{
		Task<CreatProductOut> CreateAsync(CreateProductIn @in);
		Task<ProductOut> DeleteByIdAsync(Guid productId);
	}
}