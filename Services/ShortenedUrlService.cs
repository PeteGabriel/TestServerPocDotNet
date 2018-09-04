using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Repository;
using Repository.model;
using Services.dtos;
using Services.mappers;

namespace Services
{
    public class ShortenedUrlService :IShortenedUrlService
    {
        private readonly IRepository<ShortenedUrl> repo;
        
        public const string SuggestedCodeValidateExpr = @"^[0-9a-zA-Z_]{4,}$";
        
        public ShortenedUrlService(IRepository<ShortenedUrl> repo)
        {
            this.repo = repo;
        }

        public Task<ShortenedUrlDto> GetShortenedUrlAsync(string shortCode) => Task.Run(() =>
        {
            var obj = repo.ByShortcode(shortCode);
            return obj == null ? null : ShortUrlMapper.MapDtoFromModel(repo.ByShortcode(shortCode));
        });

        public Task<IEnumerable<ShortenedUrlDto>> GetAllShortenedUrlsAsync() 
            => Task.Run(() => repo.All().Select(ShortUrlMapper.MapDtoFromModel));

        public Task<bool> AddAsync(ShortenedUrlDto obj) => Task.Run(() => obj != null && repo.Add(ShortUrlMapper.MapModelFromDto(obj)));


        public string GenerateRandomCode(int sequenceLength = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var stringChars = new char[sequenceLength];

            for (var i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }


        public bool IsCodeValid(string term, string expression) => Regex.IsMatch(term, expression);
        
        
        public Task<bool> UpdateRedirectCallsAsync(string shortcode)
        {
            return Task.Run(() =>
            {
                var instance = repo.ByShortcode(shortcode);
                if (instance != null)
                {
                    instance.NumRedirectCalls += 1;
                    return repo.TryUpdateByShortcode(shortcode, instance);   
                }

                return false;
            });
        }
        
        
    }
}
