using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.Common
{
	public static class InitializerServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddInitializer<T1>(
			this MSFrameworkBuilder builder)
			where T1 : Initializer
		{
			builder.Services.AddSingleton<Initializer, T1>();
			return builder;
		}
		
		public static MSFrameworkBuilder AddInitializer<T1, T2>(
			this MSFrameworkBuilder builder)
			where T1 : Initializer
			where T2 : Initializer
		{
			builder.Services.AddSingleton<Initializer, T1>();
			builder.Services.AddSingleton<Initializer, T2>();
			return builder;
		}
		
		public static MSFrameworkBuilder AddInitializer<T1, T2, T3>(
			this MSFrameworkBuilder builder)
			where T1 : Initializer
			where T2 : Initializer
			where T3 : Initializer
		{
			builder.Services.AddSingleton<Initializer, T1>();
			builder.Services.AddSingleton<Initializer, T2>();
			builder.Services.AddSingleton<Initializer, T3>();
			return builder;
		}
		
		public static MSFrameworkBuilder AddInitializer<T1, T2, T3, T4>(
			this MSFrameworkBuilder builder)
			where T1 : Initializer
			where T2 : Initializer
			where T3 : Initializer
			where T4 : Initializer
		{
			builder.Services.AddSingleton<Initializer, T1>();
			builder.Services.AddSingleton<Initializer, T2>();
			builder.Services.AddSingleton<Initializer, T3>();
			builder.Services.AddSingleton<Initializer, T4>();
			return builder;
		}
		
		public static MSFrameworkBuilder AddInitializer<T1, T2, T3, T4, T5>(
			this MSFrameworkBuilder builder)
			where T1 : Initializer
			where T2 : Initializer
			where T3 : Initializer
			where T4 : Initializer
			where T5 : Initializer
		{
			builder.Services.AddSingleton<Initializer, T1>();
			builder.Services.AddSingleton<Initializer, T2>();
			builder.Services.AddSingleton<Initializer, T3>();
			builder.Services.AddSingleton<Initializer, T4>();
			builder.Services.AddSingleton<Initializer, T5>();
			return builder;
		}
		
		public static MSFrameworkBuilder AddInitializer<T1, T2, T3, T4, T5, T6>(
			this MSFrameworkBuilder builder)
			where T1 : Initializer
			where T2 : Initializer
			where T3 : Initializer
			where T4 : Initializer
			where T5 : Initializer
			where T6 : Initializer
		{
			builder.Services.AddSingleton<Initializer, T1>();
			builder.Services.AddSingleton<Initializer, T2>();
			builder.Services.AddSingleton<Initializer, T3>();
			builder.Services.AddSingleton<Initializer, T4>();
			builder.Services.AddSingleton<Initializer, T5>();
			builder.Services.AddSingleton<Initializer, T6>();
			return builder;
		}
		
		public static MSFrameworkBuilder AddInitializer<T1, T2, T3, T4, T5, T6, T7>(
			this MSFrameworkBuilder builder)
			where T1 : Initializer
			where T2 : Initializer
			where T3 : Initializer
			where T4 : Initializer
			where T5 : Initializer
			where T6 : Initializer
			where T7 : Initializer
		{
			builder.Services.AddSingleton<Initializer, T1>();
			builder.Services.AddSingleton<Initializer, T2>();
			builder.Services.AddSingleton<Initializer, T3>();
			builder.Services.AddSingleton<Initializer, T4>();
			builder.Services.AddSingleton<Initializer, T5>();
			builder.Services.AddSingleton<Initializer, T6>();
			builder.Services.AddSingleton<Initializer, T7>();
			return builder;
		}
		
		public static MSFrameworkBuilder AddInitializer<T1, T2, T3, T4, T5, T6, T7, T8>(
			this MSFrameworkBuilder builder)
			where T1 : Initializer
			where T2 : Initializer
			where T3 : Initializer
			where T4 : Initializer
			where T5 : Initializer
			where T6 : Initializer
			where T7 : Initializer
			where T8 : Initializer
		{
			builder.Services.AddSingleton<Initializer, T1>();
			builder.Services.AddSingleton<Initializer, T2>();
			builder.Services.AddSingleton<Initializer, T3>();
			builder.Services.AddSingleton<Initializer, T4>();
			builder.Services.AddSingleton<Initializer, T5>();
			builder.Services.AddSingleton<Initializer, T6>();
			builder.Services.AddSingleton<Initializer, T7>();
			builder.Services.AddSingleton<Initializer, T8>();
			return builder;
		}
		
		public static MSFrameworkBuilder AddInitializer<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
			this MSFrameworkBuilder builder)
			where T1 : Initializer
			where T2 : Initializer
			where T3 : Initializer
			where T4 : Initializer
			where T5 : Initializer
			where T6 : Initializer
			where T7 : Initializer
			where T8 : Initializer
			where T9 : Initializer
		{
			builder.Services.AddSingleton<Initializer, T1>();
			builder.Services.AddSingleton<Initializer, T2>();
			builder.Services.AddSingleton<Initializer, T3>();
			builder.Services.AddSingleton<Initializer, T4>();
			builder.Services.AddSingleton<Initializer, T5>();
			builder.Services.AddSingleton<Initializer, T6>();
			builder.Services.AddSingleton<Initializer, T7>();
			builder.Services.AddSingleton<Initializer, T8>();
			builder.Services.AddSingleton<Initializer, T9>();
			return builder;
		}
	}
}