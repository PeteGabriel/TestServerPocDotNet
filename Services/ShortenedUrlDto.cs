using System;

namespace Services.dtos
{
    public class ShortenedUrlDto
    {
        public string Url { get; set; }

        public string ShortCode { get; set; }

        public DateTime StartDate { get; set;} 

        public DateTime? LastSeenDate { get; set; }

        public int RedirectCount { get; set;}
    }
}