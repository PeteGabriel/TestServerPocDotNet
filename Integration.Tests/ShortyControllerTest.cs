using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Repository.model;
using Xunit;

namespace Integrate.Tests
{
	public class ShortyControllerTest: IClassFixture<CustomWebApplicationFactory>
	{
		private static string ExpectedContentType = "application/json; charset=utf-8";

		private const string PostUri = "/shorten";

		private const int UnprocessableEntityStatusCode = 422;

		private readonly CustomWebApplicationFactory _factory;
		
		public ShortyControllerTest(CustomWebApplicationFactory factory)
		{
			_factory = factory;
		}


		#region GetByShortCode

		[Fact]
		public async Task SearchForShortenedUrl_WithValidCode_ExpectRedirectResponseWithLocationHeader()
		{
			using (var client = _factory
				.WithWebHostBuilder(_ =>{})
				.CreateClient(new WebApplicationFactoryClientOptions
				{
					AllowAutoRedirect = false
				}))
			{
				var response = await client.GetAsync("/example");
				Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
				var content = await response.Content.ReadAsStringAsync();
				Assert.Empty(content);
				Assert.Equal("www.example.com", response.Headers.Location.ToString());
			}
		}


		[Fact]
		public async Task SearchForShortenedUrl_WithInvalidCode_ExpectNotFoundResponseWithDescription()
		{
			using (var client = _factory
				.WithWebHostBuilder(_ => {})
				.CreateClient(new WebApplicationFactoryClientOptions
				{
					AllowAutoRedirect = false
				}))
			{
				const string code = "this_code_does_not_exist";
				var response = await client.GetAsync($"/{code}");
				Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

				var rawContent = await response.Content.ReadAsStringAsync();
				var content = JsonConvert.DeserializeObject<ErrorResponse>(rawContent);
				Assert.Equal($"The code {code} cannot be found in the system", content.Description);
				Assert.Equal((int)HttpStatusCode.NotFound, content.Error);
				Assert.Equal(ExpectedContentType, response.Content.Headers.ContentType.ToString());
			}
		}

		#endregion

		#region PostNewShortenedUrl

		[Fact]
		public async Task AddShortenedUrl_WithValidModel_WithSuggestedShortCode_ExpectCreatedStatusAndCorrectContentType()
		{
			using (var client = _factory
				.WithWebHostBuilder(_ => {})
				.CreateClient(new WebApplicationFactoryClientOptions
				{
					AllowAutoRedirect = false
				}))
			{
				var request = new ShortUrlRequest
				{
					Url = "www.example_one.com",
					ShortCode = "example_one"
				};

				HttpContent payload = new StringContent(
					JsonConvert.SerializeObject(request),
					Encoding.UTF8,
					"application/json");

				var response = await client.PostAsync(PostUri, payload);
				Assert.Equal(HttpStatusCode.Created, response.StatusCode);
				Assert.Equal("/example_one/stats", response.Headers.Location.ToString());

				var rawContent = await response.Content.ReadAsStringAsync();
				var content = JsonConvert.DeserializeObject<ShortenedUrl>(rawContent);

				Assert.Equal("example_one", content.ShortCode);
				Assert.Null(content.Url);
				Assert.NotNull(content.CreatedOn);
				Assert.Null(content.UpdatedOn);
				Assert.True(content.NumRedirectCalls == 0);
				Assert.Equal(ExpectedContentType, response.Content.Headers.ContentType.ToString());
			}
		}

		[Fact]
		public async Task AddShortenedUrl_WithValidModel_WithoutSuggestedShortCode_ExpectSuccessAndCorrectContentTypeWithGeneratedCode()
		{
			using (var client = _factory
				.WithWebHostBuilder(_ => {})
				.CreateClient(new WebApplicationFactoryClientOptions
				{
					AllowAutoRedirect = false
				}))
			{
				var request = new ShortUrlRequest
				{
					Url = "www.example.com"
				};

				HttpContent payload = new StringContent(
					JsonConvert.SerializeObject(request),
					Encoding.UTF8,
					"application/json");

				var response = await client.PostAsync(PostUri, payload);
				Assert.Equal(HttpStatusCode.Created, response.StatusCode);

				var rawContent = await response.Content.ReadAsStringAsync();
				var content = JsonConvert.DeserializeObject<ShortenedUrl>(rawContent);

				Assert.NotEmpty(content.ShortCode);
				Assert.True(Regex.IsMatch(content.ShortCode, @"^[0-9a-zA-Z_]{6}$"));
				Assert.Null(content.Url);
				Assert.NotNull(content.CreatedOn);
				Assert.Null(content.UpdatedOn);
				Assert.True(content.NumRedirectCalls == 0);
				Assert.Equal(ExpectedContentType, response.Content.Headers.ContentType.ToString());
			}
		}

		[Fact]
		public async Task AddShortenedUrl_WithInvalidModel_ExpectBadRequestStatusAndCorrectContentType()
		{
			using (var client = _factory
				.WithWebHostBuilder(_ => { })
				.CreateClient(new WebApplicationFactoryClientOptions
				{
					AllowAutoRedirect = false
				}))
			{
				var request = new ShortUrlRequest();

				HttpContent payload = new StringContent(
					JsonConvert.SerializeObject(request),
					Encoding.UTF8,
					"application/json");

				var response = await client.PostAsync(PostUri, payload);
				Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

				var rawContent = await response.Content.ReadAsStringAsync();
				var content = JsonConvert.DeserializeObject<ErrorResponse>(rawContent);

				Assert.Equal((int)HttpStatusCode.BadRequest, content.Error);
				Assert.Equal("url is not present", content.Description);
				Assert.Equal(ExpectedContentType, response.Content.Headers.ContentType.ToString());
			}
		}

