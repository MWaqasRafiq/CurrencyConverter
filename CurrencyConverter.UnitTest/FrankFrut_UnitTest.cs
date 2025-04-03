using Moq;
using Moq.Protected;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using CurrencyConverter.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using CurrencyConverter.Application.Models;
using System.Net;
using System.Text;

namespace CurrencyConverter.Tests
{
    public class FrankFrut_UnitTest
    {
        private  Mock<IDistributedCache> _cacheMock;
        private  Mock<IHttpClientFactory> _httpClientFactoryMock;
        private  Mock<ILogger<FrankFrutImplementation>> _loggerMock;
        private  FrankFrutImplementation _service;
      
        [SetUp]
        public void Setup()
        {
            _cacheMock = new Mock<IDistributedCache>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _loggerMock = new Mock<ILogger<FrankFrutImplementation>>();
            _service = new FrankFrutImplementation(_cacheMock.Object, _httpClientFactoryMock.Object, _loggerMock.Object);
         
        }

        [Test]
        public void GetLatestRates_ShouldReturnMockedRates()
        {
            string baseCurrency = "USD";
            var apiResponse = new { rates = new Dictionary<string, decimal> {  { "GBP", 0.75m } } };
            var jsonResponse = JsonSerializer.Serialize(apiResponse);

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var httpClient = new HttpClient(httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://api.frankfurter.app/")
            };

            _httpClientFactoryMock.Setup(f => f.CreateClient("FrankfurterClient")).Returns(httpClient);

            // Act
            var result =  _service.GetLatestRates(baseCurrency).Result;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(apiResponse.rates, result);

        }

        [Test]
        public async Task ConvertCurrency_ShouldReturnMockedExchangeRate()
        {
            // Arrange
            var request = new CurrencyConvertReq
            {
                Amount = 1,
                FromCurrency = "USD",
                ToCurrency = "EUR"
            };

            decimal expectedRate = 0.75m;
            var jsonResponse = "{\"rates\": { \"EUR\": 0.75 }}";

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            };

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            var httpClient = new HttpClient(httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://api.frankfurter.app/")
            };

            _httpClientFactoryMock.Setup(f => f.CreateClient("FrankfurterClient")).Returns(httpClient);

            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((byte[])null);

            _cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

            // Act
            var result = await _service.ConvertCurrency(request).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.EqualTo(expectedRate));
        }

        [Test]
        public async Task ConvertCurrency_ShouldReturnCachedRate_WhenAvailable()
        {
            // Arrange
            var request = new CurrencyConvertReq
            {
                Amount = 1,
                FromCurrency = "USD",
                ToCurrency = "EUR"
            };

            decimal cachedRate = 0.85m; 
            var cachedRateBytes = Encoding.UTF8.GetBytes(cachedRate.ToString());

            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(cachedRateBytes); 

            // Act
            var result = await _service.ConvertCurrency(request).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.EqualTo(cachedRate));

            _httpClientFactoryMock.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
        }


        [Test]
        public void ConvertCurrency_ShouldThrowException_ForExcludedCurrencies()
        {
            // Arrange
            var request = new CurrencyConvertReq
            {
                Amount = 1,
                FromCurrency = "TRY", 
                ToCurrency = "USD"
            };

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.ConvertCurrency(request)
            );

            Assert.That(exception.Message, Is.EqualTo("Conversion involving excluded currencies is not allowed."));
        }


        [Test]
        public void ConvertCurrency_ShouldThrowException_WhenApiFails()
        {
            // Arrange
            var request = new CurrencyConvertReq
            {
                Amount = 1,
                FromCurrency = "USD",
                ToCurrency = "EUR"
            };

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            };

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            var httpClient = new HttpClient(httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://api.frankfurter.app/")
            };

            _httpClientFactoryMock.Setup(f => f.CreateClient("FrankfurterClient")).Returns(httpClient);

            // Act & Assert
            var exception = Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _service.ConvertCurrency(request)
            );

            Assert.That(exception.Message, Is.Not.Empty);
        }

        [Test]
        public async Task ConvertCurrency_ShouldReturnZero_WhenAmountIsZero()
        {
            // Arrange
            var request = new CurrencyConvertReq
            {
                Amount = 0, 
                FromCurrency = "USD",
                ToCurrency = "EUR"
            };

            // Act
            var result = await _service.ConvertCurrency(request).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.EqualTo(0));

            _httpClientFactoryMock.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void GetHistoricalRates_ShouldThrowException_WhenApiFails()
        {
            // Arrange
            string baseCurrency = "USD";
            DateTime startDate = new DateTime(2023, 01, 01);
            DateTime endDate = new DateTime(2023, 01, 10);
            int page = 1;
            int pageSize = 5;

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError 
            };

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var httpClient = new HttpClient(httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://api.frankfurter.app/")
            };

            _httpClientFactoryMock.Setup(f => f.CreateClient("FrankfurterClient")).Returns(httpClient);

            // Act & Assert
            var exception = Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _service.GetHistoricalRates(baseCurrency, startDate, endDate, page, pageSize)
            );

            Assert.That(exception.Message, Is.EqualTo("Response status code does not indicate success: 500 (Internal Server Error)."));
        }


    }
}