using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using ShopDomainModel.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserDomainModel;

namespace Application.Products
{
    public class List
    {
        public class Query : IRequest<List<Product>>
        { }

        public class Handler : IRequestHandler<Query, List<Product>>
        {
            private readonly ShopDbContext context;

            public Handler(ShopDbContext context)
            {
                this.context = context;
            }

            public async Task<List<Product>> Handle(Query request, CancellationToken cancellationToken)
            {
                return await context.Products.ToListAsync();
            }
        }
    }
}