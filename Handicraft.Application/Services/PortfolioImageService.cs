using AutoMapper;
using Handicraft.Application.Contracts.Persistence;
using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Handicraft.Domain.Entities;

namespace Handicraft.Application.Services;

public sealed class PortfolioImageService
    : IPortfolioImageService
{
    private readonly IPortfolioImageRepository _repository;

    private readonly IMapper _mapper;

    public PortfolioImageService(
        IPortfolioImageRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PortfolioImageDto>>
        GetByPortfolioIdAsync(Guid portfolioId)
    {
        var images =
            await _repository.GetByPortfolioIdAsync(
                portfolioId);

        return images
            .Select(_mapper.Map<PortfolioImageDto>)
            .ToList();
    }

    public async Task<PortfolioImageDto>
        AddAsync(
            Guid portfolioId,
            PortfolioImageCreateDto dto)
    {
        var images =
            await _repository.GetByPortfolioIdAsync(
                portfolioId);

        var image = new PortfolioImage
        {
            PortfolioItemId = portfolioId,
            ImageUrl = dto.ImageUrl,
            Order = images.Count + 1
        };

        var created =
            await _repository.AddAsync(image);

        return _mapper.Map<PortfolioImageDto>(
            created);
    }

    public async Task DeleteAsync(Guid imageId)
    {
        var image =
            await _repository.GetByIdAsync(imageId);

        if (image is null)
            throw new InvalidOperationException(
                "تصویر پیدا نشد.");

        await _repository.DeleteAsync(image);
    }

    public async Task ReorderAsync(
        PortfolioImageReorderDto dto)
    {
        foreach (var item in dto.Images)
        {
            var image =
                await _repository.GetByIdAsync(item.Id);

            if (image is null)
                continue;

            image.Order = item.Order;

            await _repository.UpdateAsync(image);
        }
    }
}