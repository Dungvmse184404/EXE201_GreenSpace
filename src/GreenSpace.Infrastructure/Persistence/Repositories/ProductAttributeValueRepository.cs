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
    public class ProductAttributeValueRepository : GenericRepository<ProductAttributeValue>, IProductAttributeValueRepository
    {
        public ProductAttributeValueRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
