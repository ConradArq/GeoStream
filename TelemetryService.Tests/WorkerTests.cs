using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TelemetryService.Application.Interfaces;
using TelemetryService.Core.Interfaces;
using TelemetryService.Domain.Models;
using TelemetryService.Infrastructure;
using TelemetryService.Infrastructure.Scanner;

namespace TelemetryService.Tests
{
    public class WorkerTests
    {
        private readonly Mock<ILogger<Worker>> _loggerMock;
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        private readonly Mock<IServiceScope> _scopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IScannerReaderService> _scannerReaderServiceMock;
        private readonly Mock<IEventBus> _busMock;
        private readonly IOptions<ScannerSettings> _scannerSettingsOptions;

        public WorkerTests()
        {
            _loggerMock = new Mock<ILogger<Worker>>();
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _scopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _scannerReaderServiceMock = new Mock<IScannerReaderService>();
            _busMock = new Mock<IEventBus>();
            _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(_scopeMock.Object);
            _scopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceProviderMock.Setup(x => x.GetService(typeof(IScannerReaderService))).Returns(_scannerReaderServiceMock.Object);

            var scannerSettings = new ScannerSettings
            {
                HostConnection = new HostConnection
                {
                    IpAddressHost = "127.0.0.1",
                    PortHost = 30384
                },
                ScannerConfig = new ScannerConfig
                {
                    Code = "Scanner-123",
                    Connection = new Connection
                    {
                        IpAddressReader = "10.2.4.21",
                        PortReader = 1999
                    }
                }
            };

            _scannerSettingsOptions = Options.Create(scannerSettings);
        }

        [Fact]
        public async Task StartAsync_ValidConfiguration_StartsListening()
        {
            // Arrange
            var worker = new Worker(_loggerMock.Object, _scannerSettingsOptions, _scopeFactoryMock.Object, _busMock.Object);
            var cancellationToken = new CancellationToken(false);

            _scannerReaderServiceMock.Setup(x => x.ConnectReader(It.IsAny<ScannerConfig>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(new ScannerConnectionResult { Connected = true });

            _scannerReaderServiceMock
                .Setup(x => x.StartReadingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    // Ensure this method runs async code so that 'worker.StartAsync' can complete,
                    // allowing reading to run in the background without getting stuck in an infinite loop.
                    await Task.Delay(1000);
                });

            // Act
            await worker.StartAsync(cancellationToken);

            // Assert
            _scannerReaderServiceMock.Verify(x => x.ConnectReader(It.IsAny<ScannerConfig>(), It.IsAny<string>(), It.IsAny<int>()), Times.AtLeastOnce);
            _scannerReaderServiceMock.Verify(x => x.StartReadingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task Worker_ShouldCancelExecuteAsyncWhenStopped()
        {
            // Arrange
            var worker = new Worker(_loggerMock.Object, _scannerSettingsOptions, _scopeFactoryMock.Object, _busMock.Object);

            var cancellationTokenSource = new CancellationTokenSource();

            _scannerReaderServiceMock.Setup(x => x.ConnectReader(It.IsAny<ScannerConfig>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(new ScannerConnectionResult { Connected = true });

            _scannerReaderServiceMock
                .Setup(x => x.StartReadingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    // Ensure this method runs async code so that 'worker.StartAsync' can complete,
                    // allowing reading to run in the background without getting stuck in an infinite loop.
                    await Task.Delay(1000);
                });

            // Act 
            await worker.StartAsync(cancellationTokenSource.Token);  // Start the service, which internally starts ExecuteAsync
            cancellationTokenSource.Cancel();  // Signal cancellation to trigger shutdown
            await Task.Delay(100);  // Allow some time for the service to react to the cancellation
            await worker.StopAsync(CancellationToken.None);  // Explicitly stop the service

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() != null && v.ToString()!.Contains("The service is stopping...")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);        
        }

        [Fact]
        public async Task StopAsync_CompletesAllCleanupSuccessfully()
        {
            // Arrange
            var worker = new Worker(_loggerMock.Object, _scannerSettingsOptions, _scopeFactoryMock.Object, _busMock.Object);
            
            var cancellationTokenSource = new CancellationTokenSource();

            _scannerReaderServiceMock.Setup(x => x.ConnectReader(It.IsAny<ScannerConfig>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(new ScannerConnectionResult { Connected = true });

            _scannerReaderServiceMock.Setup(x => x.DisconnectReader(It.IsAny<string>()))
               .Verifiable("The scanner disconnect was not called.");

            _scannerReaderServiceMock
                .Setup(x => x.StartReadingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    // Ensure this method runs async code so that 'worker.StartAsync' can complete,
                    // allowing reading to run in the background without getting stuck in an infinite loop.
                    await Task.Delay(1000);
                });

            // Act 
            await worker.StartAsync(cancellationTokenSource.Token);  // Start the service, which internally starts ExecuteAsync
            cancellationTokenSource.Cancel();  // Signal cancellation to trigger shutdown
            await Task.Delay(100);  // Allow some time for the service to react to the cancellation
            await worker.StopAsync(CancellationToken.None);  // Explicitly stop the service

            // Assert
            _scannerReaderServiceMock.Verify(x => x.DisconnectReader(It.IsAny<string>()), Times.Once, "The scanner should be disconnected exactly once.");

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() != null && v.ToString()!.Contains("The service is stopping...")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() != null && v.ToString()!.Contains($"Scanner '{_scannerSettingsOptions.Value.ScannerConfig.Code}' disconnected successfully.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);      
        }
    }
}