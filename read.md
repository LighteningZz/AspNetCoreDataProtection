# ASP.NET Data Protection AntiForgery
## Table of Content
* [Project นี้มีอะไรบ้าง](#Project)
* [Package](#Package)
* [ConfigureServices ](#ConfigureServices)
  * [Antiforgery ](#Antiforgery)
  * [Distributed Cache In Memory Cache ](#DistMEM)
  * [Distributed Cache SQL Server ](#DistSQL)
  * [Data Protection AWS S3 Key Store](#AWS)
* [AppSettings.json](#app-settings)
* [Middleware](#middleware)

---
## <a name="Project">Project</a>  นี้มีอะไรบ้าง
* Data Protection persit key store s3 เก็บ Key ไว้บน AWS S3 เพื้อให้ Container เข
* Antiforgery ป้องกัน CSRF (Cross-Site Request Forgery)
* Middleware Prevent Duplicate Request ป้องกันการส่งข้อมูลซ้ำจาก Client
---
## <a name="Package">Package</a>  ที่ใช้ให้โหลดผ่าน NuGet
* AWSSDK.S3
* AWSSDK.Extensions.NETCore.Setup
* AspNetCore.DataProtection.Aws.S3

ตัวอย่างในไฟล์ example.csproj
```xml 
    <PackageReference Include="AspNetCore.DataProtection.Aws.S3" Version="2.0.0" />
    <PackageReference Include="AWSSDK.Extensions.NETCore.Setup" Version="3.3.4" />
    <PackageReference Include="AWSSDK.S3" Version="3.3.16" />
```
---
## Startup.cs เพิ่มอะไรบ้าง
* เพิ่ม Keyword "partial" ใน Class Startup 
```csharp 
public partial class Startup
```
* เพิ่ม variable ไว้ภายใต้ Class Startup
```csharp 
public const string _formFieldName = "X-CSRF-TOKEN";
```
## <a name="ConfigureServices">ConfigureServices(IServiceCollection services) </a>
### เพิ่ม Service <a name="Antiforgery">Antiforgery</a> และ ตั้งค่า Distributed Cache 
* เพิ่ม Service Antiforgery
```csharp สำหรับ
public void ConfigureServices(IServiceCollection services)
{
    // Set Field Name in html client form
    services.AddAntiforgery(option =>
    {
        option.FormFieldName = _formFieldName;
    });
    services.AddMvc();
}
```
### เพิ่ม Distributed Cache Server สำหรับ <a name="DistMEM">In Memory Cache</a> 
* ไม่เหมาะสำหรับงานที่ใช้เซิฟเวอร์หลายตัว
```csharp 
public void ConfigureServices(IServiceCollection services)
{
    // Add Service DistCache Use In Memory Cache
    services.AddDistributedMemoryCache();
    // Set Field Name in html client form
    services.AddAntiforgery(option =>
    {
        option.FormFieldName = _formFieldName;
    });
    services.AddMvc();
}
```
### เพิ่ม Distributed Cache Server สำหรับ <a name="DistSQL">SQL Server</a>
```csharp 
public void ConfigureServices(IServiceCollection services)
{

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
    services.AddMvc();
}
```
## การตั้งค่าใน SQL Server
* การตั้งค่า Sql Server โดย  SqlConfig.Tools [Docs](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed#using-a-sql-server-distributed-cache)
* หรือ SQL Query
```sql
CREATE TABLE [dbo].[ExampleCache](
	[Id] [nvarchar](449) NOT NULL,
	[Value] [varbinary](max) NOT NULL,
	[ExpiresAtTime] [datetimeoffset](7) NOT NULL,
	[SlidingExpirationInSeconds] [bigint] NULL,
	[AbsoluteExpiration] [datetimeoffset](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
```
### Data Protection <a name="AWS">AWS S3 Key Store <a/>
* เพิ่มหลัง Add Distributed Cache
```csharp
public void ConfigureServices(IServiceCollection services)
{
     
    #region AWS Data Protection Key Store
    //Add DI AWS Options 
    services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
    //Add DI AWS Service 
    services.AddAWSService<IAmazonS3>();
    //Add DI AWS Data Protection 
    services.AddDataProtection()
            .SetApplicationName("AppName") //Set App Name For Swarm or Load Balance Server
            .PersistKeysToAwsS3(Configuration.GetSection("S3")); // Use S3 for Store Keys
    //# Store key on local for not load balance or Development
    //.PersistKeysToFileSystem(new DirectoryInfo(@"c:\temp-keys\"));
    #endregion
    services.AddMvc();
}
```
## <a name="app-settings">App Setting Json File<a/>
* AWS [[DOC]](http://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-config-netcore.html)
* S3 [[GIT]](https://github.com/hotchkj/AspNetCore.DataProtection.Aws)
```json
{
 "AWS": {
    "Region": "ap-southeast-1",
    "Profile": "default"
  },
  "S3": {
    "bucket": "cits-keystore",
    "keyPrefix": "AspNetCoreDataProtection/"
  },
}
```
## <a name="Middleware">Middleware</a>
* Middlewares\PreventDuplicate.cs สำหรับ In memory
* Middlewares\PreventDuplicateSql.cs สำหรับ SQL
