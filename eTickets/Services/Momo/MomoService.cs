using System.Threading.Tasks;
using System;
using eTickets.Models.Momo;
using Microsoft.Extensions.Options;
namespace eTickets.Services.Momo;

public class MomoService : IMomoService
{
    private readonly IOptions<MomoOptionModel> _options;
    public MomoService(IOptions<MomoOptionModel> options)
    {
        _options = options;
    }
}
