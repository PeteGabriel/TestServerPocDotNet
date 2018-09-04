using Repository.model;
using Services.dtos;

namespace Services.mappers
{
    public static class ShortUrlMapper
    {
        public static ShortenedUrlDto MapDtoFromModel(ShortenedUrl obj)
        {
            return new ShortenedUrlDto
            {
                Url = obj.Url,
                ShortCode = obj.ShortCode,
                LastSeenDate = obj.UpdatedOn?.Date,
                StartDate = obj.CreatedOn,
                RedirectCount = obj.NumRedirectCalls
            };
        }
        
        
        public static ShortenedUrl MapModelFromDto(ShortenedUrlDto obj)
        {
            return new ShortenedUrl
            {
                Url = obj.Url,
                ShortCode = obj.ShortCode
            };
        }
    }
}