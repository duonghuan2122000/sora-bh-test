using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Sora.BankHubTest.Dtos;
using Sora.BankHubTest.Https;
using Sora.BankHubTest.MongoDB;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;
using Volo.Abp.Timing;

namespace Sora.BankHubTest
{
    [DependsOn(
        typeof(AbpAspNetCoreMvcModule),
        typeof(AbpAspNetCoreSerilogModule),
        typeof(AbpAutofacModule),
        typeof(AbpMongoDbModule)
    )]
    public class BankHubTestModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var hostingEnvironment = context.Services.GetHostingEnvironment();
            var configuration = context.Services.GetConfiguration();

            Configure<AbpClockOptions>(options =>
            {
                options.Kind = DateTimeKind.Local;
            });

            Configure<JPBankHubOption>(configuration.GetSection(JPBankHubOption.Key));

            context.Services.AddDistributedMemoryCache();

            Configure<AbpMongoDbOptions>(options =>
            {
                options.UseAbpClockHandleDateTime = false;
            });

            BsonSerializer.RegisterSerializer(typeof(DateTime), DateTimeSerializer.LocalInstance);
            BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(BsonType.String));

            context.Services.AddMongoDbContext<BankHubTestMongoDbContext>(options =>
            {
                options.AddDefaultRepositories();
            });

            context.Services.AddHttpClient<IHttpService, HttpService>();
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            var env = context.GetEnvironment();

            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseConfiguredEndpoints();
        }
    }
}