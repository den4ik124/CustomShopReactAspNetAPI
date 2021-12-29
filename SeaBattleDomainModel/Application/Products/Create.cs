using MediatR;
using Persistence;
using ShopDomainModel.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Products
{
    public class Create
    {
        public class Command : IRequest
        {
            public IProduct Product { get; set; }
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
                this.context.Add(request.Product);

                await this.context.SaveChangesAsync();

                return Unit.Value;
            }
        }
    }
}