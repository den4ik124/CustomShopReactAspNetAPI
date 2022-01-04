using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using ShopDomainModel.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserDomainModel;

namespace Application.Orders
{
    public class List
    {
        public class Query : IRequest<List<Order>>
        { }

        public class Handler : IRequestHandler<Query, List<Order>>
        {
            private readonly ShopDbContext context;

            public Handler(ShopDbContext context)
            {
                this.context = context;
            }

            public async Task<List<Order>> Handle(Query request, CancellationToken cancellationToken)
            {
                return await context.Orders.ToListAsync();
            }
        }
    }
}