using System.Threading.Tasks;
using EventBus;
using Microsoft.Extensions.Logging;
using MSFramework.Data;
using MSFramework.Domain;
using Template.Application.DTO;
using Template.Domain;
using Template.Domain.AggregateRoot;
using Template.Domain.Repository;

namespace Template.Application.Service
{
	public class Class1Service : IClass1Service
	{
		private readonly IClass1Repository _class1Repository;
		private readonly ILogger _logger;
		private readonly IMSFrameworkSession _session;
		private readonly IEventBus _eventBus;
		private readonly AppOptions _options;
		private readonly IMapper _mapper;

		public Class1Service(IMSFrameworkSession session, IMapper mapper,
			IClass1Repository class1Repository,
			AppOptions options,
			ILogger<Class1Service> logger, IEventBus eventBus)
		{
			_session = session;
			_class1Repository = class1Repository;
			_options = options;
			_logger = logger;
			_eventBus = eventBus;
			_mapper = mapper;
		}


		public async Task<CreatClass1Out> CreateAsync(CreateClass1In input)
		{
			input.NotNull(nameof(input));
			var class1 = _mapper.Map<Class1>(input);
			class1.SetCreationAudited(_session.UserId ?? "unknown", _session.UserName ?? "unknown");
			var result = await _class1Repository.InsertAsync(class1);
			_logger.LogInformation($"Create class1 {class1.Name} success");
			return _mapper.Map<CreatClass1Out>(result);
		}
	}
}