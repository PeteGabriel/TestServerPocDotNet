using System;
using System.ComponentModel.DataAnnotations;

namespace Repository.model
{
    /// <summary>
    /// Model class used to represent the urls and their
    /// shortened version. 
    /// </summary>
    public class ShortenedUrl
    {
        public ShortenedUrl()
        {
            CreatedOn = DateTime.UtcNow;
        }

        public string Url { get; set; }

        [Key]
        [Required]
        public string ShortCode { get; set; }

        public readonly DateTime CreatedOn;

        public DateTime? UpdatedOn { get; private set; }

        private int redirectCalls;

        public int NumRedirectCalls
        {
            get => redirectCalls;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Number of calls cannot be negative");
                redirectCalls = value;
                UpdatedOn = (value != 0) ? DateTime.UtcNow : (DateTime?)null;
            }
        }
    }
}