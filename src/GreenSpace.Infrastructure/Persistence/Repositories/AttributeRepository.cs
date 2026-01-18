
using GreenSpace.Application.Interfaces.Repositories;
using GreenSpace.Infrastructure.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Infrastructure.Persistence.Repositories
{
    public class AttributeRepository : GenericRepository<Domain.Models.Attribute>, IAttributeRepository
    {
        public AttributeRepository(AppDbContext dbContext) : base(dbContext)
        {

        }
    }
}
