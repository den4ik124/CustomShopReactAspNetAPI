using MediatR;
using Persistence;
using ShopDomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Products
{
    public class Edit
    {
        public class Command : IRequest
        {
            public Product Product { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly ShopDbContext context;

            public Handler(ShopDbContext context)
            {
                this.context = context;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var product = await this.context.Products.FindAsync(request.Product.Id);

                //TODO: can be removed by using AutoMapper
                product.Title = request.Product.Title;
                product.Description = request.Product.Description;
                product.Price = request.Product.Price;
                product.Id = request.Product.Id;

                await this.context.SaveChangesAsync();

                return Unit.Value;
            }
        }
    }
}