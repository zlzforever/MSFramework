using System.Threading.Tasks;
using MSFramework.DependencyInjection;
using Template.Application.DTO;
using Template.Domain;
using Template.Domain.AggregateRoot;

namespace Template.Application.Service
{
	public interface IClass1Service : IScopeDependency
	{
		Task<CreatClass1Out> CreateAsync(CreateClass1In class1);
	}
}