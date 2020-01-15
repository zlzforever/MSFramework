// using MSFramework.Data;
// using MSFramework.Http;
//
// namespace MSFramework.AspNetCore
// {
// 	public class PagedApiResult<TEntity> : ApiResult
// 	{
// 		public PagedApiResult(PagedQueryResult<TEntity> result) : base(new PagedApiResponse
// 		{
// 			Success = true,
// 			// LayUI code = 0 为正常值
// 			Code = 0,
// 			Count = result.Total,
// 			Limit = result.Limit,
// 			Page = result.Page,
// 			Data = result.Entities
// 		})
// 		{
// 		}
// 	}
// }