using Bds.TechTest.Configuration;
using Bds.TechTest.DataProviders.Search;
using Bds.TechTest.Repositories;
using Bds.TechTest.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Bds.TechTest
{
    public class Startup
    {
        private readonly IApplicationSettings configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = ApplicationSettings.Build(configuration);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwtBearerOptions =>
                {
                    jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateActor = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration.TokenIssuer,
                        ValidAudience = configuration.TokenAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(configuration.TokenSigningKey)
                    };
                });
            services.AddCors();
            services.AddMvc();
            services.AddSingleton<IMemoryCache, MemoryCache>();
            services.AddSingleton<ISavedSearchRepository, SavedSearchRepository>();
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IBingSearchDataProvider, BingSearchDataProvider>();
            services.AddSingleton<IGoogleSearchDataProvider, GoogleSearchDataProvider>();
            services.AddSingleton<ISearchService, SearchService>();
            services.AddSingleton<IUserService, UserService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseMvc();
        }
    }
}
