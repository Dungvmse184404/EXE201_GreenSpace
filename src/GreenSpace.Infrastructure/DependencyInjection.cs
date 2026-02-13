using GreenSpace.Application.Common.Mail;
using GreenSpace.Application.Common.Settings;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.External;
using GreenSpace.Application.Interfaces.Repositories;
using GreenSpace.Application.Interfaces.Security;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Application.Services;
using GreenSpace.Infrastructure.Authentication;
using GreenSpace.Infrastructure.ExternalServices;
using GreenSpace.Infrastructure.Helpers;
using GreenSpace.Infrastructure.Persistence;
using GreenSpace.Infrastructure.Persistence.Contexts;
using GreenSpace.Infrastructure.Persistence.Repositories;
using GreenSpace.Infrastructure.Persistence.Seeders;
using GreenSpace.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NETCore.MailKit.Core;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;


namespace GreenSpace.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
        {
            // Bind cấu hình từ appsettings
            var jwtSettings = new JwtSettings();
            configuration.GetSection("JwtSettings").Bind(jwtSettings);
            services.AddSingleton(jwtSettings); // Inject để dùng trong TokenService

            // Cấu hình Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });


            // Cấu hình DbContext với PostgreSQL
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly("GreenSpace.Infrastructure");
                        npgsqlOptions.CommandTimeout(60);
                        //npgsqlOptions.EnableRetryOnFailure(
                        //    maxRetryCount: 3, // Thường để 3-5
                        //    maxRetryDelay: TimeSpan.FromSeconds(15),
                        //    errorCodesToAdd: null);
                    });

                // Fix timezone 
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

                if (isDevelopment)
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                    options.LogTo(Console.WriteLine, LogLevel.Information);
                }
                else
                {
                    options.LogTo(Console.WriteLine, LogLevel.Warning);
                }
            }, ServiceLifetime.Scoped);

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserAddressRepository, UserAddressRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
            services.AddScoped<IAttributeRepository, AttributeRepository>();
            services.AddScoped<IProductAttributeValueRepository, ProductAttributeValueRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IPromotionRepository, PromotionRepository>();
            services.AddScoped<IRatingRepository, RatingRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICartItemRepository, CartItemRepository>();

            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<ITokenService, TokenService>();

            // Đăng ký dịch vụ gửi email
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<IEmailTemplateHelper, EmailTemplateHelper>();
            services.AddScoped<IOtpService, OtpService>();

            services.AddScoped<IAuthService, AuthService>();

            // Đăng ký dịch vụ thanh toán VNPay
            services.Configure<VNPaySettings>(configuration.GetSection("VNPaySettings"));
            services.AddScoped<IVNPayService, VNPayService>();
            services.AddScoped<IPaymentService, PaymentService>();

            services.Configure<ClientSettings>(configuration.GetSection("ClientSettings"));


            services.Configure<PayOSSettings>(configuration.GetSection("PayOSSettings"));
            services.AddScoped<IPayOSService, PayOSService>();

            // =================================================================
            // AI DIAGNOSIS SERVICES (Groq - using Llama 3.2 Vision)
            // Configured as optional - if API key is not set, service returns unavailable
            // To switch back to Gemini, change GroqService to GeminiService
            // =================================================================
            services.Configure<GroqSettings>(configuration.GetSection(GroqSettings.SectionName));
            services.AddHttpClient<IAIVisionService, GroqService>();

            // Diagnosis Cache Service - semantic matching for cached diagnosis results
            services.AddScoped<IDiagnosisCacheService, DiagnosisCacheService>();

            // Disease Knowledge Base Service - expert-curated disease data
            services.AddScoped<IDiseaseKnowledgeService, DiseaseKnowledgeService>();

            // Main Diagnosis Service - KB → Cache → AI priority
            services.AddScoped<IDiagnosisService, DiagnosisService>();

            // Keep Gemini config for potential future use
            services.Configure<GeminiSettings>(configuration.GetSection(GeminiSettings.SectionName));

            // Đăng ký dịch vụ quản lý kho hàng
            services.AddScoped<IStockService, StockService>();
            // Đăng ký dịch vụ chạy ngầm để dọn dẹp kho hàng
            services.AddHostedService<StockCleanupService>();

            services.AddDistributedMemoryCache();

            // Đăng ký Seeder
            services.AddScoped<AdminSeeder>();
            services.AddScoped<SymptomDictionarySeeder>();
            services.AddScoped<DiseaseKnowledgeSeeder>();
            services.Configure<DefaultAdmin>(configuration.GetSection("Seeder:DefaultAdmin"));

            return services;
        }


        ////chỗ này nghịch ngu đưng động vào
        //public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        //{
        //    if (services == null) throw new ArgumentNullException(nameof(services));

        //    services.TryAddScoped<IPasswordService, PasswordService>();

        //    var asm = Assembly.GetExecutingAssembly();

        //    var implTypes = asm.GetTypes()
        //        .Where(t => t.IsClass && !t.IsAbstract && (t.Name.EndsWith("Repository", StringComparison.Ordinal)
        //                                                 || t.Name.EndsWith("Service", StringComparison.Ordinal)));

        //    foreach (var impl in implTypes)
        //    {
        //        // Prefer interface named "I{ImplName}" (exact match), otherwise pick an interface that ends with the impl name.
        //        var iface = impl.GetInterfaces()
        //            .FirstOrDefault(i => string.Equals(i.Name, "I" + impl.Name, StringComparison.Ordinal))
        //            ?? impl.GetInterfaces()
        //                   .FirstOrDefault(i => i.Name.EndsWith(impl.Name, StringComparison.Ordinal));

        //        if (iface != null)
        //        {
        //            services.TryAddScoped(iface, impl);
            //        }
        //    }

        //    return services;
        //}
    }
}