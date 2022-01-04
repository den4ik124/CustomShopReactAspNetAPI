using MediatR;
using Persistence;
using ShopDomainModel.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Orders
{
    public class Create
    {
        public class Command : IRequest
        {
            public IOrder Order { get; set; }
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
                this.context.Add(request.Order);

                await this.context.SaveChangesAsync();

                return Unit.Value;
            }
        }
    }
}