using GreenSpace.Application.Interfaces.Repositories;
using GreenSpace.Domain.Models;
using GreenSpace.Infrastructure.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Infrastructure.Persistence.Repositories
{
    public class ProductVariantRepository : GenericRepository<ProductVariant>, IProductVariantRepository
    {
        public ProductVariantRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
