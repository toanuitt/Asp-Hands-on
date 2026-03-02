using AspNetHandons.Entities;
using AspNetHandons.DTOs;
using Riok.Mapperly.Abstractions;

namespace AspNetHandons.Mappers;

[Mapper]
public partial class ProductMapper
{
    public partial ProductDto ProductToProductDto(Product product);

}