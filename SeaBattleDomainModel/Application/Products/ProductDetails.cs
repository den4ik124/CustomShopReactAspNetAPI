using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using ShopDomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserDomainModel;

namespace Application.Products
{
    public class ProductDetails
    {
        public class Query : IRequest<Product>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Product>
        {
            private readonly ShopDbContext context;

            public Handler(ShopDbContext context)
            {
                this.context = context;
            }

            public async Task<Product> Handle(Query request, CancellationToken cancellationToken)
            {
                var product = await context.Products.FirstOrDefaultAsync(x => x.Id == request.Id);
                return product;
            }
        }
    }
}