using System.Collections.Generic;
using System.Threading.Tasks;
using Services.dtos;

namespace Services
{
    public interface IShortenedUrlService
    {
        /// <summary>
        /// Retrieve a shortened url by given code.
        /// </summary>
        Task<ShortenedUrlDto> GetShortenedUrlAsync(string shortCode);
        
        /// <summary>
        /// Retrieve a set of all shortened urls.
        /// </summary>
        Task<IEnumerable<ShortenedUrlDto>> GetAllShortenedUrlsAsync();

        /// <summary>
        /// Add a new shortened url.
        /// Returns a task that represents work in progress.
        /// If successful, evaluate to True, false otherwise.
        /// </summary>
        Task<bool> AddAsync(ShortenedUrlDto obj);

        /// <summary>
        /// Generate a random code with the given length.
        /// </summary>
        string GenerateRandomCode(int sequenceLength = 6);

        /// <summary>
        /// Validate given term against given expression for a set of requirements.
        /// </summary>
        bool IsCodeValid(string term, string expression);

        /// <summary>
        /// Update number of redirect calls for object with key <paramref name="shortcode"/>. 
        /// </summary>
        Task<bool> UpdateRedirectCallsAsync(string shortcode);
    }
}