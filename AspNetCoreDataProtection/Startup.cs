using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using AspNetCore.DataProtection.Aws.S3;
using Middlewares;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace AspNetCoreDataProtection
{
    public partial class Startup
    {
        //FieldName in html form client for detected by middleware
        public const string _formFieldName = "X-CSRF-TOKEN";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Service DistCache Use In Memory Cache
            //services.AddDistributedMemoryCache();

            // Add Service DistCache Use Sql Server 
            services.AddDistributedSqlServerCache(options =>
            {
                // Connection String ของ SQL Server
                options.ConnectionString = @"Data Source=AZKALR/SQLSERVER;Initial Catalog=DistCache;Integrated Security=True;";
                options.SchemaName = "dbo";
                //Table ที่ใช้
                options.TableName = "ExampleCache";
            });
            // Set Field Name in html client form
            services.AddAntiforgery(option =>
            {
                option.FormFieldName = _formFieldName;
            });


            #region AWS Data Protection Key Store
            //Require library you can download in NuGet
            //1. AWSSDK.Extensions.NETCore.Setup
            //2. AWSSDK.S3
            //3. AspNetCore.DataProtection.Aws.S3
            //Add DI AWS Options 
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            //Add DI AWS Service 
            services.AddAWSService<IAmazonS3>();
            //Add DI AWS Data Protection 
            services.AddDataProtection()
                    .SetApplicationName("AppName") //Set App Name For Load Balance 
                    .PersistKeysToAwsS3(Configuration.GetSection("S3")); // Use S3 for Store Keys
            //# Store key on local
            //.PersistKeysToFileSystem(new DirectoryInfo(@"c:\temp-keys\"));


            #endregion

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            //For DistCache In Memorie
            //app.UseMiddleware<PreventDuplicate>();

            //For DistCacheServer SQL
            app.UseMiddleware<PreventDuplicateSql>();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
