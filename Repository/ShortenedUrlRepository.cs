using System.Collections.Generic;
using System.Linq;
using Repository.model;

namespace Repository.impl
{
	public class ShortenedUrlRepository : IRepository<ShortenedUrl>
	{
		private readonly AppDbContext context;

		public ShortenedUrlRepository(AppDbContext context)
		{

			this.context = context;
		}

		public IEnumerable<ShortenedUrl> All()
		{
			return context.ShortenedUrls.ToList();
		}

		public ShortenedUrl ByShortcode(string shortCode) => context.ShortenedUrls.Any(obj => obj.ShortCode == shortCode) ?
			context.ShortenedUrls.Find(shortCode) : null;


		public bool TryUpdateByShortcode(string shortCode, ShortenedUrl updatedOne)
		{
			if (context.ShortenedUrls.All(obj => obj.ShortCode != shortCode)) return false;
			context.ShortenedUrls.Update(updatedOne);
			return context.SaveChanges() > 0;
		}


		public bool Add(ShortenedUrl obj)
		{
			context.ShortenedUrls.Add(obj);
			var i = context.SaveChanges();
			return i > 0;
		}
	}
}
