using AutoMapper;
using Handicraft.Application.Contracts.Persistence;
using Handicraft.Application.Services.Dtos;
using Handicraft.Domain.Entities;
using Handicraft.Application.Contracts.Services;

namespace Handicraft.Application.Services;

public class PortfolioService : IPortfolioService
{
    private readonly IPortfolioItemRepository _repo;
    private readonly IMapper _mapper;

    public PortfolioService(IPortfolioItemRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PortfolioItemDto>> GetPublishedAsync()
    {
        var items = await _repo.GetPublishedItemsAsync();
        return items.Select(i => _mapper.Map<PortfolioItemDto>(i)).ToList();
    }

    public async Task<IEnumerable<PortfolioItemDto>> GetByCategoryAsync(string category)
    {
        var items = await _repo.GetItemsByCategoryAsync(category);
        return items.Select(i => _mapper.Map<PortfolioItemDto>(i)).ToList();
    }

    public async Task<PortfolioItemDto> CreateAsync(PortfolioCreateDto dto)
    {
        var item = _mapper.Map<PortfolioItem>(dto);
        var created = await _repo.AddAsync(item);
        return _mapper.Map<PortfolioItemDto>(created);
    }
}
