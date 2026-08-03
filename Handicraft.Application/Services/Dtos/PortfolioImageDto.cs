using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handicraft.Application.Services.Dtos
{
    public record PortfolioImageDto(
    Guid Id,
    string ImageUrl,
    int Order
);
}