		[Fact]
		public async Task AddShortenedUrl_WithValidModel_WithSuggestedCodeAlreadyPresent_ExpectConflictStatusAndCorrectContentType()
		{
			using (var client = _factory
				.WithWebHostBuilder(builder => { })
				.CreateClient(new WebApplicationFactoryClientOptions
				{
					AllowAutoRedirect = false
				}))
			{
				var request = new ShortUrlRequest
				{
					Url = "www.youtube.com",
					ShortCode = "Example"
				};

				HttpContent payload = new StringContent(
					JsonConvert.SerializeObject(request),
					Encoding.UTF8,
					"application/json");


				var response = await client.PostAsync(PostUri, payload);

				Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

				var rawContent = await response.Content.ReadAsStringAsync();
				var content = JsonConvert.DeserializeObject<ErrorResponse>(rawContent);

				Assert.Equal((int)HttpStatusCode.Conflict, content.Error);
				Assert.Equal("The the desired shortcode is already in use. Shortcodes are case-sensitive.", content.Description);
				Assert.Equal(ExpectedContentType, response.Content.Headers.ContentType.ToString());
			}
		}

		[Fact]
		public async Task AddShortenedUrl_WithValidModel_WithInvalidSuggestedCode_ExpectUnprocessableStatusAndCorrectContentType()
		{
			using (var client = _factory
				.WithWebHostBuilder(builder => { })
				.CreateClient(new WebApplicationFactoryClientOptions
				{
					AllowAutoRedirect = false
				}))
			{
				var request = new ShortUrlRequest
				{
					Url = "www.youtube.com",
					ShortCode = "e"
				};

				HttpContent payload = new StringContent(
					JsonConvert.SerializeObject(request),
					Encoding.UTF8,
					"application/json");

				var response = await client.PostAsync(PostUri, payload);
				Assert.Equal(UnprocessableEntityStatusCode, (int)response.StatusCode);

				var rawContent = await response.Content.ReadAsStringAsync();
				var content = JsonConvert.DeserializeObject<ErrorResponse>(rawContent);

				Assert.Equal(UnprocessableEntityStatusCode, content.Error);
				Assert.Equal("The shortcode fails to meet the following regexp: ^[0-9a-zA-Z_]{4,}$.", content.Description);
				Assert.Equal(ExpectedContentType, response.Content.Headers.ContentType.ToString());
			}
		}

		#endregion

		#region GetStats

		[Fact]
		public async Task GetStats_WithValidCode_WhithoutPreviousRedirects_ExpectSuccessAndCorrectContentType()
		{
			using (var client = _factory
				.WithWebHostBuilder(builder => { })
				.CreateClient(new WebApplicationFactoryClientOptions
				{
					AllowAutoRedirect = false
				}))
			{
				var response = await client.GetAsync("/example/stats");
				Assert.Equal(HttpStatusCode.OK, response.StatusCode);

				var rawContent = await response.Content.ReadAsStringAsync();
				dynamic content = JsonConvert.DeserializeObject<object>(rawContent);

				Assert.NotNull(content.startDate);
				Assert.Null(content.lastSeenDate);
				Assert.True(content.redirectCount == 0);

				Assert.Equal(ExpectedContentType, response.Content.Headers.ContentType.ToString());
			}
		}
		
		
		[Fact]
		public async Task GetStats_WithValidCode_WhitPreviousRedirects_ExpectSuccessAndCorrectContentType()
		{
			using (var client = _factory
				.WithWebHostBuilder(builder => { })
				.CreateClient(new WebApplicationFactoryClientOptions
				{
					AllowAutoRedirect = false
				}))
			{
				await client.GetAsync("/example");
				var response = await client.GetAsync("/example/stats");
				Assert.Equal(HttpStatusCode.OK, response.StatusCode);

				var rawContent = await response.Content.ReadAsStringAsync();
				dynamic content = JsonConvert.DeserializeObject<object>(rawContent);

				Assert.NotNull(content.startDate);
				Assert.NotNull(content.lastSeenDate);
				Assert.True(content.redirectCount == 1);

				Assert.Equal(ExpectedContentType, response.Content.Headers.ContentType.ToString());
			}
		}

		[Fact]
		public async Task GetStats_WithInvalidCode_ExpectNotFoundAndCorrectContentType()
		{
			using (var client = _factory
				.WithWebHostBuilder(builder => { })
				.CreateClient(new WebApplicationFactoryClientOptions
				{
					AllowAutoRedirect = false
				}))
			{
				const string code = "this_code_does_not_exist";
				var response = await client.GetAsync($"/{code}/stats");
				Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

				var rawContent = await response.Content.ReadAsStringAsync();
				var content = JsonConvert.DeserializeObject<ErrorResponse>(rawContent);

				Assert.Equal(ExpectedContentType, response.Content.Headers.ContentType.ToString());
				Assert.Equal($"The code {code} cannot be found in the system", content.Description);
				Assert.Equal((int)HttpStatusCode.NotFound, content.Error);
			}
		}

		#endregion
	}
}