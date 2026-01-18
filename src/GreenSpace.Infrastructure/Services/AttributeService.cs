using GreenSpace.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Infrastructure.Services
{
    public class AttributeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AttributeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


    }
}
