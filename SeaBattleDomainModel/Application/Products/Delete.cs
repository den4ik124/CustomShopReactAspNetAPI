using MediatR;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Products
{
    public class Delete
    {
        public class Command : IRequest
        {
            public Guid Id { get; set; }
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
                var product = await this.context.Products.FindAsync(request.Id);

                if (product == null) return Unit.Value;

                this.context.Products.Remove(product);

                await this.context.SaveChangesAsync();

                return Unit.Value;
            }
        }
    }
}