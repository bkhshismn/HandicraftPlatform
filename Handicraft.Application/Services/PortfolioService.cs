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

    public async Task<IEnumerable<PortfolioItemDto>> GetAllAsync()
    {
        var items = await _repo.GetAllWithImagesAsync();
        return items.Select(i => _mapper.Map<PortfolioItemDto>(i)).ToList();
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
        var entity = _mapper.Map<PortfolioItem>(dto);

        await _repo.AddAsync(entity);

        var created =
            await _repo.GetByIdAsync(entity.Id);

        return _mapper.Map<PortfolioItemDto>(created!);
    }
    public async Task<PortfolioItemDto?> GetByIdAsync(Guid id)
    {
        var item = await _repo.GetByIdAsync(id);

        if (item is null)
            return null;

        return _mapper.Map<PortfolioItemDto>(item);
    }
}
