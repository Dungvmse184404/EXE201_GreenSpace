using GreenSpace.Application;
using GreenSpace.Infrastructure;
using GreenSpace.Infrastructure.Persistence.Seeders;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;

namespace GreenSpace.WebAPI
{
    public class Program
    {
        public static async Task Main(string[] args)// cẩn thận với async main
        {
            var builder = WebApplication.CreateBuilder(args);

            // ==========================================
            // 1. ĐĂNG KÝ SERVICES (DEPENDENCY INJECTION)
            // ==========================================

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddHttpContextAccessor();

            // --- Cấu hình Swagger ---
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your token: Bearer {your JWT token}"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                   policy =>
                   {
                       policy.WithOrigins(
                               "http://localhost:3000",
                               "http://localhost:5173",
                               "https://green-space-exe.vercel.app"
                           )
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                   });
            });


            // Configure request size limits for file uploads
            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 100_000_000; // 100MB
            });

            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 100_000_000; // 100MB
            });

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 100_000_000; // 100MB
                options.ValueLengthLimit = int.MaxValue;
                options.ValueCountLimit = int.MaxValue;
                options.KeyLengthLimit = int.MaxValue;
            });

            // --- GỌI CÁC LAYER KHÁC ---
            builder.Services.AddInfrastructure(
                builder.Configuration,
                builder.Environment.IsDevelopment()
            );
            builder.Services.AddApplications();


            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("ADMIN"));
                options.AddPolicy("StaffOrAdmin", policy => policy.RequireRole("ADMIN", "STAFF"));
            });

            // ==========================================
            // 2. MIDDLEWARE PIPELINE (HTTP REQUEST)
            // ==========================================
            var app = builder.Build();

            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI();
            //}

            // Seed data khi khởi động ứng dụng
            using (var scope = app.Services.CreateScope())
            {
                var adminSeeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
                await adminSeeder.SeedAsync();

                var symptomSeeder = scope.ServiceProvider.GetRequiredService<SymptomDictionarySeeder>();
                await symptomSeeder.SeedAsync();

                // Seed Disease Knowledge Base (PlantTypes, Diseases, DiseaseSymptoms)
                var diseaseKnowledgeSeeder = scope.ServiceProvider.GetRequiredService<DiseaseKnowledgeSeeder>();
                await diseaseKnowledgeSeeder.SeedAsync();
            }

            app.UseCors("AllowAll");
            app.UseHttpsRedirection();

            app.UseStaticFiles();  

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
