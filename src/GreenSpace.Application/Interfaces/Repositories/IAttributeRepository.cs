using GreenSpace.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Attribute = GreenSpace.Domain.Models.Attribute;

namespace GreenSpace.Application.Interfaces.Repositories
{
    public interface IAttributeRepository : IGenericRepository<Attribute>
    {
    }
}
