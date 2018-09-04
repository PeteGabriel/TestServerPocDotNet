using System.Collections.Generic;

namespace Repository
{
    public interface IRepository<T>
	{
		/// <summary>
		/// Retrieve all objects 
		/// </summary>
		IEnumerable<T> All();

		/// <summary>
		/// Retrieve an object identified by given code 
		/// </summary>
		T ByShortcode(string shortCode);

		/// <summary>
		/// Add new object. Return true if object was added with success,
		/// false otherwise.
		/// </summary>
		bool Add(T obj);

		/// <summary>
		/// Try to update the element with key <paramref name="shortCode"/>.
		/// </summary>
		bool TryUpdateByShortcode(string shortCode, T updatedOne);
	}
}