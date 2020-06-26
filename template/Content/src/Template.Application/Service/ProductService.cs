using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSFramework;
using MSFramework.Common;
using MSFramework.Domain.Event;
using MSFramework.Mapper;
using Template.Application.DTO;
using Template.Application.Event;
using Template.Domain.AggregateRoot;
using Template.Domain.Repository;

namespace Template.Application.Service
{
	public class ProductService : IProductService
	{
		private readonly IProductRepository _productRepository;
		private readonly ILogger _logger;
		private readonly IObjectMapper _mapper;
		private readonly IEventDispatcher _eventDispatcher;

		public ProductService(IObjectMapper mapper,
			IProductRepository productRepository,
			ILogger<ProductService> logger, IEventDispatcher eventDispatcher)
		{
			_productRepository = productRepository;
			_logger = logger;
			_eventDispatcher = eventDispatcher;
			_mapper = mapper;
		}

		public async Task<CreatProductOut> CreateAsync(CreateProductIn @in)
		{
			@in.NotNull(nameof(@in));
			var product = _mapper.Map<Product>(@in);
			var result = await _productRepository.InsertAsync(product);
			_logger.LogInformation($"Create product {result.Name} success");
			return _mapper.Map<CreatProductOut>(result);
		}

		public async Task<ProductOut> DeleteByIdAsync(Guid productId)
		{
			productId.NotNullOrDefault(nameof(productId));
			var product = await _productRepository.GetAsync(productId);
			if (product != null)
			{
				await _productRepository.DeleteAsync(product);
				await _eventDispatcher.DispatchAsync(new ProductDeletedEvent(productId));
				return _mapper.Map<ProductOut>(product);
			}
			else
			{
				throw new MSFrameworkException(110, "Product is not exists");
			}
		}
	}
}